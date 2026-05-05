using UnityEngine;

namespace JDTechnology
{
    public class WeaponSway_Combined : MonoBehaviour
    {
        #region Sway Settings (Mouse)
        [Header("=== SWAY (Mouse) ===")]
        [SerializeField] private float initialRotationSwingSpeed = 10f;
        [SerializeField] private float returnRotationSwingSpeed = 5f;
        [SerializeField] private float linearRotationalSwayAmount = 20f;
        [SerializeField] private float forwardRotationSwayAmount = 30f;
        [SerializeField] private float movementSwaySpeed = 3f;
        [SerializeField] private float verticalSwayAmount = 1f;
        [SerializeField] private float horizontalSwayAmount = 1f;

        // 🔥 НОВОЕ: Сглаживание мыши для устранения рывков
        [Tooltip("Сглаживание ввода мыши (0 = нет, 0.9 = очень плавно)")]
        [Range(0f, 0.9f)]
        [SerializeField] private float mouseInputSmooth = 0.3f;
        #endregion

        #region Tilt Settings (WASD)
        [Header("=== TILT (WASD) ===")]
        [SerializeField] private float tiltSpeed = 10f;
        [SerializeField] private float forwardTilt = -5f;
        [SerializeField] private float backwardTilt = 5f;
        [SerializeField] private float leftTilt = 5f;
        [SerializeField] private float rightTilt = -5f;
        [SerializeField] private bool returnTiltToCenter = true;
        [SerializeField] private float tiltDamping = 0.5f;
        #endregion

        #region Private
        private Quaternion targetRotation;
        private Quaternion startingLocalRotation;
        private Vector3 startingLocalPosition;
        private float horizontalPos;
        private float verticalPos;

        // 🔥 Сглаженный ввод мыши
        private float smoothMouseX;
        private float smoothMouseY;

        // Для tilt
        private Vector3 targetTilt;
        private Vector3 currentTilt;
        private Vector3 tiltVelocity;
        private float deadZone = 0.1f;
        #endregion

        private void Start()
        {
            startingLocalPosition = transform.localPosition;
            startingLocalRotation = transform.localRotation;
            targetRotation = startingLocalRotation;
        }

        void Update()
        {
            // 🔥 1. Получаем и сглаживаем ввод мыши
            GetSmoothMouseInput();

            // 🔥 2. Применяем SWAY (от мыши)
            ApplySway();

            // 🔥 3. Применяем TILT (от WASD)
            ApplyTilt();

            // 🔥 4. Комбинируем и применяем
            transform.localRotation = startingLocalRotation * targetRotation * Quaternion.Euler(currentTilt);
            transform.localPosition = startingLocalPosition + new Vector3(horizontalPos, verticalPos, 0);
        }

        // 🔥 Сглаженный ввод мыши (убирает рывки)
        private void GetSmoothMouseInput()
        {
            float rawMouseX = Input.GetAxis("Mouse X");
            float rawMouseY = Input.GetAxis("Mouse Y");

            // Low-pass filter для плавности
            smoothMouseX = Mathf.Lerp(smoothMouseX, rawMouseX, Time.deltaTime / (mouseInputSmooth + 0.01f));
            smoothMouseY = Mathf.Lerp(smoothMouseY, rawMouseY, Time.deltaTime / (mouseInputSmooth + 0.01f));
        }

        // 🔥 Применяем SWAY (оригинальная логика + сглаженный ввод)
        private void ApplySway()
        {
            Quaternion rotationX = Quaternion.AngleAxis(-smoothMouseY * linearRotationalSwayAmount, Vector3.right);
            Quaternion rotationY = Quaternion.AngleAxis(smoothMouseX * linearRotationalSwayAmount, Vector3.up);
            Quaternion rotationZ = Quaternion.AngleAxis(-smoothMouseX * forwardRotationSwayAmount, Vector3.forward);

            Quaternion frameTargetRot = rotationX * rotationY * rotationZ;
            targetRotation = Quaternion.Slerp(targetRotation, frameTargetRot, Time.deltaTime * returnRotationSwingSpeed);

            horizontalPos = Mathf.Lerp(horizontalPos, smoothMouseX * horizontalSwayAmount, movementSwaySpeed * Time.deltaTime);
            verticalPos = Mathf.Lerp(verticalPos, smoothMouseY * verticalSwayAmount, movementSwaySpeed * Time.deltaTime);
        }

        // 🔥 Применяем TILT (от WASD)
        private void ApplyTilt()
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            bool isMoving = (Mathf.Abs(x) > deadZone || Mathf.Abs(z) > deadZone);

            if (isMoving)
            {
                if (z > deadZone)
                    targetTilt.x = Mathf.Lerp(targetTilt.x, forwardTilt, Time.deltaTime * tiltSpeed);
                else if (z < -deadZone)
                    targetTilt.x = Mathf.Lerp(targetTilt.x, backwardTilt, Time.deltaTime * tiltSpeed);
                else
                    targetTilt.x = Mathf.Lerp(targetTilt.x, 0f, Time.deltaTime * tiltSpeed);

                if (x > deadZone)
                    targetTilt.z = Mathf.Lerp(targetTilt.z, rightTilt, Time.deltaTime * tiltSpeed);
                else if (x < -deadZone)
                    targetTilt.z = Mathf.Lerp(targetTilt.z, leftTilt, Time.deltaTime * tiltSpeed);
                else
                    targetTilt.z = Mathf.Lerp(targetTilt.z, 0f, Time.deltaTime * tiltSpeed);
            }
            else if (returnTiltToCenter)
            {
                targetTilt.x = Mathf.Lerp(targetTilt.x, 0f, Time.deltaTime * tiltSpeed * tiltDamping);
                targetTilt.z = Mathf.Lerp(targetTilt.z, 0f, Time.deltaTime * tiltSpeed * tiltDamping);
            }

            currentTilt = Vector3.SmoothDamp(currentTilt, targetTilt, ref tiltVelocity, 0.15f);
        }
    }
}