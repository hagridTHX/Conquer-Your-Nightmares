using UnityEngine;
using System.Collections;

public class Clown : Enemy 
{
    public enum BossState 
    { 
        Intro,          
        Chase,          
        MeleeAttack,    
        ShootProjectiles, 
        BalloonDrop,    
        DogSummon,      
        Floating        
    }

    [Header("Boss Logic")]
    [SerializeField] private BossState currentState = BossState.Chase;
    [SerializeField] private float specialAttackCooldown = 6f;
    
    private float specialTimer;
    private bool isActionLocked = false; 
    private Animator anim;

    [Header("Prefabs & Spawners")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject balloonAoEPrefab;
    [SerializeField] private GameObject balloonDogPrefab;
    [SerializeField] private Transform attackSpawnPoint; 

    [Header("Attack: Melee")]
    [SerializeField] private float meleeDamage = 30f;
    [SerializeField] private float meleeRange = 3.5f;

    [Header("Attack: Projectiles")]
    [SerializeField] private int projectileCount = 100;
    [SerializeField] private float projectileSpreadAngle = 120f;

    [Header("Attack: Balloon Drop")]
    [SerializeField] private int balloonDropCount = 8;
    [SerializeField] private float balloonDropRadius = 8f;

    [Header("Attack: Dogs")]
    [SerializeField] private int dogSummonCount = 4;
    [SerializeField] private float dogSpawnSpread = 5f;

    [Header("Attack: Floating")]
    [SerializeField] private float floatHeight = 4f;
    [SerializeField] private float floatDuration = 4f;

    public BossState CurrentState { get => currentState; set => currentState = value; }
    public float SpecialAttackCooldown { get => specialAttackCooldown; set => specialAttackCooldown = value; }
    public float MeleeDamage { get => meleeDamage; set => meleeDamage = value; }
    public float MeleeRange { get => meleeRange; set => meleeRange = value; }
    public int ProjectileCount { get => projectileCount; set => projectileCount = value; }
    public float ProjectileSpreadAngle { get => projectileSpreadAngle; set => projectileSpreadAngle = value; }
    public int BalloonDropCount { get => balloonDropCount; set => balloonDropCount = value; }
    public float BalloonDropRadius { get => balloonDropRadius; set => balloonDropRadius = value; }
    public int DogSummonCount { get => dogSummonCount; set => dogSummonCount = value; }
    public float DogSpawnSpread { get => dogSpawnSpread; set => dogSpawnSpread = value; }
    public float FloatHeight { get => floatHeight; set => floatHeight = value; }
    public float FloatDuration { get => floatDuration; set => floatDuration = value; }

    protected override void Start()
    {
        base.Start();
        anim = GetComponentInChildren<Animator>();
        specialTimer = specialAttackCooldown;
    }

    protected override void HandleMovement()
    {
        if (isActionLocked || isDead) return;

        specialTimer -= Time.fixedDeltaTime;

        switch (currentState)
        {
            case BossState.Chase:
                HandleChaseBehavior();
                break;
            case BossState.MeleeAttack:
                StartCoroutine(PerformMeleeAttack());
                break;
            case BossState.ShootProjectiles:
                StartCoroutine(PerformShootProjectiles());
                break;
            case BossState.BalloonDrop:
                StartCoroutine(PerformBalloonDrop());
                break;
            case BossState.DogSummon:
                StartCoroutine(PerformDogSummon());
                break;
            case BossState.Floating:
                StartCoroutine(PerformFloating());
                break;
        }
    }

    private void HandleChaseBehavior()
    {
        if (playerTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer < meleeRange)
        {
            currentState = BossState.MeleeAttack;
            return;
        }
        
        if (specialTimer <= 0)
        {
            int randomAttack = Random.Range(3, 7); 
            currentState = (BossState)randomAttack;
            specialTimer = specialAttackCooldown;
            return;
        }

        Vector3 dir = (playerTarget.position - transform.position).normalized;
        dir.y = 0;
        
        rb.linearVelocity = new Vector3(dir.x * MoveSpeed, rb.linearVelocity.y, dir.z * MoveSpeed);
        
        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            rb.rotation = Quaternion.Slerp(rb.rotation, lookRot, 10f * Time.fixedDeltaTime);
        }
    }

    private IEnumerator PerformMeleeAttack()
    {
        isActionLocked = true;
        rb.linearVelocity = Vector3.zero;

        if (anim != null) anim.SetTrigger("MeleeAttack");
        
        yield return new WaitForSeconds(0.6f);

        if (playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) <= meleeRange + 1f)
        {
            IDamageable playerDamageable = playerTarget.GetComponent<IDamageable>();
            if (playerDamageable != null)
            {
                Vector3 knockbackDir = (playerTarget.position - transform.position).normalized;
                playerDamageable.TakeDamage(meleeDamage, knockbackDir, 15f);
            }
        }

