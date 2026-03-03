using UnityEngine;

public class Sword : Weapon
{
    [Header("Free Hinge Physics")]
    [Tooltip("Jak ciężki jest miecz (bezwładność). Im więcej, tym mocniej lata przy zwrotach.")]
    [SerializeField] private float massInertia = 20.0f;

    [Tooltip("Opór powietrza. To jedyna rzecz, która hamuje miecz. 0 = kręci się w nieskończoność.")]
    [SerializeField] private float airDrag = 2.5f;

    [Tooltip("Delikatna siła centrująca. 0 = miecz żyje własnym życiem. 1-2 = lekko dąży do tyłu przy biegu.")]
    [SerializeField] private float stability = 0.5f;

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

        // 1. Obliczamy PRZYSPIESZENIE gracza (zmianę prędkości)
        // To jest siła, która szarpie mieczem.
        Vector3 currentVelocity = playerRb.linearVelocity;
        currentVelocity.y = 0; // Ignorujemy ruch góra-dół

        // Przyspieszenie = (ObecnaPrędkość - Poprzednia) / czas
        Vector3 acceleration = (currentVelocity - lastVelocity) / dt;
        lastVelocity = currentVelocity;

        // 2. Siła Bezwładności (Inertia Force)
        // Działa przeciwnie do przyspieszenia gracza (III zasada dynamiki)
        // Jak ruszasz w przód, miecz czuje siłę w tył.
        Vector3 inertiaForce = -acceleration * massInertia;

        // 3. Zamiana Siły na Moment Obrotowy (Torque)
        // Używamy iloczynu wektorowego (Cross Product), żeby sprawdzić, 
        // czy siła działa prostopadle do miecza (wtedy kręci najmocniej).
        
        // Wektor kierunku miecza (gdzie celuje ostrze)
        Vector3 swordArm = transform.forward;
        
        // Obliczamy moment obrotowy z siły bezwładności
        // (Cross product w Y powie nam jak mocno obrócić miecz w poziomie)
        float torque = Vector3.Cross(swordArm, inertiaForce).y;

        // 4. (Opcjonalnie) Mała stabilizacja, żeby przy długim biegu prosto miecz nie leciał bokiem
        if (currentVelocity.magnitude > 0.1f)
        {
            // Kąt wiatru (przeciwny do ruchu)
            Vector3 windDir = -currentVelocity.normalized;
            float stabilityTorque = Vector3.Cross(swordArm, windDir).y * (stability * currentVelocity.magnitude);
            torque += stabilityTorque;
        }

        // 5. Aplikacja Fizyki
        angularVelocity += torque * dt;

        // Opór powietrza (Drag) - hamuje obrót
        angularVelocity -= angularVelocity * airDrag * dt;

        // Aplikacja obrotu
        currentAngleY += angularVelocity * dt * 50.0f; // Skaler dla wygody (50x)

        ApplyRotation();
    }

    void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0, currentAngleY, 0);
    }

    // --- Special Ability ---
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