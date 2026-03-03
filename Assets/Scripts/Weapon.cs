using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] protected float baseDamage = 10f;
    [SerializeField] protected float knockbackForce = 10f;
    [SerializeField] protected float velocityDamageMultiplier = 2.0f;
    [SerializeField] protected float minDamageVelocity = 1.0f; // Zmniejszyłem lekko próg

    [Header("Setup")]
    [Tooltip("Przeciągnij tutaj obiekt Model (to co faktycznie uderza).")]
    [SerializeField] protected Transform damagePoint; // <--- NOWOŚĆ

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
        // Obsługa starszych i nowszych wersji Unity
#if UNITY_6000_0_OR_NEWER
        playerRb = owner.GetComponent<Rigidbody>();
#else
        playerRb = owner.GetComponent<Rigidbody>();
#endif

        // Jeśli zapomniałeś przypisać w Inspectorze, próbujemy znaleźć dziecko
        if (damagePoint == null) 
        {
            damagePoint = transform.GetChild(0); 
        }
        lastPointPosition = damagePoint.position;
    }

    public virtual void HandlePhysics(float dt)
    {
        if (damagePoint == null) return;

        // ZMIANA: Liczymy prędkość punktu uderzenia (Modelu), a nie Pivota!
        float distanceMoved = (damagePoint.position - lastPointPosition).magnitude;
        currentSpeed = distanceMoved / dt;
        
        lastPointPosition = damagePoint.position;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        // Ignoruj gracza i inne bronie
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Weapon")) return;

        IDamageable target = other.GetComponent<IDamageable>();

        if (target != null)
        {
            // Debugowanie prędkości - odkomentuj jeśli nadal są problemy
            // Debug.Log($"Uderzenie! Prędkość miecza: {currentSpeed}");

            if (currentSpeed < minDamageVelocity) return;

            float finalDamage = baseDamage + (currentSpeed * velocityDamageMultiplier);
            Vector3 knockbackDir = (other.transform.position - transform.position).normalized;

            target.TakeDamage(finalDamage, knockbackDir, knockbackForce);

            Debug.Log($"<color=green>TRAFIENIE!</color> {other.name} | Prędkość: {currentSpeed:F1} | DMG: {finalDamage:F1}");
        }
    }

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