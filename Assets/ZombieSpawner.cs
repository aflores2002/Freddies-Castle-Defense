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
    [SerializeField] private float leftEdgeX = -10f; // Add this line

    [SerializeField] private string zombieSortingLayerName = "Zombies";
    [SerializeField] private int zombieOrderInLayer = 101;

    private List<Transform> lanes;

    void Start()
    {
        InitializeLanes();
        StartCoroutine(SpawnZombies());
    }

    void InitializeLanes()
    {
        lanes = new List<Transform>();
        float startY = (numberOfLanes - 1) * laneHeight / 2f;

        for (int i = 0; i < numberOfLanes; i++)
        {
            GameObject lane = new GameObject($"Lane_{i}");
            lane.transform.position = new Vector3(0, startY - i * laneHeight, 0);
            lanes.Add(lane.transform);
        }
    }

    IEnumerator SpawnZombies()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnZombie();
        }
    }

    void SpawnZombie()
    {
        int randomLaneIndex = Random.Range(0, numberOfLanes);
        Vector3 spawnPosition = new Vector3(rightEdgeX, lanes[randomLaneIndex].position.y, 0);

        GameObject zombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.Euler(180, 0, 180));

        // Apply Layer Override to all Sprite Renderers
        SpriteRenderer[] renderers = zombie.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.sortingLayerName = zombieSortingLayerName;
            renderer.sortingOrder = zombieOrderInLayer;
        }

        ZombieMovement zombieMovement = zombie.GetComponent<ZombieMovement>() ?? zombie.AddComponent<ZombieMovement>();
        zombieMovement.speed = zombieSpeed;
        zombieMovement.leftEdgeX = leftEdgeX; // Add this line

        ZombieHealth zombieHealth = zombie.GetComponent<ZombieHealth>() ?? zombie.AddComponent<ZombieHealth>();
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