using UnityEngine;

// Dziedziczy z Enemy, a nie z MonoBehaviour!
// Dzięki temu ma już HP, damage i ataki zdefiniowane w Enemy.cs
public class ChaserEnemy : Enemy 
{
    void FixedUpdate()
    {
        // Jeśli nie ma gracza lub wróg nie żyje, nie rób nic
        if (playerTarget == null || isDead) return;

        // 1. Oblicz kierunek
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0; 

        // 2. Ruch (korzystamy z moveSpeed i rb zdefiniowanych w klasie Enemy)
        Vector3 newPos = transform.position + direction * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        // 3. Obrót
        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            rb.rotation = Quaternion.Slerp(rb.rotation, lookRot, 10f * Time.fixedDeltaTime);
        }
    }
}