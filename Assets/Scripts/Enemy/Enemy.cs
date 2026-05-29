using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Enemy : MonoBehaviour, IDamageable
{
    [Header("Core Stats")]
    [SerializeField] private float maxHealth = 30f;
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Attack Stats")]
    [SerializeField] private float damage = 15f;
    [SerializeField] private float knockbackOnPlayer = 3f;
    [SerializeField] private float attackCooldown = 0.8f;

    [Header("Physics")]
    [SerializeField] private float knockbackResistance = 0f;
    
    [Header("XP")]
    [SerializeField] private GameObject xpGemPrefab; 
    [SerializeField] private float baseXpReward = 10f; 

    [Header("Obstacle Avoidance")]
    [SerializeField] private float obstacleDetectionDistance = 2.5f;
    [SerializeField] private float sensorHeight = 0.5f;
    [SerializeField] private LayerMask obstacleLayer;

    public float MaxHealth { get => maxHealth; set => maxHealth = value; }
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    public float Damage { get => damage; set => damage = value; }
    public float BaseXpReward { get => baseXpReward; set => baseXpReward = value; }

    protected float currentHealth;
    protected Transform playerTarget;
    protected Rigidbody rb;
    protected bool isDead = false;
    protected bool isTouchingPlayer = false; 
    protected float knockbackTimer = 0f;
    
    private float nextAttackTime = 0f;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = MaxHealth;
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTarget = playerObj.transform;
    }

    protected virtual void FixedUpdate()
    {
        if (isDead || playerTarget == null) return;

        if (Vector3.Distance(transform.position, playerTarget.position) > 100f)
        {
            float teleportRadius = 35f;
            Vector3 newPos = transform.position;
            Camera cam = Camera.main;

            for(int i = 0; i < 15; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle.normalized * teleportRadius;
                newPos = playerTarget.position + new Vector3(randomCircle.x, 2f, randomCircle.y);
                
                if (cam != null)
                {
                    Vector3 viewPos = cam.WorldToViewportPoint(newPos);
                    if (viewPos.z < 0 || viewPos.x < -0.1f || viewPos.x > 1.1f || viewPos.y < -0.1f || viewPos.y > 1.1f)
                    {
                        break;
                    }
                    teleportRadius += 5f;
                }
            }
            
            rb.position = newPos; 
            rb.linearVelocity = Vector3.zero; 
            return; 
        }

        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return; 
        }

        if (isTouchingPlayer)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            
            Vector3 dir = (playerTarget.position - transform.position).normalized;
            dir.y = 0;
            if (dir != Vector3.zero)
                rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(dir), 10f * Time.fixedDeltaTime);
            
            return;
        }

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        HandleMovement();
    }

    protected abstract void HandleMovement();

    protected Vector3 ApplyObstacleAvoidance(Vector3 desiredDirection)
    {
        Vector3 origin = transform.position + Vector3.up * sensorHeight;
        
        Vector3 dirLeft = Quaternion.Euler(0, -35, 0) * desiredDirection;
        Vector3 dirRight = Quaternion.Euler(0, 35, 0) * desiredDirection;

        bool hitCenter = Physics.Raycast(origin, desiredDirection, obstacleDetectionDistance, obstacleLayer);
        bool hitLeft = Physics.Raycast(origin, dirLeft, obstacleDetectionDistance, obstacleLayer);
        bool hitRight = Physics.Raycast(origin, dirRight, obstacleDetectionDistance, obstacleLayer);

        if (hitCenter)
        {
            if (!hitRight) return Quaternion.Euler(0, 70, 0) * desiredDirection;
            if (!hitLeft) return Quaternion.Euler(0, -70, 0) * desiredDirection;
            return Quaternion.Euler(0, 120, 0) * desiredDirection;
        }
        if (hitLeft) return Quaternion.Euler(0, 45, 0) * desiredDirection;
        if (hitRight) return Quaternion.Euler(0, -45, 0) * desiredDirection;

        return desiredDirection;
    }

    public virtual void TakeDamage(float amount, Vector3 knockbackDir, float knockbackForce)
    {
        if (isDead) return;
        currentHealth -= amount;

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        knockbackTimer = 0.3f;
        rb.linearVelocity = Vector3.zero;

        float finalKnockback = knockbackForce * (1f - knockbackResistance);
        rb.AddForce(knockbackDir * finalKnockback, ForceMode.Impulse);

        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        isDead = true;

        if (xpGemPrefab != null)
        {
            Vector3 dropPos = transform.position + Vector3.up * 0.5f; 
            GameObject droppedGem = Instantiate(xpGemPrefab, dropPos, Quaternion.identity);

            XPGem gemScript = droppedGem.GetComponent<XPGem>();
            if (gemScript != null)
            {
                gemScript.xpValue = BaseXpReward;
            }
        }

        Destroy(gameObject);
    }

    protected virtual void OnCollisionStay(Collision collision)
    {
        if (isDead || playerTarget == null) return;

        if (collision.gameObject == playerTarget.gameObject)
        {
            isTouchingPlayer = true;

            if (Time.time >= nextAttackTime)
            {
                IDamageable playerHealth = collision.gameObject.GetComponent<IDamageable>();
                if (playerHealth != null)
                {
                    Vector3 dir = (collision.transform.position - transform.position).normalized;
                    dir.y = 0; 
                    playerHealth.TakeDamage(Damage, dir, knockbackOnPlayer);
                    nextAttackTime = Time.time + attackCooldown;
                }
            }
        }
    }

    protected virtual void OnCollisionExit(Collision collision)
    {
        if (playerTarget == null) return;

        if (collision.gameObject == playerTarget.gameObject)
        {
            isTouchingPlayer = false; 
        }
    }
    
    public virtual void UpgradeStats(float healthMultiplier, float damageMultiplier, float speedMultiplier, float scaleMultiplier = 1f)
    {
        MaxHealth *= healthMultiplier;
        currentHealth = MaxHealth;
        Damage *= damageMultiplier;
        MoveSpeed *= speedMultiplier;
        transform.localScale *= scaleMultiplier;
        BaseXpReward *= healthMultiplier;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + Vector3.up * sensorHeight;
        Vector3 fwd = transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + fwd * obstacleDetectionDistance);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + (Quaternion.Euler(0, -35, 0) * fwd) * obstacleDetectionDistance);
        Gizmos.DrawLine(origin, origin + (Quaternion.Euler(0, 35, 0) * fwd) * obstacleDetectionDistance);
    }
}