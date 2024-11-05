using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ZombieSpawner manages zombie spawning, wave difficulty, and boss encounters
public class ZombieSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject zombiePrefab;      // Prefab for spawning zombies
    [SerializeField] private int numberOfLanes = 5;        // Number of vertical lanes
    [SerializeField] private float laneHeight = 1f;        // Vertical spacing between lanes
    [SerializeField] private float baseSpawnInterval = 5f; // Starting time between spawns
    [SerializeField] private float baseZombieSpeed = 1f;   // Starting zombie movement speed

    [Header("Boss Settings")]
    [SerializeField] private int bossWaveInterval = 3;    // Waves between boss spawns
    [SerializeField] private float bossScale = 2.5f;      // Size multiplier for boss
    [SerializeField] private int bossHealth = 1000;       // Boss health points
    [SerializeField] private float bossSpeed = 0.5f;      // Boss movement speed
    [SerializeField] private Color bossColor = Color.red; // Boss tint color

    [Header("Difficulty Settings")]
    [SerializeField] private float spawnIntervalDecrease = 0.5f; // Spawn speed increase per wave
    [SerializeField] private float speedIncrease = 0.1f;         // Movement speed increase per wave
    [SerializeField] private float minimumSpawnInterval = 1f;    // Max spawn rate
    [SerializeField] private int baseZombieHealth = 100;         // Starting zombie health
    [SerializeField] private int healthIncreasePerWave = 20;     // Health increased per wave

    [Header("Position Settings")]
    [SerializeField] private float rightEdgeX = 10f; // Spawn position
    [SerializeField] private float leftEdgeX = -10f; // Castle position

    [Header("Visual Settings")]
    [SerializeField] private int zombieOrderInLayer = 101; // Base sorting order

    // Internal state tracking
    private List<Transform> lanes;    // List of lane positions
    private bool isSpawning = false;  // Current spawning state
    private Coroutine spawnCoroutine; // Reference to spawn coroutine
    private WaveManager waveManager;  // Reference to wave manager

    // Current wave properties
    private float currentSpawnInterval;  // Current time between spawns
    private float currentZombieSpeed;    // Current zombie speed
    private int currentZombieHealth;     // Current zombie health
    private int currentWave = 1;         // Current wave number
    private bool isBossWave = false;     // Whether current wave is a boss wave
    private bool hasBossSpawned = false; // Whether boss has spawned this wave

    // Additional sorting order settings
    [Header("Sorting Settings")]
    [SerializeField] private string zombieSortingLayerName = "Zombies";
    // Base sorting order for zombies in the top lane
    [SerializeField] private int baseTopLaneSortingOrder = 200;
    // Amount to decrease sorting order per lane (lower lanes appear behind higher lanes)
    [SerializeField] private int sortingOrderPerLaneDecrease = 20;
    // Additional sorting boost for boss zombie
    [SerializeField] private int bossExtraSortingOrder = 10;

    // Track boss state
    private bool hasBossActive = false;
    private GameObject currentBossZombie;

    void Start()
    {
        Debug.Log("ZombieSpawner: Starting initialization");

        // Validate required prefab
        if (zombiePrefab == null)
        {
            Debug.LogError("ZombieSpawner: Zombie prefab is not assigned!");
            return;
        }

        // Initialize systems
        InitializeLanes();
        waveManager = FindObjectOfType<WaveManager>();
        ResetDifficultyToBase();
    }

    // Reset all difficulty parameters to starting values
    public void ResetDifficultyToBase()
    {
        currentSpawnInterval = baseSpawnInterval;
        currentZombieSpeed = baseZombieSpeed;
        currentZombieHealth = baseZombieHealth;
        currentWave = 1;
        isBossWave = false;
        hasBossSpawned = false;

        Debug.Log($"Reset to base difficulty - Interval: {currentSpawnInterval}, Speed: {currentZombieSpeed}, Health: {currentZombieHealth}");
    }

    // Increase difficulty for new wave
    public void IncreaseDifficulty(int waveNumber)
    {
        currentWave = waveNumber;
        hasBossSpawned = false;
        hasBossActive = false;
        currentBossZombie = null;

        // Determine if this is a boss wave
        isBossWave = (currentWave % bossWaveInterval == 0);

        // Update difficulty parameters
        currentSpawnInterval = Mathf.Max(minimumSpawnInterval, currentSpawnInterval - spawnIntervalDecrease);
        currentZombieSpeed += speedIncrease;
        currentZombieHealth = baseZombieHealth + (healthIncreasePerWave * (currentWave - 1));

        // Log appropriate message
        if (isBossWave)
        {
            Debug.Log($"Wave {currentWave} is a BOSS wave! Regular zombies will have - " +
                     $"Interval: {currentSpawnInterval:F2}, Speed: {currentZombieSpeed:F2}, Health: {currentZombieHealth}");
        }
        else
        {
            Debug.Log($"Wave {currentWave} Difficulty - Interval: {currentSpawnInterval:F2}, " +
                     $"Speed: {currentZombieSpeed:F2}, Health: {currentZombieHealth}");
        }
    }

    // Create and position lanes for zombie paths
    void InitializeLanes()
    {
        Debug.Log("ZombieSpawner: Initializing lanes");
        lanes = new List<Transform>();
        float startY = (numberOfLanes - 1) * laneHeight / 2f;

        for (int i = 0; i < numberOfLanes; i++)
        {
            GameObject lane = new GameObject($"Lane_{i}");
            lane.transform.parent = transform;
            lane.transform.position = new Vector3(0, startY - i * laneHeight, 0);
            lanes.Add(lane.transform);
        }
        Debug.Log($"ZombieSpawner: Created {numberOfLanes} lanes");
    }

    // Begin spawning zombies
    public void StartSpawning()
    {
        Debug.Log("ZombieSpawner: StartSpawning called");
        if (!isSpawning)
        {
            isSpawning = true;

            // Spawn boss immediately if it's a boss wave
            if (isBossWave && !hasBossSpawned)
            {
                SpawnBossZombie();
            }

            spawnCoroutine = StartCoroutine(SpawnZombies());
            Debug.Log("ZombieSpawner: Spawning started");
        }
    }

    // Stop spawning zombies
    public void StopSpawning()
    {
        Debug.Log("ZombieSpawner: StopSpawning called");
        if (isSpawning && spawnCoroutine != null)
        {
            isSpawning = false;
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
            Debug.Log("ZombieSpawner: Spawning stopped");
        }
    }

    // Remove all existing zombies
    public void DestroyAllZombies()
    {
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
        foreach (GameObject zombie in zombies)
        {
            Destroy(zombie);
        }
        hasBossActive = false;
        currentBossZombie = null;
        Debug.Log($"Destroyed {zombies.Length} zombies and reset boss state");
    }

    // Coroutine for spawning zombies at intervals
    IEnumerator SpawnZombies()
    {
        Debug.Log("ZombieSpawner: Starting spawn coroutine");
        while (isSpawning)
        {
            SpawnZombie();
            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    // Handle zombie death event
    private void OnZombieKilled()
    {
        if (waveManager != null)
        {
            waveManager.OnZombieKilled();
        }
    }

    // Spawn regular zombie
    void SpawnZombie()
    {
        if (zombiePrefab == null) return;

        // If boss exists, only use lanes below it
        int startLane = hasBossActive ? 1 : 0; // Skip top lane if boss exists

        if (startLane >= numberOfLanes)
        {
            Debug.LogWarning("No available lanes for zombie spawn");
            return;
        }

        // Choose random lane from available lanes
        int randomLaneIndex = Random.Range(startLane, numberOfLanes);
        Vector3 spawnPosition = new Vector3(rightEdgeX, lanes[randomLaneIndex].position.y, 0);

        // Create and configure zombie
        GameObject zombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.Euler(180, 0, 180));
        zombie.tag = "Zombie";
        ConfigureNormalZombie(zombie, randomLaneIndex);
    }

    // Spawn boss zombie
    void SpawnBossZombie()
    {
        if (zombiePrefab == null) return;

        // Always spawn boss in top lane (index 0)
        Vector3 spawnPosition = new Vector3(rightEdgeX, lanes[0].position.y, 0);

        // Create and configure boss
        GameObject bossZombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.Euler(180, 0, 180));
        bossZombie.tag = "Zombie";
        ConfigureBossZombie(bossZombie);

        hasBossActive = true;
        currentBossZombie = bossZombie;
        hasBossSpawned = true;
        Debug.Log("Boss zombie spawned in top lane");
    }

    // Configure boss zombie properties
    private void ConfigureBossZombie(GameObject zombie)
    {
        // Scale up the boss
        zombie.transform.localScale *= bossScale;

        // Calculate boss sorting order (top lane + extra boost)
        int bossSortingOrder = baseTopLaneSortingOrder + bossExtraSortingOrder;

        // Set up visual properties
        SpriteRenderer[] renderers = zombie.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.sortingLayerName = zombieSortingLayerName;
            renderer.sortingOrder = bossSortingOrder;
            renderer.color = bossColor;
        }

        // Configure movement
        ZombieMovement zombieMovement = zombie.GetComponent<ZombieMovement>() ?? zombie.AddComponent<ZombieMovement>();
        zombieMovement.speed = bossSpeed;
        zombieMovement.leftEdgeX = leftEdgeX;

        // Configure health with boss death handler
        ZombieHealth zombieHealth = zombie.GetComponent<ZombieHealth>() ?? zombie.AddComponent<ZombieHealth>();
        zombieHealth.SetMaxHealth(bossHealth);
        zombieHealth.IsBoss = true;

        // Add listener for boss death
        zombieHealth.OnZombieDeath.AddListener(() => OnBossZombieKilled(zombie));
        zombieHealth.OnZombieDeath.AddListener(OnZombieKilled);

        Debug.Log($"Spawned BOSS zombie with Health: {bossHealth}, Speed: {bossSpeed:F2}, SortingOrder: {bossSortingOrder}");
    }

    // Configure normal zombie properties
    private void ConfigureNormalZombie(GameObject zombie, int laneIndex)
    {
        // Calculate sorting order based on lane
        // Lower lanes get lower sorting order (appear behind higher lanes)
        int zombieSortingOrder = baseTopLaneSortingOrder - (laneIndex * sortingOrderPerLaneDecrease);

        // Set up visual properties
        SpriteRenderer[] renderers = zombie.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.sortingLayerName = zombieSortingLayerName;
            renderer.sortingOrder = zombieSortingOrder;
        }

        // Configure movement
        ZombieMovement zombieMovement = zombie.GetComponent<ZombieMovement>() ?? zombie.AddComponent<ZombieMovement>();
        zombieMovement.speed = currentZombieSpeed;
        zombieMovement.leftEdgeX = leftEdgeX;

        // Configure health
        ZombieHealth zombieHealth = zombie.GetComponent<ZombieHealth>() ?? zombie.AddComponent<ZombieHealth>();
        zombieHealth.SetMaxHealth(currentZombieHealth);
        zombieHealth.IsBoss = false;
        zombieHealth.OnZombieDeath.AddListener(OnZombieKilled);

        Debug.Log($"Spawned normal zombie in lane {laneIndex} with Health: {currentZombieHealth}, " +
                 $"Speed: {currentZombieSpeed:F2}, SortingOrder: {zombieSortingOrder}");
    }

    // Method to handle boss death and free up lane
    private void OnBossZombieKilled(GameObject bossZombie)
    {
        hasBossActive = false;
        currentBossZombie = null;
        Debug.Log("Boss killed, all lanes now available");
    }
}

