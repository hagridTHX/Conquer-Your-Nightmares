using UnityEngine;

public class ChaserEnemy : Enemy 
{
    protected override void HandleMovement()
    {
        // oblicz kierunek
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0; 

        rb.linearVelocity = new Vector3(direction.x * moveSpeed, rb.linearVelocity.y, direction.z * moveSpeed);

        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            rb.rotation = Quaternion.Slerp(rb.rotation, lookRot, 10f * Time.fixedDeltaTime);
        }
    }
}