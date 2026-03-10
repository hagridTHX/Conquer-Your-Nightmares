using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TrainingDummy : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    private Rigidbody rb;
    private Renderer rend;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        currentHealth = maxHealth;
        
        rb.mass = 50f;
        rb.linearDamping = 5f;
    }

    public void TakeDamage(float amount, Vector3 knockbackDir, float knockbackForce)
    {
        currentHealth -= amount;
        StartCoroutine(FlashRed());
        rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Kukła zniszczona!");
        Destroy(gameObject);
    }

    private System.Collections.IEnumerator FlashRed()
    {
        rend.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        rend.material.color = Color.white;
    }
}