// ZombieMovement handles zombie movement and castle collision detection
public class ZombieMovement : MonoBehaviour
{
    // Movement and collision properties
    public float speed = 1f;                // Movement speed
    public float leftEdgeX = -10f;          // Castle position point
    public int damageToCastle = 20;         // Damage dealt to castle
    private CastleHealth castleHealth;      // Reference to castle health
    private bool hasAttackedCastle = false; // Track if castle was attacked
    private Rigidbody2D rb2d;               // Physics component

    void Start()
    {
        // Get reference to castle
        castleHealth = FindObjectOfType<CastleHealth>();

        // Setup physics
        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d == null)
        {
            rb2d = gameObject.AddComponent<Rigidbody2D>();
        }
        rb2d.isKinematic = true; // Use kinematic body for controlled movement
        rb2d.gravityScale = 0;   // Disable gravity

        // Setup knight collision (not a trigger)
        CapsuleCollider2D mainCollider = GetComponent<CapsuleCollider2D>();
        if (mainCollider != null)
        {
            mainCollider.isTrigger = false;
        }

        // Setup castle collision detection (is a trigger)
        BoxCollider2D triggerCollider = gameObject.AddComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector2(0.5f, 1f);  // Smaller trigger area
        triggerCollider.offset = mainCollider != null ? mainCollider.offset : Vector2.zero;

        Debug.Log("Zombie initialized with dual colliders");
    }

    void Update()
    {
        // Move zombie leftwards
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // Fallback check for castle collision
        if (transform.position.x <= leftEdgeX && !hasAttackedCastle)
        {
            if (castleHealth != null)
            {
                Debug.Log("Zombie reached leftEdgeX, damaging castle");
                castleHealth.TakeDamage(damageToCastle);
                hasAttackedCastle = true;
                Destroy(gameObject);
            }
        }
    }

    // Handle trigger collision with castle
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasAttackedCastle && other.CompareTag("Castle"))
        {
            Debug.Log("Castle collision detected!");
            if (castleHealth != null)
            {
                castleHealth.TakeDamage(damageToCastle);
                hasAttackedCastle = true;
                Destroy(gameObject);
            }
        }
    }
}