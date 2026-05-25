using UnityEngine;

public class SmartEnemy : Enemy 
{
    [Tooltip("Distance at which the enemy starts anticipating an attack.")]
    [SerializeField] private float detectionRange = 5.0f;
    
    [Tooltip("Player speed threshold that triggers a dodge response.")]
    [SerializeField] private float dangerousVelocity = 5.0f;
    
    [Tooltip("Retreat speed during a dodge.")]
    [SerializeField] private float dodgeSpeed = 12f;

    [Tooltip("Dodge duration in seconds; prevents rapid oscillation.")]
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
            moveDir = -directionToPlayer;
            currentSpeed = dodgeSpeed; 
            Debug.DrawRay(transform.position, Vector3.up * 3, Color.cyan);
        }
        else
        {
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