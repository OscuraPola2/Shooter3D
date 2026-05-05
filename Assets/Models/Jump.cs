using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class JumpController : MonoBehaviour
{
    [Header("=== НАСТРОЙКИ ПРЫЖКА ===")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;
    public float groundCheckDistance = 0.2f;

    [Header("=== ИНЕРЦИЯ ПРЫЖКА ===")]
    public float inheritMomentum = 1f;  // Сколько инерции сохранять при прыжке (0 = нет, 1 = полная)

    [Header("=== ССЫЛКИ ===")]
    public Animator animator1;  // Тело/ноги
    public Animator animator2;  // Руки/оружие
    public Transform groundCheck;  // Точка проверки земли

    // Ссылка на WalkingController
    private WalkingController walkingController;

    // Внутренние переменные
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    // Хэш параметра
    private static readonly int GroundedHash = Animator.StringToHash("Grounded");

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (animator1 == null)
            animator1 = GetComponent<Animator>();

        if (groundCheck == null)
        {
            groundCheck = new GameObject("GroundCheck").transform;
            groundCheck.SetParent(transform);
            groundCheck.localPosition = new Vector3(0f, -0.5f, 0f);
        }

        // 🔥 Находим скрипт ходьбы для получения инерции
        walkingController = GetComponent<WalkingController>();
    }

    void Update()
    {
        // 1. Проверка земли
        CheckGround();

        // 2. Прыжок
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        // 3. Гравитация
        ApplyGravity();

        // 4. Отправка состояния в Animator
        SetGroundedOnBoth(isGrounded);
    }

    // Проверка земли
    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, LayerMask.GetMask("Ground"));

        if (!isGrounded)
        {
            isGrounded = controller.isGrounded;
        }
    }

    // 🔥 Выполнение прыжка с сохранением инерции
    void Jump()
    {
        // Вертикальная скорость прыжка
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // 🔥 Сохраняем горизонтальную инерцию от ходьбы/бега
        if (walkingController != null && inheritMomentum > 0)
        {
            Vector3 currentVelocity = walkingController.GetCurrentVelocity();

            // Добавляем горизонтальную скорость к прыжку
            controller.Move(new Vector3(currentVelocity.x, 0, currentVelocity.z) * inheritMomentum * Time.deltaTime);
        }
    }

    // Применение гравитации
    void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(Vector3.up * velocity.y * Time.deltaTime);
    }

    // Функция для двух аниматоров (Bool)
    void SetGroundedOnBoth(bool value)
    {
        if (animator1 != null)
        {
            animator1.SetBool(GroundedHash, value);
        }

        if (animator2 != null)
        {
            animator2.SetBool(GroundedHash, value);
        }
    }

    // 🔥 ПУБЛИЧНЫЙ МЕТОД для WalkingController
    public bool IsGrounded()
    {
        return isGrounded;
    }

    // Отладка
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckDistance);
        }
    }
}