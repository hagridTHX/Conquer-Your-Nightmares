using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Ustawienia Kamery")]
    [Tooltip("Jak szybko kamera przesuwa się na WASD")]
    [SerializeField] private float panSpeed = 20f;

    [Tooltip("Marginesy mapy (żeby nie wyjechać w nicość). X = lewo/prawo, Y = góra/dół")]
    [SerializeField] private Vector2 limitX = new Vector2(-50, 50);
    [SerializeField] private Vector2 limitZ = new Vector2(-50, 50);

    [Header("Śledzenie Gracza (Opcjonalne)")]
    [Tooltip("Czy kamera ma wracać do gracza, gdy nic nie wciskamy?")]
    [SerializeField] private bool returnToPlayer = false;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float returnSpeed = 2.0f;

    private GameInput inputActions;
    private Vector3 initialOffset; // Różnica pozycji startowej między kamerą a graczem

    void Awake()
    {
        inputActions = new GameInput();
    }

    void Start()
    {
        // Zapamiętujemy, jak daleko kamera jest od gracza na starcie (wysokość i kąt)
        if (playerTransform != null)
        {
            initialOffset = transform.position - playerTransform.position;
        }
    }

    void OnEnable() => inputActions.Player.Enable();
    void OnDisable() => inputActions.Player.Disable();

    void Update()
    {
        // 1. Czytamy WASD
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();

        // 2. Jeśli gracz wciska WASD -> Przesuwamy kamerę
        if (input.magnitude > 0.1f)
        {
            Vector3 moveDir = new Vector3(input.x, 0, input.y);
            
            // Space.World jest kluczowe! Przesuwamy się wzdłuż podłogi, a nie tam gdzie patrzy obiektyw
            transform.Translate(moveDir * panSpeed * Time.deltaTime, Space.World);
        }
        // 3. (Opcjonalnie) Jeśli nic nie wciskamy i włączono powrót -> Kamera płynie do gracza
        else if (returnToPlayer && playerTransform != null)
        {
            Vector3 targetPosition = playerTransform.position + initialOffset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, returnSpeed * Time.deltaTime);
        }

        // 4. Ograniczenie do mapy (Clamp), żeby nie zgubić poziomu
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, limitX.x, limitX.y);
        pos.z = Mathf.Clamp(pos.z, limitZ.x, limitZ.y);
        transform.position = pos;
    }
}