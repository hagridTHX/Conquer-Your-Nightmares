using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Enemy : MonoBehaviour, IDamageable
{
    [Header("Główne Statystyki")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float moveSpeed = 4f;
    
    [Header("Statystyki Ataku")]
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float knockbackOnPlayer = 0f;
    [SerializeField] protected float attackCooldown = 1.0f;

    [Header("Fizyka")]
    [SerializeField] protected float knockbackResistance = 0f;

    protected float currentHealth;
    protected Transform playerTarget;
    protected Rigidbody rb;
    protected bool isDead = false;
    
    private float nextAttackTime = 0f;

    protected bool isTouchingPlayer = false; 
    protected float knockbackTimer = 0f;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTarget = playerObj.transform;
    }

    protected virtual void FixedUpdate()
    {
        if (isDead || playerTarget == null) return;

        //recykling wrogów - teleport gdy za daleko
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

        //ogłuszony - czeka na koniec knockbacku
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return; 
        }

        //dotyka gracza - blokada ruchu
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
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        HandleMovement();
    }

    protected abstract void HandleMovement();

    public virtual void TakeDamage(float amount, Vector3 knockbackDir, float knockbackForce)
    {
        if (isDead) return;
        currentHealth -= amount;

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        knockbackTimer = 0.3f;
        rb.linearVelocity = Vector3.zero;

        float finalKnockback = knockbackForce * (1f - knockbackResistance);
        rb.AddForce(knockbackDir * finalKnockback, ForceMode.Impulse);

        Debug.Log($"{gameObject.name} oberwał za {amount:F1}. HP: {currentHealth}");

        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        isDead = true;
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
                    playerHealth.TakeDamage(damage, dir, knockbackOnPlayer);
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
        maxHealth *= healthMultiplier;
        currentHealth = maxHealth;
        damage *= damageMultiplier;
        moveSpeed *= speedMultiplier;
        transform.localScale *= scaleMultiplier;
    }
}