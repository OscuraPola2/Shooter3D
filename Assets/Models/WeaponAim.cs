using UnityEngine;

public class WeaponTiltController : MonoBehaviour
{
    [Header("=== НАСТРОЙКИ НАКЛОНА ===")]
    public float tiltSpeed = 10f;              // Скорость наклона
    public float smoothTime = 0.15f;           // Время сглаживания (чем больше, тем плавнее)
    public float forwardTilt = -5f;            // Наклон вниз при движении вперёд
    public float backwardTilt = 5f;            // Наклон вверх при движении назад
    public float leftTilt = 5f;                // Наклон влево
    public float rightTilt = -5f;              // Наклон вправо
    public bool returnToCenter = true;         // Возвращать в центр

    [Header("=== СГЛАЖИВАНИЕ ===")]
    public AnimationCurve tiltCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // Кривая плавности
    public float damping = 0.5f;               // Затухание (0 = нет, 1 = сильное)

    [Header("=== ССЫЛКИ ===")]
    public Transform cameraTransform;
    public Transform weaponPivot;

    // Внутренние переменные
    private Vector3 currentTilt;
    private Vector3 targetTilt;
    private Vector3 velocity;                  // Для SmoothDamp
    private float deadZone = 0.1f;

    void Start()
    {
        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
                cameraTransform = mainCam.transform;
        }

        // Кривая по умолчанию (если не назначена)
        if (tiltCurve.length == 0)
        {
            tiltCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }
    }

    void Update()
    {
        // 1. Получаем ввод движения
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // 2. Проверяем, есть ли ввод
        bool isMoving = (Mathf.Abs(x) > deadZone || Mathf.Abs(z) > deadZone);

        // 3. Вычисляем целевой наклон
        if (isMoving)
        {
            // 🔥 Наклон по X (вперёд/назад)
            if (z > deadZone)
            {
                targetTilt.x = Mathf.Lerp(targetTilt.x, forwardTilt, Time.deltaTime * tiltSpeed);
            }
            else if (z < -deadZone)
            {
                targetTilt.x = Mathf.Lerp(targetTilt.x, backwardTilt, Time.deltaTime * tiltSpeed);
            }
            else
            {
                targetTilt.x = Mathf.Lerp(targetTilt.x, 0f, Time.deltaTime * tiltSpeed);
            }

            // 🔥 Наклон по Z (влево/вправо)
            if (x > deadZone)
            {
                targetTilt.z = Mathf.Lerp(targetTilt.z, rightTilt, Time.deltaTime * tiltSpeed);
            }
            else if (x < -deadZone)
            {
                targetTilt.z = Mathf.Lerp(targetTilt.z, leftTilt, Time.deltaTime * tiltSpeed);
            }
            else
            {
                targetTilt.z = Mathf.Lerp(targetTilt.z, 0f, Time.deltaTime * tiltSpeed);
            }
        }
        else if (returnToCenter)
        {
            // 🔥 Плавный возврат в центр с затуханием
            targetTilt.x = Mathf.Lerp(targetTilt.x, 0f, Time.deltaTime * tiltSpeed * damping);
            targetTilt.z = Mathf.Lerp(targetTilt.z, 0f, Time.deltaTime * tiltSpeed * damping);
        }

        // 4. 🔥 SmoothDamp для максимальной плавности
        currentTilt = Vector3.SmoothDamp(currentTilt, targetTilt, ref velocity, smoothTime);

        // 5. Применяем наклон к пустышке
        if (weaponPivot != null)
        {
            weaponPivot.localRotation = Quaternion.Euler(currentTilt);
        }
    }

    // 🔥 Отладка
    void OnDrawGizmosSelected()
    {
        if (weaponPivot != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(weaponPivot.position, weaponPivot.up * 1f);

            // Показываем текущий наклон
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(weaponPivot.position, weaponPivot.position + weaponPivot.right * currentTilt.z);
        }
    }
}