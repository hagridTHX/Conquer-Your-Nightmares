using UnityEngine;

public class SmartEnemy : Enemy 
{
    [SerializeField] private float detectionRange = 5.0f;
    [SerializeField] private float dangerousVelocity = 5.0f;
    [SerializeField] private float dodgeSpeed = 12f;
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
        }
        else
        {
            moveDir = directionToPlayer;
            currentSpeed = MoveSpeed;
        }

        Vector3 finalDirection = ApplyObstacleAvoidance(moveDir);

        rb.linearVelocity = new Vector3(finalDirection.x * currentSpeed, rb.linearVelocity.y, finalDirection.z * currentSpeed);

        if (finalDirection != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(finalDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, lookRot, 10f * Time.fixedDeltaTime);
        }
    }
}