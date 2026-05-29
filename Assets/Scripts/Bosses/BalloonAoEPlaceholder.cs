using UnityEngine;
using System.Collections;

public class BalloonAoEPlaceholder : MonoBehaviour
{
    [SerializeField] private float delay = 1.5f;
    [SerializeField] private float radius = 4f;
    [SerializeField] private float damage = 25f;

    void Start() => StartCoroutine(ExplosionSequence());

    private IEnumerator ExplosionSequence()
    {
        // Tutaj można przeskalować okrąg, sugerując zbliżający się wybuch
        yield return new WaitForSeconds(delay);

        // Efekt wybuchu (wizualizacja sferą lub log)
        Debug.Log("<color=red>BUM! Balon uderzył w ziemię!</color>");

        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                IDamageable player = hit.GetComponent<IDamageable>();
                if (player != null) player.TakeDamage(damage, (hit.transform.position - transform.position).normalized, 10f);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos() // Widoczne w edytorze kółko zasięgu wybuchu
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}