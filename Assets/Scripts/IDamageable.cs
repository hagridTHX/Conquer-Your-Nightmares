using UnityEngine;

public interface IDamageable
{
    // Każdy kto oberwie, musi obsłużyć tę metodę
    void TakeDamage(float amount, Vector3 knockbackDir, float knockbackForce);
}