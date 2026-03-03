using UnityEngine;
using UnityEngine.SceneManagement; // <-- Dodaj to na samej górze!

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Zdrowie")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount, Vector3 knockbackDir, float knockbackForce)
    {
        currentHealth -= amount;
        Debug.Log($"<color=red>AUĆ! HP: {currentHealth}/{maxHealth}</color>");

        // Opcjonalnie: lekki odrzut gracza
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Lekki impuls w tył
            rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("ZGON! Restart poziomu...");
        
        // Pobiera indeks aktualnej sceny i ładuje ją od nowa
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}