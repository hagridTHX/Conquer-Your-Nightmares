using UnityEngine;

public class Sword : Weapon
{
    [Header("Momentum Attack (LPM/PPM)")]
    [Tooltip("Base force when the player is stationary.")]
    [SerializeField] private float baseSwingForce = 1200f;
    [Tooltip("Momentum multiplier that scales force with player speed.")]
    [SerializeField] private float momentumMultiplier = 60f;
    
    [Tooltip("Velocity retained after the attack (0.5 halves momentum, 1.0 preserves it).")]
    [Range(0f, 1f)]
    [SerializeField] private float playerBrakingFactor = 0.5f; // Lower braking preserves momentum for attack flow.
    [SerializeField] private float attackCooldown = 0.4f;

    [Header("Swing Arc Control")]
    [Tooltip("Maximum sweep angle in degrees for a single swing.")]
    [SerializeField] private float maxSweepAngle = 150f;
    [Tooltip("Momentum retained after a swing (follow-through); 0.1 is a hard stop.")]
    [Range(0f, 1f)]
    [SerializeField] private float followThroughBrake = 0.05f; 

    [Header("Orbital Physics (Free Hinge)")]
    [SerializeField] private float massInertia = 30.0f;
    
    public float MassInertia 
    {
        get => massInertia;
        set => massInertia = value;
    }
    
    [SerializeField] private float airDrag = 0.7f; 
    [SerializeField] private float windResistance = 1.0f; 

    [Header("Limits & Collisions")]
    [SerializeField] private float maxAngularVelocity = 2500f;
    [SerializeField] private float bounceBounciness = 0.6f;
    [SerializeField] private float hitResistance = 0.15f;

    private float currentGlobalAngleY; 
    private float angularVelocity;
    private Vector3 lastPlayerVelocity;
    private float lastAttackTime;

    // Swing arc state bounds rotation to a deliberate, readable cut.
    private bool isSwinging;
    private float swingStartAngle;

    public override void Initialize(PlayerController owner)
    {
        base.Initialize(owner);
        currentGlobalAngleY = transform.eulerAngles.y;
        
        if(playerRb != null) 
        {
            lastPlayerVelocity = playerRb.linearVelocity;
        }
    }

    public override void TriggerAttack(bool swingLeft)
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        float playerSpeed = 0f;

        if (playerRb != null)
        {
            playerSpeed = playerRb.linearVelocity.magnitude;

            // Retain partial momentum so the attack feels like a dash without runaway speed.
            playerRb.linearVelocity *= playerBrakingFactor;
        }

        float momentumBonus = Mathf.Pow(playerSpeed, 1.5f) * momentumMultiplier;
        float totalSwingForce = baseSwingForce + momentumBonus;
        float swingDirection = swingLeft ? 1f : -1f;

        // Start a controlled swing to cap arc length and enable deterministic follow-through.
        isSwinging = true;
        swingStartAngle = currentGlobalAngleY; // Capture start angle to measure swept arc.
        
        angularVelocity = totalSwingForce * swingDirection;

        Debug.Log($"<color=orange>ATAK!</color> Kierunek: {(swingLeft ? "Lewo" : "Prawo")} | Pęd Gracza: {playerSpeed:F1} | Siła Ciosu: {totalSwingForce:F0}");
    }

    public override void HandlePhysics(float dt)
    {
        base.HandlePhysics(dt);
        if (playerRb == null) return;

        Vector3 currentVelocity = playerRb.linearVelocity;
        currentVelocity.y = 0; 

        Vector3 acceleration = (currentVelocity - lastPlayerVelocity) / dt;
        lastPlayerVelocity = currentVelocity;

        acceleration = Vector3.ClampMagnitude(acceleration, 60f);
        Vector3 inertiaForce = -acceleration * massInertia;
        Vector3 swordForward = Quaternion.Euler(0, currentGlobalAngleY, 0) * Vector3.forward;
        float torque = Vector3.Cross(swordForward, inertiaForce).y;

        // Apply wind drag only outside active swings to avoid damping attack impulse.
        if (!isSwinging && currentVelocity.sqrMagnitude > 0.1f)
        {
            Vector3 windDir = -currentVelocity.normalized;
            float windTorque = Vector3.Cross(swordForward, windDir).y * (windResistance * currentVelocity.magnitude);
            torque += windTorque;
        }

        angularVelocity += torque * dt;
        angularVelocity -= angularVelocity * airDrag * dt; 

        float currentMaxSpeed = maxAngularVelocity * speedMultiplier;
        angularVelocity = Mathf.Clamp(angularVelocity, -currentMaxSpeed, currentMaxSpeed);

        currentGlobalAngleY += angularVelocity * dt; 

        // Swing-arc limiter applies a brake once the configured sweep is reached.
        if (isSwinging)
        {
            // Measure the swept angle in world space for a stable arc limit.
            float traveledAngle = Mathf.Abs(currentGlobalAngleY - swingStartAngle);
            
            // End the swing once the configured arc length is exceeded.
            if (traveledAngle >= maxSweepAngle)
            {
                isSwinging = false; // End the active cut.
                angularVelocity *= followThroughBrake; // Hard brake with a small follow-through.
            }
        }

        ApplyRotation();
    }

    void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0, currentGlobalAngleY, 0);
    }

    protected override void HandleObstacleHit(Collider obstacle)
    {
        isSwinging = false; // Cancel the swing on wall impact to avoid tunneling.
        angularVelocity = -angularVelocity * bounceBounciness;
        currentGlobalAngleY += (angularVelocity > 0 ? 5f : -5f);
        ApplyRotation();
    }

    protected override void HandleEnemyHit()
    {
        // Enemy hits bleed speed without breaking the swing arc.
        angularVelocity -= angularVelocity * hitResistance;
    }

    protected override void StartSpecial()
    {
        base.StartSpecial();
        player.SetMovementMode(true);
    }

    protected override void EndSpecial()
    {
        base.EndSpecial();
        player.SetMovementMode(false);
    }
}