using UnityEngine;

public class SmartEnemy : Enemy // <-- Dziedziczy z Enemy
{
    [Header("Inteligencja")]
    [Tooltip("Dystans, przy którym wróg zaczyna się bać ataku.")]
    [SerializeField] private float detectionRange = 5.0f;
    
    [Tooltip("Jak szybko gracz musi się poruszać, żeby wróg uznał to za atak.")]
    [SerializeField] private float dangerousVelocity = 5.0f;
    
    [Tooltip("Jak szybko cofa się przy uniku.")]
    [SerializeField] private float dodgeSpeed = 12f;

    [Tooltip("Czas trwania uniku (w sekundach). Zapobiega 'drganiu' wroga.")]
    [SerializeField] private float dodgeDuration = 0.3f; // <--- NOWOŚĆ

    private Rigidbody playerRb;
    private float dodgeTimer = 0f; // <--- NOWOŚĆ: Licznik czasu uniku

    protected override void Start()
    {
        base.Start(); // Wywołaj start z klasy bazowej (szukanie gracza, HP)
        
        if (playerTarget != null)
        {
            playerRb = playerTarget.GetComponent<Rigidbody>();
        }
    }

    void FixedUpdate()
    {
        if (playerTarget == null || isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
        directionToPlayer.y = 0;

        // --- MÓZG PRZECIWNIKA ---
        
        // 1. Zmniejszamy licznik uniku (jeśli trwa)
        if (dodgeTimer > 0)
        {
            dodgeTimer -= Time.fixedDeltaTime;
        }

        // 2. Sprawdzamy zagrożenie TYLKO wtedy, gdy wróg aktualnie NIE robi uniku
        if (dodgeTimer <= 0 && playerRb != null && distanceToPlayer < detectionRange)
        {
            float playerSpeed = playerRb.linearVelocity.magnitude;

            if (playerSpeed > dangerousVelocity)
            {
                // Zaczynamy unik! Ustawiamy czas trwania (np. na 0.5 sekundy)
                dodgeTimer = dodgeDuration;
            }
        }

        // --- RUCH ---

        Vector3 moveDir;
        float currentSpeed;

        // Jeśli licznik jest większy od 0, wróg JEST w trakcie odskakiwania
        if (dodgeTimer > 0)
        {
            // UCIEKAJ! (Wektor przeciwny do gracza)
            moveDir = -directionToPlayer;
            currentSpeed = dodgeSpeed; 
            
            // Debug: Błękitny promień oznacza, że wróg jest w trakcie uniku
            Debug.DrawRay(transform.position, Vector3.up * 3, Color.cyan);
        }
        else
        {
            // GOŃ!
            moveDir = directionToPlayer;
            currentSpeed = moveSpeed;
        }

        // Fizyczne przesunięcie
        Vector3 newPos = transform.position + moveDir * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        // Obrót zawsze w stronę gracza
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(directionToPlayer);
            rb.rotation = Quaternion.Slerp(rb.rotation, lookRot, 10f * Time.fixedDeltaTime);
        }
    }
}