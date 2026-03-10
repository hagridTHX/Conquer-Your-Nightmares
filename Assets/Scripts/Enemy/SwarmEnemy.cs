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

    protected override void HandleMovement()
    {
        Vector3 separationMove = Vector3.zero;
        Vector3 cohesionMove = Vector3.zero;
        Vector3 alignmentMove = Vector3.zero;
        int swarmCount = 0;

        Collider[] neighbors = Physics.OverlapSphere(transform.position, swarmRadius);

        foreach (Collider col in neighbors)
        {
            if (col.gameObject != this.gameObject)
            {
                SwarmEnemy otherEnemy = col.GetComponent<SwarmEnemy>();
                if (otherEnemy != null)
                {
                    Vector3 pushAway = transform.position - col.transform.position;
                    pushAway.y = 0;
                    float distance = pushAway.magnitude;
                    
                    if (distance > 0.01f && distance < swarmRadius * 0.5f)
                    {
                        separationMove += pushAway.normalized / distance;
                    }

                    cohesionMove += col.transform.position;
                    alignmentMove += col.transform.forward;
                    swarmCount++;
                }
            }
        }

        if (swarmCount > 0)
        {
            separationMove /= swarmCount;

            cohesionMove /= swarmCount;
            cohesionMove = (cohesionMove - transform.position).normalized;
            cohesionMove.y = 0;

            alignmentMove /= swarmCount;
            alignmentMove.y = 0;
            alignmentMove = alignmentMove.normalized;
        }

        Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
        directionToPlayer.y = 0;

        //flocking - sumowanie sil
        Vector3 finalDirection = (directionToPlayer * targetWeight) + 
                                 (separationMove * separationWeight) + 
                                 (cohesionMove * cohesionWeight) + 
                                 (alignmentMove * alignmentWeight);
        
        finalDirection.y = 0;
        finalDirection.Normalize();

        rb.linearVelocity = new Vector3(finalDirection.x * moveSpeed, rb.linearVelocity.y, finalDirection.z * moveSpeed);

        if (finalDirection != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(finalDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, lookRot, 10f * Time.fixedDeltaTime);
        }
    }
}