using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Attributes")]
    [SerializeField] private int baseEnemies = 10;
    [SerializeField] private float enemiesPerSecond = 0.5f;
    [SerializeField] private float timeBetweenWaves = 2f;
    [SerializeField] private float difficultyScalingFactor = 0.75f;

    [Header("Events")]
    public static UnityEvent onEnemyDestroy = new();



    private int currentWave = 0;
    private float timeSinceLastSpawn;
    private int enemiesAlive;
    private int enemiesLeftToSpawn;
    private bool isSpawning = false;



    private void Awake()
    {
        onEnemyDestroy.AddListener(EnemyDestroyed);
    }


    // Start is called before the first frame update
    public void SendWave()
    {
        if (!isSpawning)
        {
            Debug.Log("SendWave called in GameManager, starting new wave.");
            Debug.Log($"The wave is now: {currentWave}");
            isSpawning = true;
            StartCoroutine(StartWave());
        }
    }



    // Update is called once per frame
    private void Update()
    {
        if (!isSpawning)
        {
           // Debug.Log("Spawning is not enabled.");
            return;
        }
        timeSinceLastSpawn += Time.deltaTime;

        if (timeSinceLastSpawn >= (1f / enemiesPerSecond) && enemiesLeftToSpawn > 0)
        {
            SpawnEnemy();
            enemiesLeftToSpawn--;
            enemiesAlive++;
            timeSinceLastSpawn = 0f;
        }

        if (enemiesAlive == 0 && enemiesLeftToSpawn == 0)
        {
            EndWave();
        }
    }

    private void EnemyDestroyed()
    {
        enemiesAlive--;
        Debug.Log($"Enemy killed There are now: {enemiesAlive}");
    }

    private IEnumerator StartWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        currentWave++;
        isSpawning = true;
        enemiesLeftToSpawn = EnemiesPerWave();
        Debug.Log($"Enemies to spawn: {enemiesLeftToSpawn}");

    }

    private void EndWave()
    {
        isSpawning = false;
        timeSinceLastSpawn = 0f;
    }

    private void SpawnEnemy()
    {
        GameObject prefabToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(prefabToSpawn, randomSpawnPoint.position, Quaternion.identity);
        Debug.Log("Spawned enemy at " + randomSpawnPoint.position);
    }

    private int EnemiesPerWave()
    {
        return Mathf.RoundToInt(baseEnemies * Mathf.Pow(currentWave, difficultyScalingFactor));
    }

}
