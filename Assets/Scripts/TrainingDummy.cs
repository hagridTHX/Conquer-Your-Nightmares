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
        
        // Kukła musi mieć fizykę, żeby odlecieć po uderzeniu (knockback)
        rb.mass = 50f; // Ciężki obiekt
        rb.linearDamping = 5f;  // Szybko wyhamowuje
    }

    public void TakeDamage(float amount, Vector3 knockbackDir, float knockbackForce)
    {
        currentHealth -= amount;
        // Efekt wizualny (błyśnięcie na czerwono)
        StartCoroutine(FlashRed());

        // Aplikacja Knockbacku (odrzutu)
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