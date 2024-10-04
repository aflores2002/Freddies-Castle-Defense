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

    private float timeSinceLastSpawn = 0f;
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

        GameObject zombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
        ZombieMovement zombieMovement = zombie.AddComponent<ZombieMovement>();
        zombieMovement.speed = zombieSpeed;

        // Rotate the zombie 180 degrees around both X and Z axes
        zombie.transform.rotation = Quaternion.Euler(180, 0, 180);
    }
}

public class ZombieMovement : MonoBehaviour
{
    public float speed = 1f;

    void Update()
    {
        // Move the zombie to the left
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }
}