using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 12f;
    [SerializeField] private float acceleration = 80f;
    [SerializeField] private float friction = 2f;
    [SerializeField] private float stopDistance = 1.0f;

    [Header("Equipment")]
    [SerializeField] private Weapon currentWeapon;
    
    // Pola prywatne
    private Rigidbody rb;
    private Camera mainCam;
    private GameInput inputActions;
    
    // Zmienne do logiki ruchu
    private Vector3 targetPosition; // Dla myszki
    private Vector2 moveInput;      // Dla WASD
    private bool isUsingWasdMovement = false; // Flaga trybu sterowania

    // Właściwość (Property) - bezpieczny dostęp do Rigidbody dla Broni
    public Rigidbody Rigidbody => rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCam = Camera.main;
        inputActions = new GameInput();

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (currentWeapon != null)
        {
            currentWeapon.Initialize(this);
        }
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        // Podpięcie przycisku specjala
        inputActions.Player.SpecialAttack.performed += ctx => UseWeaponSpecial();
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Player.SpecialAttack.performed -= ctx => UseWeaponSpecial();
    }

    void Update()
    {
        // Czytamy wejścia
        moveInput = inputActions.Player.Move.ReadValue<Vector2>(); // WASD
        Vector2 mousePos = inputActions.Player.MousePosition.ReadValue<Vector2>();

        // Obliczanie celu dla myszki (Raycast)
        if (!isUsingWasdMovement)
        {
            Ray ray = mainCam.ScreenPointToRay(mousePos);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float rayDist))
            {
                targetPosition = ray.GetPoint(rayDist);
                Debug.DrawLine(transform.position, targetPosition, Color.green);
            }
        }
    }

    void FixedUpdate()
    {
        Vector3 desiredVelocity = Vector3.zero;

        if (isUsingWasdMovement)
        {
            // --- TRYB WASD (Podczas specjala mieczem) ---
            Vector3 inputDir = new Vector3(moveInput.x, 0, moveInput.y).normalized;
            desiredVelocity = inputDir * maxSpeed;
        }
        else
        {
            // --- TRYB MYSZKI (Domyślny) ---
            Vector3 direction = (targetPosition - transform.position);
            direction.y = 0; // Ignoruj wysokość
            
            if (direction.magnitude > stopDistance)
            {
                desiredVelocity = direction.normalized * maxSpeed;
            }
        }

        ApplyPhysicsMovement(desiredVelocity);

        // Aktualizacja fizyki broni
        if (currentWeapon != null)
        {
            currentWeapon.HandlePhysics(Time.fixedDeltaTime);
        }
    }

    private void ApplyPhysicsMovement(Vector3 desiredVel)
    {
        Vector3 currentVel = rb.linearVelocity;
        Vector3 velXZ = new Vector3(currentVel.x, 0, currentVel.z);

        // Steering Force
        Vector3 steering = desiredVel - velXZ;
        rb.AddForce(steering * acceleration * Time.fixedDeltaTime, ForceMode.Acceleration);

        // Tarcie (tylko w poziomie)
        if (velXZ.magnitude > 0.1f)
        {
            rb.AddForce(-velXZ * friction * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
    }

    // --- PUBLIC METHODS FOR WEAPON INTERACTION ---

    public void UseWeaponSpecial()
    {
        if (currentWeapon != null)
        {
            currentWeapon.TryUseSpecial();
        }
    }

    // Metoda pozwalająca broni zmienić sterowanie (wymóg GDD dla tarczy)
    public void SetMovementMode(bool useWasd)
    {
        isUsingWasdMovement = useWasd;
        // Resetujemy pęd przy zmianie trybu, żeby nie było dziwnych przeskoków
        // (Opcjonalnie, zależy od game feelu)
    }
}