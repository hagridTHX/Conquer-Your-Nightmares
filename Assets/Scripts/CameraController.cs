using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Ustawienia Kamery")]
    [Tooltip("Jak szybko kamera przesuwa się na WASD")]
    [SerializeField] private float panSpeed = 20f;

    [Header("Śledzenie Gracza (Opcjonalne)")]
    [Tooltip("Czy kamera ma wracać do gracza, gdy nic nie wciskamy?")]
    [SerializeField] private bool returnToPlayer = false;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float returnSpeed = 2.0f;

    private GameInput inputActions;
    private Vector3 initialOffset;

    void Awake()
    {
        inputActions = new GameInput();
    }

    void Start()
    {
        if (playerTransform != null)
        {
            initialOffset = transform.position - playerTransform.position;
        }
    }

    void OnEnable() => inputActions.Player.Enable();
    void OnDisable() => inputActions.Player.Disable();

    void Update()
    {
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();

        if (input.magnitude > 0.1f)
        {
            Vector3 moveDir = new Vector3(input.x, 0, input.y);
            transform.Translate(moveDir * panSpeed * Time.deltaTime, Space.World);
        }
        else if (returnToPlayer && playerTransform != null)
        {
            Vector3 targetPosition = playerTransform.position + initialOffset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, returnSpeed * Time.deltaTime);
        }
    }
}