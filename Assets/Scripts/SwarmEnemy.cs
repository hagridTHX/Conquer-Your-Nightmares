using UnityEngine;

public class SwarmEnemy : Enemy
{
    [Header("Ustawienia Chmary (Flocking)")]
    [Tooltip("Z jakiej odległości potwór 'czuje' innych w grupie.")]
    [SerializeField] private float swarmRadius = 3.0f;

    [Tooltip("Separacja: Jak mocno odpycha się, żeby nie wchodzić na innych.")]
    [SerializeField] private float separationWeight = 2.0f;

    [Tooltip("Spójność: Jak bardzo dąży do bycia wewnątrz grupy (tworzy zbitą chmarę).")]
    [SerializeField] private float cohesionWeight = 1.5f;

    [Tooltip("Wyrównanie: Jak bardzo chce iść równolegle do grupy.")]
    [SerializeField] private float alignmentWeight = 1.0f;

    [Tooltip("Cel: Jak mocno stado ciągnie w stronę gracza.")]
    [SerializeField] private float targetWeight = 3.0f;

    void FixedUpdate()
    {
        if (playerTarget == null || isDead) return;

        Vector3 separationMove = Vector3.zero;
        Vector3 cohesionMove = Vector3.zero;
        Vector3 alignmentMove = Vector3.zero;
        int swarmCount = 0;

        // Szukamy kolegów w promieniu
        Collider[] neighbors = Physics.OverlapSphere(transform.position, swarmRadius);

        foreach (Collider col in neighbors)
        {
            if (col.gameObject != this.gameObject)
            {
                // Interesują nas tylko potwory z tego samego stada
                SwarmEnemy otherEnemy = col.GetComponent<SwarmEnemy>();
                if (otherEnemy != null)
                {
                    // 1. SEPARACJA (odpychanie działa silniej, gdy są bardzo blisko)
                    Vector3 pushAway = transform.position - col.transform.position;
                    pushAway.y = 0;
                    float distance = pushAway.magnitude;
                    
                    if (distance > 0.01f && distance < swarmRadius * 0.5f)
                    {
                        // Im bliżej, tym silniej odpycha
                        separationMove += pushAway.normalized / distance;
                    }

                    // 2. SPÓJNOŚĆ (zbieranie pozycji sąsiadów do wyliczenia środka grupy)
                    cohesionMove += col.transform.position;

                    // 3. WYRÓWNANIE (zbieranie kierunku ruchu sąsiadów)
                    alignmentMove += col.transform.forward;

                    swarmCount++;
                }
            }
        }

        if (swarmCount > 0)
        {
            // Wyliczanie średnich dla stada
            separationMove /= swarmCount;

            // Środek masy grupy (idź w stronę centrum chmary)
            cohesionMove /= swarmCount;
            cohesionMove = (cohesionMove - transform.position).normalized;
            cohesionMove.y = 0;

            // Średni kierunek grupy
            alignmentMove /= swarmCount;
            alignmentMove.y = 0;
            alignmentMove = alignmentMove.normalized;
        }

        // 4. CEL (Kierunek do gracza)
        Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
        directionToPlayer.y = 0;

        // --- SUMOWANIE WSZYSTKICH SIŁ ---
        Vector3 finalDirection = (directionToPlayer * targetWeight) + 
                                 (separationMove * separationWeight) + 
                                 (cohesionMove * cohesionWeight) + 
                                 (alignmentMove * alignmentWeight);
        
        finalDirection.y = 0;
        finalDirection.Normalize();

        // --- RUCH I OBRÓT ---
        Vector3 newPos = transform.position + finalDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        if (finalDirection != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(finalDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, lookRot, 10f * Time.fixedDeltaTime);
        }
    }
}