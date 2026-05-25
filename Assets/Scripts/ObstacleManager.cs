using UnityEngine;
using System.Collections.Generic;

public class ObstacleManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject[] obstaclePrefabs;
    
    [Header("Density Settings")]
    [Tooltip("Target obstacle count around the camera.")]
    [SerializeField] [Range(10, 300)] private int maxObstacles = 75;

    [Tooltip("Minimum spacing to prevent overlaps.")]
    [SerializeField] [Range(1f, 10f)] private float minSpacing = 3.0f;

    [Header("Spawn Area")]
    [SerializeField] private float spawnRadius = 35f;
    [SerializeField] private float despawnRadius = 50f;

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

        // Cull obstacles outside both camera and player radii to keep density bounded.
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

        // Spawn offscreen to maintain density without visible popping.
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
        // Project the camera center onto the ground plane to anchor spawn logic in world space.
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