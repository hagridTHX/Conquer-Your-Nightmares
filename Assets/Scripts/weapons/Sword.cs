using UnityEngine;

public class Sword : Weapon
{
    [Header("Free Hinge Physics")]
    [SerializeField] private float massInertia = 20.0f;
    [SerializeField] private float airDrag = 2.5f;
    [SerializeField] private float stability = 0.5f;

    [Header("Limits & Collisions")]
    [Tooltip("Bazowa maksymalna prędkość obrotowa (zapobiega kręceniu się jak helikopter).")]
    [SerializeField] private float maxAngularVelocity = 25f;
    
    [Tooltip("Jak bardzo miecz odbija się od drzew (0 = staje w miejscu, 1 = sprężyna).")]
    [SerializeField] private float bounceBounciness = 0.6f;

    [Tooltip("Ile pędu traci miecz przy trafieniu wroga (uczucie cięcia mięsa).")]
    [SerializeField] private float hitResistance = 0.15f;

    private float currentAngleY;
    private float angularVelocity;
    private Vector3 lastVelocity;

    public override void Initialize(PlayerController owner)
    {
        base.Initialize(owner);
        currentAngleY = transform.eulerAngles.y;
        if(playerRb != null) lastVelocity = playerRb.linearVelocity;
    }

    public override void HandlePhysics(float dt)
    {
        base.HandlePhysics(dt);
        if (playerRb == null) return;

        Vector3 currentVelocity = playerRb.linearVelocity;
        currentVelocity.y = 0; 

        Vector3 acceleration = (currentVelocity - lastVelocity) / dt;
        lastVelocity = currentVelocity;

        acceleration = Vector3.ClampMagnitude(acceleration, 40f);

        Vector3 inertiaForce = -acceleration * massInertia;
        Vector3 swordArm = transform.forward;
        float torque = Vector3.Cross(swordArm, inertiaForce).y;

        if (currentVelocity.magnitude > 0.1f)
        {
            Vector3 windDir = -currentVelocity.normalized;
            float stabilityTorque = Vector3.Cross(swordArm, windDir).y * (stability * currentVelocity.magnitude);
            torque += stabilityTorque;
        }

        angularVelocity += torque * dt;
        angularVelocity -= angularVelocity * airDrag * dt;

        //blokada predkosci obrotowej z uwzglednieniem multipliera
        float currentMaxSpeed = maxAngularVelocity * speedMultiplier;
        angularVelocity = Mathf.Clamp(angularVelocity, -currentMaxSpeed, currentMaxSpeed);

        currentAngleY += angularVelocity * dt * 50.0f; 

        ApplyRotation();
    }

    void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0, currentAngleY, 0);
    }

    protected override void HandleObstacleHit(Collider obstacle)
    {
        //odbicie od przeszkody
        angularVelocity = -angularVelocity * bounceBounciness;
        currentAngleY += (angularVelocity > 0 ? 5f : -5f);
        ApplyRotation();
    }

    protected override void HandleEnemyHit()
    {
        //spowalnia przy trafieniu
        angularVelocity -= angularVelocity * hitResistance;
    }

    protected override void StartSpecial()
    {
        base.StartSpecial();
        player.SetMovementMode(useWasd: true);
    }

    protected override void EndSpecial()
    {
        base.EndSpecial();
        player.SetMovementMode(useWasd: false);
    }
}