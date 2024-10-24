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
    }

    public void IncreaseDifficulty(int waveNumber)
    {
        currentWave = waveNumber;

        // Decrease spawn interval but don't go below minimum
        currentSpawnInterval = Mathf.Max(minimumSpawnInterval, currentSpawnInterval - spawnIntervalDecrease);
        currentZombieSpeed += speedIncrease;
        currentZombieHealth = baseZombieHealth + (healthIncreasePerWave * (currentWave - 1));

        Debug.Log($"Wave {currentWave} Difficulty - Interval: {currentSpawnInterval:F2}, " +
                 $"Speed: {currentZombieSpeed:F2}, Health: {currentZombieHealth}");
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
        // Find all zombies in the scene
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

    void SpawnZombie()
    {
        if (zombiePrefab == null)
        {
            Debug.LogError("ZombieSpawner: Cannot spawn zombie - prefab is null!");
            return;
        }

        int randomLaneIndex = Random.Range(0, numberOfLanes);
        Vector3 spawnPosition = new Vector3(rightEdgeX, lanes[randomLaneIndex].position.y, 0);

        GameObject zombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.Euler(180, 0, 180));
        zombie.tag = "Zombie";

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
        zombieHealth.SetMaxHealth(currentZombieHealth); // Set the scaled health
        zombieHealth.OnZombieDeath.AddListener(OnZombieKilled);

        Debug.Log($"Spawned zombie with Health: {currentZombieHealth}, Speed: {currentZombieSpeed:F2}");
    }

    private void OnZombieKilled()
    {
        if (waveManager != null)
        {
            waveManager.OnZombieKilled();
        }
    }

    void OnValidate()
    {
        if (baseSpawnInterval < minimumSpawnInterval)
            baseSpawnInterval = minimumSpawnInterval;

        if (spawnIntervalDecrease < 0)
            spawnIntervalDecrease = 0;

        if (speedIncrease < 0)
            speedIncrease = 0;
    }
}

public class ZombieMovement : MonoBehaviour
{
    public float speed = 1f;
    public float leftEdgeX = -10f;
        public int damageToCastle = 20; // Variable for configurable damage
    private CastleHealth castleHealth;

    void Start()
    {
        castleHealth = FindObjectOfType<CastleHealth>();
    }

    void Update()
    {
        // Move the zombie to the left
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // Check if the zombie has reached the left edge of the screen
        if (transform.position.x <= leftEdgeX)
        {
            // Damage the castle
            if (castleHealth != null)
            {
                castleHealth.TakeDamage(damageToCastle);
            }

            // Destroy the zombie
            Destroy(gameObject);
        }
    }
}