using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class WalkingController : MonoBehaviour
{
    [Header("=== НАСТРОЙКИ ДВИЖЕНИЯ ===")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float rotationSpeed = 10f;

    [Header("=== ИНЕРЦИЯ ===")]
    public float acceleration = 10f;      // Как быстро набираем скорость
    public float groundDrag = 5f;         // Трение на земле (чем больше, тем быстрее остановка)
    public float airDrag = 1f;            // Трение в воздухе (меньше = больше инерция)
    public float airControl = 0.3f;       // Контроль в воздухе (0 = нет контроля, 1 = полный)

    [Header("=== ССЫЛКИ ===")]
    public Animator animator1;  // Тело/ноги
    public Animator animator2;  // Руки/оружие
    public Transform cameraTransform;  // Камера

    // Ссылка на JumpController
    private JumpController jumpController;

    // Внутренние переменные
    private CharacterController controller;
    private Vector3 moveDirection;      // Текущее направление движения (с инерцией)
    private float horizontalSpeed;
    private float verticalVelocity;
    private float gravity = -9.81f;

    // Хэш параметра
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (animator1 == null)
            animator1 = GetComponent<Animator>();

        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
                cameraTransform = mainCam.transform;
        }

        jumpController = GetComponent<JumpController>();
    }

    void Update()
    {
        // 1. Проверка земли
        bool isGrounded = controller.isGrounded;
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        // 2. Ввод движения
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // 3. Определяем: идём или бежим?
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && z != 0;
        float targetSpeed = isRunning ? runSpeed : walkSpeed;

        // 🔥 4. ПРОВЕРКА: Можно ли двигаться? (только на земле для полного контроля)
        bool canMove = jumpController != null ? jumpController.IsGrounded() : controller.isGrounded;
        float controlMultiplier = canMove ? 1f : airControl;

        // 🔥 5. Вычисляем желаемое направление (относительно камеры)
        Vector3 targetDirection = Vector3.zero;
        if (x != 0 || z != 0)
        {
            targetDirection = GetCameraRelativeMovement(x, z);
        }

        // 🔥 6. Плавное изменение скорости (ИНЕРЦИЯ!)
        if (canMove)
        {
            // На земле: ускоряемся к целевой скорости
            if (targetDirection.magnitude > 0.1f)
            {
                horizontalSpeed = Mathf.MoveTowards(horizontalSpeed, targetSpeed, acceleration * Time.deltaTime);
                moveDirection = targetDirection * horizontalSpeed;
            }
            else
            {
                // Нет ввода: замедляемся из-за трения
                horizontalSpeed = Mathf.MoveTowards(horizontalSpeed, 0, groundDrag * Time.deltaTime);
                moveDirection = transform.forward * horizontalSpeed;
            }
        }
        else
        {
            // 🔥 В воздухе: сохраняем инерцию + небольшое трение
            horizontalSpeed = Mathf.MoveTowards(horizontalSpeed, horizontalSpeed, 0); // Сохраняем скорость
            moveDirection = moveDirection.normalized * horizontalSpeed;

            // Небольшое трение в воздухе
            horizontalSpeed = Mathf.MoveTowards(horizontalSpeed, horizontalSpeed * 0.99f, airDrag * Time.deltaTime);
        }

        // 🔥 7. Применяем движение
        controller.Move(moveDirection * Time.deltaTime);

        // 🔥 8. Поворот (только если на земле и есть движение)
        if (moveDirection.magnitude >= 0.1f && canMove)
        {
            Vector3 lookDirection = new Vector3(moveDirection.x, 0f, moveDirection.z);
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 9. Гравитация
        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);

        // 10. ОТПРАВКА В ОБО АНИМАТОРА
        float speedValue = 0f;

        if (horizontalSpeed >= 0.1f && canMove)
        {
            speedValue = isRunning ? 1f : 0.5f;
        }

        SetFloatOnBoth(SpeedHash, speedValue);
    }

    // Движение относительно камеры
    Vector3 GetCameraRelativeMovement(float x, float z)
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = (forward * z) + (right * x);
        move.Normalize();

        return move;
    }

    // Функция для двух аниматоров
    void SetFloatOnBoth(int hash, float value)
    {
        if (animator1 != null)
        {
            animator1.SetFloat(hash, value, 0.1f, Time.deltaTime);
        }

        if (animator2 != null)
        {
            animator2.SetFloat(hash, value, 0.1f, Time.deltaTime);
        }
    }

    // 🔥 Публичный метод для получения текущей скорости (для прыжка)
    public Vector3 GetCurrentVelocity()
    {
        return moveDirection + Vector3.up * verticalVelocity;
    }
}