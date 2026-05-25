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
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    
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
        // Sample input in Update to avoid missed events; apply physics in FixedUpdate.
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        Vector2 mousePos = inputActions.Player.MousePosition.ReadValue<Vector2>();

        if (!isUsingWasdMovement)
        {
            // Intersect the mouse ray with a ground plane to keep aiming planar.
            Ray ray = mainCam.ScreenPointToRay(mousePos);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float rayDist))
            {
                targetPosition = ray.GetPoint(rayDist);
                Debug.DrawLine(transform.position, targetPosition, Color.green);
            }
        }
        
        if (Mouse.current != null && currentWeapon != null)
                {
                    if (Mouse.current.leftButton.wasPressedThisFrame)
                    {
                        currentWeapon.TriggerAttack(swingLeft: true);
                    }
                    else if (Mouse.current.rightButton.wasPressedThisFrame)
                    {
                        currentWeapon.TriggerAttack(swingLeft: false);
                    }
                }
    }

    void FixedUpdate()
    {
        // Compute desired planar velocity first, then hand off to physics for integration.
        Vector3 desiredVelocity = Vector3.zero;

        if (isUsingWasdMovement)
        {
            Vector3 inputDir = new Vector3(moveInput.x, 0, moveInput.y).normalized;
            desiredVelocity = inputDir * maxSpeed;
        }
        else
        {
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

        Vector3 lookDir = targetPosition - transform.position;
        lookDir.y = 0; 
        
        // Use squared magnitude to avoid a sqrt in the idle-rotation check.
        if (lookDir.sqrMagnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, 15f * Time.fixedDeltaTime));
        }

        if (animator != null)
        {
            Vector3 horizVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            animator.SetFloat("Speed", horizVel.magnitude);
        }
    }

    private void ApplyPhysicsMovement(Vector3 desiredVel)
    {
        Vector3 currentVel = rb.linearVelocity;
        Vector3 velXZ = new Vector3(currentVel.x, 0, currentVel.z);

        // Steering is a velocity delta; acceleration mode keeps it mass-independent.
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