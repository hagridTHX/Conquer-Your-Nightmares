using UnityEngine;

public class SmartEnemy : Enemy 
{
    [Header("Inteligencja")]
    [Tooltip("Dystans, przy którym wróg zaczyna się bać ataku.")]
    [SerializeField] private float detectionRange = 5.0f;
    
    [Tooltip("Jak szybko gracz musi się poruszać, żeby wróg uznał to za atak.")]
    [SerializeField] private float dangerousVelocity = 5.0f;
    
    [Tooltip("Jak szybko cofa się przy uniku.")]
    [SerializeField] private float dodgeSpeed = 12f;

    [Tooltip("Czas trwania uniku (w sekundach). Zapobiega 'drganiu' wroga.")]
    [SerializeField] private float dodgeDuration = 0.3f;

    private Rigidbody playerRb;
    private float dodgeTimer = 0f;

    protected override void Start()
    {
        base.Start(); 
        
        if (playerTarget != null)
        {
            playerRb = playerTarget.GetComponent<Rigidbody>();
        }
    }

    protected override void HandleMovement()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
        directionToPlayer.y = 0;
        
        if (dodgeTimer > 0)
        {
            dodgeTimer -= Time.fixedDeltaTime;
        }

        if (dodgeTimer <= 0 && playerRb != null && distanceToPlayer < detectionRange)
        {
            float playerSpeed = playerRb.linearVelocity.magnitude;
            if (playerSpeed > dangerousVelocity)
            {
                dodgeTimer = dodgeDuration;
            }
        }

        Vector3 moveDir;
        float currentSpeed;

        if (dodgeTimer > 0)
        {
            //unik - ucieka
            moveDir = -directionToPlayer;
            currentSpeed = dodgeSpeed; 
            Debug.DrawRay(transform.position, Vector3.up * 3, Color.cyan);
        }
        else
        {
            //goni gracza
            moveDir = directionToPlayer;
            currentSpeed = moveSpeed;
        }

        rb.linearVelocity = new Vector3(moveDir.x * currentSpeed, rb.linearVelocity.y, moveDir.z * currentSpeed);

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(directionToPlayer);
            rb.rotation = Quaternion.Slerp(rb.rotation, lookRot, 10f * Time.fixedDeltaTime);
        }
    }
}