using UnityEngine;
using System.Collections.Generic;

public class ObstacleManager : MonoBehaviour
{
    public Transform player;
    public GameObject[] obstaclePrefabs;
    
    [Header("Ustawienia Zagęszczenia")]
    [Tooltip("Główne zagęszczenie: Ile przeszkód ma otaczać kamerę w danej chwili.")]
    [Range(10, 300)] public int maxObstacles = 75;

    [Tooltip("Minimalny odstęp między przeszkodami, żeby nie wchodziły jedna w drugą.")]
    [Range(1f, 10f)] public float minSpacing = 3.0f;

    [Header("Obszar Generowania")]
    public float spawnRadius = 35f;
    public float despawnRadius = 50f;

    private List<GameObject> activeObstacles = new List<GameObject>();
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        Vector3 currentCenter = GetCameraCenter();

        int attempts = 0;
        while (activeObstacles.Count < maxObstacles && attempts < maxObstacles * 3)
        {
            Vector2 randomPos = Random.insideUnitCircle * spawnRadius;
            Vector3 testPos = currentCenter + new Vector3(randomPos.x, 0, randomPos.y);
            
            if (IsSpotClear(testPos))
            {
                SpawnObstacleAt(testPos);
            }
            attempts++;
        }
    }

    void Update()
    {
        if (player == null || obstaclePrefabs.Length == 0 || mainCam == null) return;

        Vector3 currentCenter = GetCameraCenter();

        //usuwanie odleglych przeszkod
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (activeObstacles[i] == null) continue;

            bool isFarFromCamera = Vector3.Distance(activeObstacles[i].transform.position, currentCenter) > despawnRadius;
            bool isFarFromPlayer = Vector3.Distance(activeObstacles[i].transform.position, player.position) > despawnRadius;

            if (isFarFromCamera && isFarFromPlayer)
            {
                Destroy(activeObstacles[i]);
                activeObstacles.RemoveAt(i);
            }
        }

        int localObstacles = 0;
        foreach (GameObject obs in activeObstacles)
        {
            if (obs != null && Vector3.Distance(obs.transform.position, currentCenter) <= despawnRadius)
            {
                localObstacles++;
            }
        }

        //tworzenie nowych przeszkod
        int loopFailsafe = 0;
        while (localObstacles < maxObstacles && loopFailsafe < 20)
        {
            Vector3 spawnPos = GetValidOffscreenPosition(currentCenter);
            
            if (IsSpotClear(spawnPos))
            {
                SpawnObstacleAt(spawnPos);
                localObstacles++;
            }
            loopFailsafe++;
        }
    }

    bool IsSpotClear(Vector3 pos)
    {
        foreach (GameObject obs in activeObstacles)
        {
            if (obs != null)
            {
                if (Vector3.Distance(obs.transform.position, pos) < minSpacing)
                {
                    return false;
                }
            }
        }
        return true;
    }

    Vector3 GetCameraCenter()
    {
        if (mainCam == null) return Vector3.zero;
        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    Vector3 GetValidOffscreenPosition(Vector3 centerPoint)
    {
        Vector3 bestPos = centerPoint;
        for (int i = 0; i < 15; i++)
        {
            Vector2 randomEdge = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 testPos = centerPoint + new Vector3(randomEdge.x, 0, randomEdge.y);

            Vector3 viewportPos = mainCam.WorldToViewportPoint(testPos);
            float margin = 0.1f; 
            bool isVisible = viewportPos.z > 0 && 
                             viewportPos.x > -margin && viewportPos.x < 1 + margin && 
                             viewportPos.y > -margin && viewportPos.y < 1 + margin;

            if (!isVisible) return testPos;
            bestPos = testPos;
        }
        return bestPos + (bestPos - centerPoint).normalized * 10f; 
    }

    void SpawnObstacleAt(Vector3 position)
    {
        GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        GameObject newObstacle = Instantiate(prefab, position, randomRotation);
        newObstacle.transform.parent = this.transform; 
        activeObstacles.Add(newObstacle);
    }
}