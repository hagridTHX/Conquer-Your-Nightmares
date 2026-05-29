using UnityEngine;

public class BalloonDogPlaceholder : MonoBehaviour
{
    private Transform target;
    [SerializeField] private float speed = 5.5f;
    [SerializeField] private float explodeDistance = 1.2f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private float fuseTimer = 6f; 

    public void SetTarget(Transform player) => target = player;

    void Update()
    {
        if (target == null) return;

        // Ruch w stronę gracza
        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0;
        transform.position += dir * speed * Time.deltaTime;
        transform.forward = dir;

        fuseTimer -= Time.deltaTime;

        if (Vector3.Distance(transform.position, target.position) <= explodeDistance || fuseTimer <= 0)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (target != null && Vector3.Distance(transform.position, target.position) <= explodeDistance + 1f)
        {
            IDamageable player = target.GetComponent<IDamageable>();
            if (player != null) player.TakeDamage(damage, (target.position - transform.position).normalized, 8f);
        }
        
        // Tutaj można zespawnować mały system cząsteczek (confetti/pęknięcie balona)
        Destroy(gameObject);
    }
}