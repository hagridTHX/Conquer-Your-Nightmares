using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] protected float baseDamage = 10f;
    [SerializeField] protected float knockbackForce = 10f;
    [SerializeField] protected float velocityDamageMultiplier = 2.0f;
    [SerializeField] protected float minDamageVelocity = 1.0f;

    [Header("Stat Multipliers (For Leveling Up)")]
    public float damageMultiplier = 1.0f;
    public float speedMultiplier = 1.0f;
    public float sizeMultiplier = 1.0f;

    [Header("Setup")]
    [Tooltip("Przeciągnij tutaj obiekt Model (to co faktycznie uderza).")]
    [SerializeField] protected Transform damagePoint; 

    [Header("Special Ability")]
    [SerializeField] protected float specialCooldown = 5f;
    [SerializeField] protected float specialDuration = 3f;
    
    protected float lastSpecialTime;
    protected bool isSpecialActive;

    protected PlayerController player;
    protected Rigidbody playerRb;
    
    private Vector3 lastPointPosition;
    protected float currentSpeed; 

    public virtual void Initialize(PlayerController owner)
    {
        player = owner;
#if UNITY_6000_0_OR_NEWER
        playerRb = owner.GetComponent<Rigidbody>();
#else
        playerRb = owner.GetComponent<Rigidbody>();
#endif
        if (damagePoint == null) 
        {
            damagePoint = transform.GetChild(0); 
        }
        lastPointPosition = damagePoint.position;
    }

    public virtual void HandlePhysics(float dt)
    {
        if (damagePoint == null) return;

        float distanceMoved = (damagePoint.position - lastPointPosition).magnitude;
        currentSpeed = distanceMoved / dt;
        lastPointPosition = damagePoint.position;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Weapon")) return;

        if (other.CompareTag("Obstacle"))
        {
            HandleObstacleHit(other);
            return;
        }

        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null)
        {
            if (currentSpeed < minDamageVelocity) return;

            float finalDamage = (baseDamage * damageMultiplier) + (currentSpeed * velocityDamageMultiplier);
            Vector3 knockbackDir = (other.transform.position - transform.position).normalized;

            target.TakeDamage(finalDamage, knockbackDir, knockbackForce);
            HandleEnemyHit();

            Debug.Log($"<color=green>TRAFIENIE!</color> {other.name} | Prędkość: {currentSpeed:F1} | DMG: {finalDamage:F1}");
        }
    }

    protected virtual void HandleObstacleHit(Collider obstacle) { }
    protected virtual void HandleEnemyHit() { }

    public void TryUseSpecial()
    {
        if (Time.time >= lastSpecialTime + specialCooldown && !isSpecialActive)
        {
            StartSpecial();
            lastSpecialTime = Time.time;
        }
    }
    protected virtual void StartSpecial() { isSpecialActive = true; Invoke(nameof(EndSpecial), specialDuration); }
    protected virtual void EndSpecial() { isSpecialActive = false; }
}