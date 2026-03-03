using UnityEngine;

// Wymaga Rigidbody, żeby fizyka działała
[RequireComponent(typeof(Rigidbody))]
public abstract class Enemy : MonoBehaviour, IDamageable
{
    [Header("Główne Statystyki")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float moveSpeed = 4f;
    
    [Header("Statystyki Ataku")]
    [SerializeField] protected float damage = 10f;
    
    [Tooltip("Siła odrzutu gracza przy dotyku. Ustaw 0 dla zwykłych mobów, a np. 15 dla Bossów!")]
    [SerializeField] protected float knockbackOnPlayer = 0f; // <--- TUTAJ JEST MAGIA
    
    [Tooltip("Co ile sekund wróg zadaje obrażenia przy ciągłym styku.")]
    [SerializeField] protected float attackCooldown = 1.0f;

    [Header("Fizyka")]
    [SerializeField] protected float knockbackResistance = 0f; // 0 = leci daleko, 1 = niewzruszony

    protected float currentHealth;
    protected Transform playerTarget;
    protected Rigidbody rb;
    protected bool isDead = false;
    
    private float nextAttackTime = 0f; // Licznik do ataków

    // --- SETUP ---
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        
        // Każdy wróg szuka gracza na starcie
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }
    }

    // --- LOGIKA OTRZYMYWANIA OBRAŻEŃ (Wspólna dla wszystkich) ---
    public virtual void TakeDamage(float amount, Vector3 knockbackDir, float knockbackForce)
    {
        if (isDead) return;

        currentHealth -= amount;
        
        // Reakcja na uderzenie (Odrzut) - uwzględniamy odporność (dla bossów)
        float finalKnockback = knockbackForce * (1f - knockbackResistance);
        rb.AddForce(knockbackDir * finalKnockback, ForceMode.Impulse);

        Debug.Log($"{gameObject.name} oberwał za {amount:F1}. HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;
        // Tu dodamy później particle wybuchu albo dropienie itemów
        Destroy(gameObject);
    }

    // --- ATAKOWANIE GRACZA (Wspólne) ---
    protected virtual void OnCollisionStay(Collision collision)
    {
        if (isDead) return;

        // Prosty system ataku dotykowego
        if (collision.gameObject.CompareTag("Player"))
        {
            // Czy minął czas od ostatniego ataku?
            if (Time.time >= nextAttackTime)
            {
                IDamageable playerHealth = collision.gameObject.GetComponent<IDamageable>();
                if (playerHealth != null)
                {
                    Vector3 dir = (collision.transform.position - transform.position).normalized;
                    dir.y = 0; // Żeby nie wbijało gracza w podłogę
                    
                    // Używamy zmiennej "knockbackOnPlayer" zamiast sztywnej liczby
                    playerHealth.TakeDamage(damage, dir, knockbackOnPlayer);
                    
                    // Ustawiamy czas kolejnego ataku
                    nextAttackTime = Time.time + attackCooldown;
                }
            }
        }
    }
}