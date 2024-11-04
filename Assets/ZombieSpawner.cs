// ZombieSpawner.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private int numberOfLanes = 5;
    [SerializeField] private float laneHeight = 1f;
    [SerializeField] private float baseSpawnInterval = 5f;
    [SerializeField] private float baseZombieSpeed = 1f;

    [Header("Boss Settings")]
    [SerializeField] private int bossWaveInterval = 3;
    [SerializeField] private float bossScale = 2.5f;
    [SerializeField] private int bossHealth = 1000;
    [SerializeField] private float bossSpeed = 0.5f;
    [SerializeField] private Color bossColor = Color.red;

    [Header("Difficulty Settings")]
    [SerializeField] private float spawnIntervalDecrease = 0.5f;
    [SerializeField] private float speedIncrease = 0.1f;
    [SerializeField] private float minimumSpawnInterval = 1f;
    [SerializeField] private int baseZombieHealth = 100;
    [SerializeField] private int healthIncreasePerWave = 20;

    [Header("Position Settings")]
    [SerializeField] private float rightEdgeX = 10f;
    [SerializeField] private float leftEdgeX = -10f;

    [Header("Visual Settings")]
    [SerializeField] private string zombieSortingLayerName = "Zombies";
    [SerializeField] private int zombieOrderInLayer = 101;

    private List<Transform> lanes;
    private bool isSpawning = false;
    private Coroutine spawnCoroutine;
    private WaveManager waveManager;

    private float currentSpawnInterval;
    private float currentZombieSpeed;
    private int currentZombieHealth;
    private int currentWave = 1;
    private bool isBossWave = false;

    void Start()
    {
        Debug.Log("ZombieSpawner: Starting initialization");

        if (zombiePrefab == null)
        {
            Debug.LogError("ZombieSpawner: Zombie prefab is not assigned!");
            return;
        }

        InitializeLanes();
        waveManager = FindObjectOfType<WaveManager>();
        ResetDifficultyToBase();
        StartSpawning();

        Debug.Log("ZombieSpawner: Initialization complete");
    }

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

    private bool hasBossSpawned = false; // New field to track if boss has spawned this wave

    public void IncreaseDifficulty(int waveNumber)
    {
        currentWave = waveNumber;
        hasBossSpawned = false; // Reset boss spawn flag for new wave

        // Check if this is a boss wave
        isBossWave = (currentWave % bossWaveInterval == 0);

        // Normal wave difficulty progression (applies to all waves)
        currentSpawnInterval = Mathf.Max(minimumSpawnInterval, currentSpawnInterval - spawnIntervalDecrease);
        currentZombieSpeed += speedIncrease;
        currentZombieHealth = baseZombieHealth + (healthIncreasePerWave * (currentWave - 1));

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

    public void StartSpawning()
    {
        Debug.Log("ZombieSpawner: StartSpawning called");
        if (!isSpawning)
        {
            isSpawning = true;

            // If it's a boss wave, spawn the boss immediately
            if (isBossWave && !hasBossSpawned)
            {
                SpawnBossZombie();
            }

            spawnCoroutine = StartCoroutine(SpawnZombies());
            Debug.Log("ZombieSpawner: Spawning started");
        }
    }

    public void StopSpawning()
    {
        Debug.Log("ZombieSpawner: StopSpawning called");
        if (isSpawning)
        {
            isSpawning = false;
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
            Debug.Log("ZombieSpawner: Spawning stopped");
        }
    }

    public void DestroyAllZombies()
    {
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
        foreach (GameObject zombie in zombies)
        {
            Destroy(zombie);
        }
        Debug.Log($"Destroyed {zombies.Length} zombies");
    }

    IEnumerator SpawnZombies()
    {
        Debug.Log("ZombieSpawner: Starting spawn coroutine");
        while (isSpawning)
        {
            SpawnZombie();
            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    private void OnZombieKilled()
    {
        if (waveManager != null)
        {
            waveManager.OnZombieKilled();
        }
    }

    void SpawnZombie()
    {
        if (zombiePrefab == null) return;

        int randomLaneIndex = Random.Range(0, numberOfLanes);
        Vector3 spawnPosition = new Vector3(rightEdgeX, lanes[randomLaneIndex].position.y, 0);

        GameObject zombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.Euler(180, 0, 180));
        zombie.tag = "Zombie";

        // Always configure as normal zombie in regular spawn cycle
        ConfigureNormalZombie(zombie);
    }

    void SpawnBossZombie()
    {
        if (zombiePrefab == null) return;

        // Spawn boss in middle lane
        int middleLaneIndex = numberOfLanes / 2;
        Vector3 spawnPosition = new Vector3(rightEdgeX, lanes[middleLaneIndex].position.y, 0);

        GameObject bossZombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.Euler(180, 0, 180));
        bossZombie.tag = "Zombie";
        ConfigureBossZombie(bossZombie);

        hasBossSpawned = true;
        Debug.Log("Boss zombie spawned for this wave!");
    }

    private void ConfigureBossZombie(GameObject zombie)
    {
        // Scale up the boss
        zombie.transform.localScale *= bossScale;

        // Set up renderers with boss visuals
        SpriteRenderer[] renderers = zombie.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.sortingLayerName = zombieSortingLayerName;
            renderer.sortingOrder = zombieOrderInLayer + 1;
            renderer.color = bossColor;
        }

        // Set up boss movement
        ZombieMovement zombieMovement = zombie.GetComponent<ZombieMovement>() ?? zombie.AddComponent<ZombieMovement>();
        zombieMovement.speed = bossSpeed;
        zombieMovement.leftEdgeX = leftEdgeX;

        // Set up boss health
        ZombieHealth zombieHealth = zombie.GetComponent<ZombieHealth>() ?? zombie.AddComponent<ZombieHealth>();
        zombieHealth.SetMaxHealth(bossHealth);
        zombieHealth.IsBoss = true;
        zombieHealth.OnZombieDeath.AddListener(OnZombieKilled);

        Debug.Log($"Spawned BOSS zombie with Health: {bossHealth}, Speed: {bossSpeed:F2}");
    }

    private void ConfigureNormalZombie(GameObject zombie)
    {
        SpriteRenderer[] renderers = zombie.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.sortingLayerName = zombieSortingLayerName;
            renderer.sortingOrder = zombieOrderInLayer;
        }

        ZombieMovement zombieMovement = zombie.GetComponent<ZombieMovement>() ?? zombie.AddComponent<ZombieMovement>();
        zombieMovement.speed = currentZombieSpeed;
        zombieMovement.leftEdgeX = leftEdgeX;

        ZombieHealth zombieHealth = zombie.GetComponent<ZombieHealth>() ?? zombie.AddComponent<ZombieHealth>();
        zombieHealth.SetMaxHealth(currentZombieHealth);
        zombieHealth.IsBoss = false;
        zombieHealth.OnZombieDeath.AddListener(OnZombieKilled);

        Debug.Log($"Spawned normal zombie with Health: {currentZombieHealth}, Speed: {currentZombieSpeed:F2}");
    }
}

