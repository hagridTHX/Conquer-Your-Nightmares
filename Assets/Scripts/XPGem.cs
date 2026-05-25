using UnityEngine;

public class XPGem : MonoBehaviour
{
    [Header("Gem Settings")]
    [Tooltip("XP granted when collected.")]
    public float xpValue = 10f;
    
    [Tooltip("Pickup radius that triggers homing.")]
    public float magnetRadius = 5f;
    
    [Tooltip("Homing speed once magnetized.")]
    public float flySpeed = 15f;

    private Transform playerTarget;
    private bool isMagnetized = false;

    void Start()
    {
        // Resolve the player once on spawn to avoid repeated scene-wide searches.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;
        }
    }

    void Update()
    {
        if (playerTarget == null) return;

        // Gate homing until the pickup radius is reached to keep idle gems cheap.
        if (!isMagnetized)
        {
            float distance = Vector3.Distance(transform.position, playerTarget.position);
            if (distance <= magnetRadius)
            {
                isMagnetized = true; // Switch to homing state once inside radius.
            }
        }
        else
        {
            // Home to a chest-height offset to avoid ground clipping during pickup.
            Vector3 targetPos = playerTarget.position + Vector3.up;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, flySpeed * Time.deltaTime);

            // Auto-collect once within a small radius to prevent jitter at close range.
            if (Vector3.Distance(transform.position, targetPos) < 0.5f)
            {
                CollectGem();
            }
        }
    }

    private void CollectGem()
    {
        // Route XP through PlayerStats so progression stays centralized.
        PlayerStats stats = playerTarget.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.AddXP(xpValue);
        }
        
        // Destroy after grant to prevent duplicate pickups.
        Destroy(gameObject);
    }
}