        yield return new WaitForSeconds(0.9f); 
        ReturnToChase();
    }

    private IEnumerator PerformShootProjectiles()
    {
        isActionLocked = true;
        rb.linearVelocity = Vector3.zero;

        if (anim != null) anim.SetTrigger("CastSpell");

        yield return new WaitForSeconds(0.5f);

        if (projectilePrefab != null && attackSpawnPoint != null)
        {
            float startAngle = -projectileSpreadAngle / 2f;
            float angleStep = projectileCount > 1 ? projectileSpreadAngle / (projectileCount - 1) : 0f;

            for (int i = 0; i < projectileCount; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                Quaternion rotation = transform.rotation * Quaternion.Euler(0, currentAngle, 0);
                Instantiate(projectilePrefab, attackSpawnPoint.position, rotation);
            }
        }

        yield return new WaitForSeconds(1f);
        ReturnToChase();
    }

    private IEnumerator PerformBalloonDrop()
    {
        isActionLocked = true;
        rb.linearVelocity = Vector3.zero;

        if (anim != null) anim.SetTrigger("CastSpell");

        if (balloonAoEPrefab != null && playerTarget != null)
        {
            for (int i = 0; i < balloonDropCount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * balloonDropRadius;
                Vector3 spawnPos = playerTarget.position + new Vector3(randomOffset.x, 0.1f, randomOffset.y);
                Instantiate(balloonAoEPrefab, spawnPos, Quaternion.identity);
                yield return new WaitForSeconds(0.15f); 
            }
        }

        yield return new WaitForSeconds(1f);
        ReturnToChase();
    }

    private IEnumerator PerformDogSummon()
    {
        isActionLocked = true;
        rb.linearVelocity = Vector3.zero;

        if (anim != null) anim.SetTrigger("Summon");

        yield return new WaitForSeconds(0.5f);

        if (balloonDogPrefab != null)
        {
            for (int i = 0; i < dogSummonCount; i++)
            {
                float xOffset = 0f;
                if (dogSummonCount > 1) 
                {
                    float t = (float)i / (dogSummonCount - 1);
                    xOffset = Mathf.Lerp(-dogSpawnSpread / 2f, dogSpawnSpread / 2f, t);
                }

                Vector3 spawnOffset = transform.rotation * new Vector3(xOffset, 0, 1.5f);
                GameObject dog = Instantiate(balloonDogPrefab, transform.position + spawnOffset, Quaternion.identity);
                
                var dogScript = dog.GetComponent<BalloonDogPlaceholder>();
                if (dogScript != null) dogScript.SetTarget(playerTarget);
            }
        }

        yield return new WaitForSeconds(1f);
        ReturnToChase();
    }

    private IEnumerator PerformFloating()
    {
        isActionLocked = true;
        rb.isKinematic = true; 

        if (anim != null) anim.SetBool("IsFloating", true);

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.up * floatHeight;
        float elapsed = 0f;
        
        while (elapsed < 1f)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed);
            elapsed += Time.deltaTime * 2f;
            yield return null;
        }

        float floatTimer = floatDuration;
        
        while (floatTimer > 0)
        {
            if (balloonAoEPrefab != null && playerTarget != null)
            {
                Vector3 spawnPos = playerTarget.position + new Vector3(Random.Range(-3f, 3f), 0.1f, Random.Range(-3f, 3f));
                Instantiate(balloonAoEPrefab, spawnPos, Quaternion.identity);
            }
            floatTimer -= 0.8f;
            yield return new WaitForSeconds(0.8f);
        }

        startPos = transform.position;
        targetPos = new Vector3(transform.position.x, playerTarget.position.y, transform.position.z);
        elapsed = 0f;
        
        while (elapsed < 1f)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed);
            elapsed += Time.deltaTime * 2f;
            yield return null;
        }

        rb.isKinematic = false;
        if (anim != null) anim.SetBool("IsFloating", false);
        
        ReturnToChase();
    }

    private void ReturnToChase()
    {
        currentState = BossState.Chase;
        isActionLocked = false;
    }

    public override void TakeDamage(float amount, Vector3 knockbackDir, float knockbackForce)
    {
        if (currentState == BossState.Floating) return; 
        base.TakeDamage(amount, knockbackDir, 0f);
    }

    protected override void Die()
    {
        base.Die();
    }
}