public class ZombieMovement : MonoBehaviour
{
    public float speed = 1f;
    public float leftEdgeX = -10f;
    public int damageToCastle = 20;
    private CastleHealth castleHealth;
    private bool hasDamagedCastle = false;

    void Start()
    {
        castleHealth = FindObjectOfType<CastleHealth>();

        // Setup colliders
        SetupColliders();
    }

    private void SetupColliders()
    {
        // Add a BoxCollider2D for physical collisions if it doesn't exist
        BoxCollider2D physicsCollider = GetComponent<BoxCollider2D>();
        if (physicsCollider == null)
        {
            physicsCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        physicsCollider.isTrigger = false; // Make sure it's not a trigger

        // Add a second BoxCollider2D for trigger detection
        BoxCollider2D triggerCollider = gameObject.AddComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = physicsCollider.size * 1.1f; // Make trigger slightly larger
    }

    void Update()
    {
        // Move the zombie to the left
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Handle collision with castle
        if (!hasDamagedCastle && collision.gameObject.CompareTag("Castle"))
        {
            if (castleHealth != null)
            {
                castleHealth.TakeDamage(damageToCastle);
                hasDamagedCastle = true;
                Debug.Log($"Zombie collided with castle! Dealing {damageToCastle} damage");
                Destroy(gameObject);
            }
        }
    }
}