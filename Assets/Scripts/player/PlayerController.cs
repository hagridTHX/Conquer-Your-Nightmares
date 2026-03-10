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
    
    private Rigidbody rb;
    private Camera mainCam;
    private GameInput inputActions;
    
    private Vector3 targetPosition;
    private Vector2 moveInput;
    private bool isUsingWasdMovement = false;

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
        inputActions.Player.SpecialAttack.performed += ctx => UseWeaponSpecial();
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Player.SpecialAttack.performed -= ctx => UseWeaponSpecial();
    }

    void Update()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        Vector2 mousePos = inputActions.Player.MousePosition.ReadValue<Vector2>();

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
            //tryb wasd
            Vector3 inputDir = new Vector3(moveInput.x, 0, moveInput.y).normalized;
            desiredVelocity = inputDir * maxSpeed;
        }
        else
        {
            //tryb myszki
            Vector3 direction = (targetPosition - transform.position);
            direction.y = 0;
            
            if (direction.magnitude > stopDistance)
            {
                desiredVelocity = direction.normalized * maxSpeed;
            }
        }

        ApplyPhysicsMovement(desiredVelocity);

        if (currentWeapon != null && currentWeapon.gameObject.activeInHierarchy)
        {
            currentWeapon.HandlePhysics(Time.fixedDeltaTime);
        }
    }

    private void ApplyPhysicsMovement(Vector3 desiredVel)
    {
        Vector3 currentVel = rb.linearVelocity;
        Vector3 velXZ = new Vector3(currentVel.x, 0, currentVel.z);

        Vector3 steering = desiredVel - velXZ;
        rb.AddForce(steering * acceleration * Time.fixedDeltaTime, ForceMode.Acceleration);

        if (velXZ.magnitude > 0.1f)
        {
            rb.AddForce(-velXZ * friction * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
    }

    public void UseWeaponSpecial()
    {
        if (currentWeapon != null)
        {
            currentWeapon.TryUseSpecial();
        }
    }

    public void SetMovementMode(bool useWasd)
    {
        isUsingWasdMovement = useWasd;
    }
    
    public void EquipWeapon(Weapon newWeapon)
    {
        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(false);
        }

        currentWeapon = newWeapon;
        currentWeapon.gameObject.SetActive(true);
        currentWeapon.Initialize(this);

        Debug.Log($"Gracz wyposażył: {currentWeapon.gameObject.name}");
    }
}