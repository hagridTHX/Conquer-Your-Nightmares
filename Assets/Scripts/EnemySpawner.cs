using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Referencje")]
    public Transform player;
    public List<GameObject> normalEnemyPrefabs; 

    [Header("Ustawienia Zwykłego Spawnu")]
    public float spawnRadius = 25f;
    public float baseSpawnRate = 2f;
    public float minSpawnRate = 0.2f;

    [Header("Ustawienia Chmary (Swarm)")]
    public GameObject swarmPrefab;
    public float swarmSpawnRate = 15f; 
    public int minSwarmSize = 10;
    public int maxSwarmSize = 20;

    private float nextSpawnTime;
    private float nextSwarmTime;
    
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if (GameManager.Instance.currentState != GameManager.GameState.Playing) return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnNormalEnemy();
            float currentMultiplier = GameManager.Instance.GetCurrentDifficultyMultiplier();
            float currentSpawnRate = Mathf.Max(baseSpawnRate / currentMultiplier, minSpawnRate);
            nextSpawnTime = Time.time + currentSpawnRate;
        }

        if (swarmPrefab != null && Time.time >= nextSwarmTime)
        {
            SpawnSwarmGroup();
            float currentMultiplier = GameManager.Instance.GetCurrentDifficultyMultiplier();
            nextSwarmTime = Time.time + Mathf.Max(swarmSpawnRate / currentMultiplier, 5f);
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        if (player == null || mainCam == null) return Vector3.zero;

        float currentRadius = spawnRadius;
        Vector3 bestPos = player.position;
        
        for (int i = 0; i < 20; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized * currentRadius;
            Vector3 testPos = player.position + new Vector3(randomCircle.x, 2.0f, randomCircle.y);

            Vector3 viewportPos = mainCam.WorldToViewportPoint(testPos);
            float margin = 0.15f;
            
            bool isVisible = viewportPos.z > 0 && 
                             viewportPos.x > -margin && viewportPos.x < 1 + margin && 
                             viewportPos.y > -margin && viewportPos.y < 1 + margin;

            if (!isVisible)
            {
                return testPos;
            }
            
            currentRadius += 5f; 
            bestPos = testPos;
        }

        return bestPos; 
    }

    void SpawnNormalEnemy()
    {
        if (player == null || normalEnemyPrefabs.Count == 0) return;

        Vector3 spawnPos = GetValidSpawnPosition();

        GameObject prefabToSpawn = normalEnemyPrefabs[Random.Range(0, normalEnemyPrefabs.Count)];
        GameObject newEnemy = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        Enemy enemyScript = newEnemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            float diffMult = GameManager.Instance.GetCurrentDifficultyMultiplier();
            enemyScript.UpgradeStats(healthMultiplier: diffMult, damageMultiplier: diffMult, speedMultiplier: 1f + (diffMult * 0.05f));
        }
    }

    void SpawnSwarmGroup()
    {
        if (player == null || swarmPrefab == null) return;

        Vector3 groupCenterPos = GetValidSpawnPosition();

        int swarmSize = Random.Range(minSwarmSize, maxSwarmSize + 1);
        float diffMult = GameManager.Instance.GetCurrentDifficultyMultiplier();

        for (int i = 0; i < swarmSize; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * 3f; 
            Vector3 spawnPos = groupCenterPos + new Vector3(randomOffset.x, 0, randomOffset.y);

            GameObject newEnemy = Instantiate(swarmPrefab, spawnPos, Quaternion.identity);
            Enemy enemyScript = newEnemy.GetComponent<Enemy>();
            
            if (enemyScript != null)
            {
                enemyScript.UpgradeStats(healthMultiplier: diffMult * 0.5f, damageMultiplier: diffMult, speedMultiplier: 1f + (diffMult * 0.05f));
            }
        }
    }
}