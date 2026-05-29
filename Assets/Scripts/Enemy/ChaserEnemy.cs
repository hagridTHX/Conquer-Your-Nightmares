using UnityEngine;

public class ChaserEnemy : Enemy 
{
    protected override void HandleMovement()
    {
        Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
        directionToPlayer.y = 0; 
        
        Vector3 finalDirection = ApplyObstacleAvoidance(directionToPlayer);
        
        rb.linearVelocity = new Vector3(finalDirection.x * MoveSpeed, rb.linearVelocity.y, finalDirection.z * MoveSpeed);

        if (finalDirection != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(finalDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, lookRot, 10f * Time.fixedDeltaTime);
        }
    }
}