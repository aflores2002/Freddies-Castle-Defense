using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private int numberOfLanes = 5;
    [SerializeField] private float laneHeight = 1f;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float rightEdgeX = 10f;
    [SerializeField] private float zombieSpeed = 1f;
    [SerializeField] private float leftEdgeX = -10f;

    [SerializeField] private string zombieSortingLayerName = "Zombies";
    [SerializeField] private int zombieOrderInLayer = 101;

    private List<Transform> lanes;
    private bool isSpawning = false;  // Changed to false initially
    private Coroutine spawnCoroutine;
    private WaveManager waveManager;

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
        StartSpawning();  // This will now work since isSpawning is false initially

        Debug.Log("ZombieSpawner: Initialization complete");
    }

    void InitializeLanes()
    {
        Debug.Log("ZombieSpawner: Initializing lanes");
        lanes = new List<Transform>();
        float startY = (numberOfLanes - 1) * laneHeight / 2f;

        for (int i = 0; i < numberOfLanes; i++)
        {
            GameObject lane = new GameObject($"Lane_{i}");
            lane.transform.parent = transform; // Parent to the spawner
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

    IEnumerator SpawnZombies()
    {
        Debug.Log("ZombieSpawner: Starting spawn coroutine");
        while (isSpawning)
        {
            SpawnZombie();
            yield return new WaitForSeconds(spawnInterval);
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

        Debug.Log($"ZombieSpawner: Attempting to spawn zombie at position {spawnPosition}");

        GameObject zombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.Euler(180, 0, 180));
        Debug.Log("ZombieSpawner: Zombie instantiated");

        // Apply Layer Override to all Sprite Renderers
        SpriteRenderer[] renderers = zombie.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.sortingLayerName = zombieSortingLayerName;
            renderer.sortingOrder = zombieOrderInLayer;
        }

        ZombieMovement zombieMovement = zombie.GetComponent<ZombieMovement>() ?? zombie.AddComponent<ZombieMovement>();
        zombieMovement.speed = zombieSpeed;
        zombieMovement.leftEdgeX = leftEdgeX;

        ZombieHealth zombieHealth = zombie.GetComponent<ZombieHealth>() ?? zombie.AddComponent<ZombieHealth>();
        zombieHealth.OnZombieDeath.AddListener(OnZombieKilled);
    }

    private void OnZombieKilled()
    {
        if (waveManager != null)
        {
            waveManager.OnZombieKilled();
        }
    }

    // Add validation in the inspector
    void OnValidate()
    {
        if (spawnInterval <= 0)
            spawnInterval = 1f;

        if (numberOfLanes <= 0)
            numberOfLanes = 1;
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