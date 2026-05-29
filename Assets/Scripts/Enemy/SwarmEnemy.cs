using UnityEngine;

public class SwarmEnemy : Enemy
{
    [SerializeField] private float swarmRadius = 3.0f;
    [SerializeField] private float separationWeight = 2.0f;
    [SerializeField] private float cohesionWeight = 1.5f;
    [SerializeField] private float alignmentWeight = 1.0f;
    [SerializeField] private float targetWeight = 3.0f;

    protected override void HandleMovement()
    {
        Vector3 separationMove = Vector3.zero;
        Vector3 cohesionMove = Vector3.zero;
        Vector3 alignmentMove = Vector3.zero;
        int swarmCount = 0;

        Collider[] neighbors = Physics.OverlapSphere(transform.position, swarmRadius);

        foreach (Collider col in neighbors)
        {
            if (col.gameObject != this.gameObject)
            {
                SwarmEnemy otherEnemy = col.GetComponent<SwarmEnemy>();
                if (otherEnemy != null)
                {
                    Vector3 pushAway = transform.position - col.transform.position;
                    pushAway.y = 0;
                    float distance = pushAway.magnitude;
                    
                    if (distance > 0.01f && distance < swarmRadius * 0.5f)
                    {
                        separationMove += pushAway.normalized / distance;
                    }

                    cohesionMove += col.transform.position;
                    alignmentMove += col.transform.forward;
                    swarmCount++;
                }
            }
        }

        if (swarmCount > 0)
        {
            separationMove /= swarmCount;

            cohesionMove /= swarmCount;
            cohesionMove = (cohesionMove - transform.position).normalized;
            cohesionMove.y = 0;

            alignmentMove /= swarmCount;
            alignmentMove.y = 0;
            alignmentMove = alignmentMove.normalized;
        }

        Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
        directionToPlayer.y = 0;

        Vector3 flockingDirection = (directionToPlayer * targetWeight) + 
                                    (separationMove * separationWeight) + 
                                    (cohesionMove * cohesionWeight) + 
                                    (alignmentMove * alignmentWeight);
        
        flockingDirection.y = 0;
        flockingDirection.Normalize();

        Vector3 finalDirection = ApplyObstacleAvoidance(flockingDirection);

        rb.linearVelocity = new Vector3(finalDirection.x * MoveSpeed, rb.linearVelocity.y, finalDirection.z * MoveSpeed);

        if (finalDirection != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(finalDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, lookRot, 10f * Time.fixedDeltaTime);
        }
    }
}