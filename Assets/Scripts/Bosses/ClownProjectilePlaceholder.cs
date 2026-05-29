using UnityEngine;

public class ClownProjectilePlaceholder : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 4f;

    void Start() => Destroy(gameObject, lifetime);

    void Update() => transform.Translate(Vector3.forward * speed * Time.deltaTime);

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IDamageable player = other.GetComponent<IDamageable>();
            if (player != null) player.TakeDamage(damage, transform.forward, 5f);
            Destroy(gameObject);
        }
    }
}