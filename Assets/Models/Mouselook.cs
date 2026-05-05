using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("=== НАСТРОЙКИ ===")]
    public float mouseSensitivity = 2f;
    public float minVerticalAngle = -90f;
    public float maxVerticalAngle = 90f;

    [Header("=== ССЫЛКИ ===")]
    public Transform playerBody;  // Тело игрока (для горизонтального поворота)
    public Transform cameraTransform;  // Камера (для вертикального поворота)

    // Внутренние переменные
    private float verticalRotation = 0f;

    void Start()
    {
        // Если камера не назначена, используем Main Camera
        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
                cameraTransform = mainCam.transform;
        }

        // Если тело не назначено, используем родительский объект
        if (playerBody == null)
        {
            playerBody = transform;
        }

        // Скрываем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. Получаем ввод мыши
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 2. 🔥 Вертикальное вращение (камера вверх/вниз)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }

        // 3. 🔥 Горизонтальное вращение (тело влево/вправо)
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }

    // Показывать курсор при паузе (опционально)
    public void ToggleCursor(bool show)
    {
        if (show)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}