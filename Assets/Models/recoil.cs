using UnityEngine;

/// <summary>
/// Скрипт процедурной отдачи.
/// Повесьте на дочерний объект оружия (например, "RecoilHandler").
/// </summary>
public class WeaponRecoil : MonoBehaviour
{
    [Header("=== НАСТРОЙКИ ОТДАЧИ ===")]
    [Tooltip("Сила отдачи камеры (вверх)")]
    public float cameraRecoil = 0.5f;

    [Tooltip("Сила отдачи оружия (назад)")]
    public float weaponRecoil = 0.1f;

    [Tooltip("Накопление отдачи при зажиме")]
    public float recoilAccumulation = 0.1f;

    [Tooltip("Максимальная отдача")]
    public float maxRecoil = 2f;

    [Tooltip("Скорость возврата отдачи")]
    public float recoilRecovery = 5f;

    [Tooltip("Случайный разброс отдачи")]
    public float recoilRandomness = 0.2f;

    [Header("=== ССЫЛКИ ===")]
    [Tooltip("Объект оружия для смещения")]
    public Transform weaponPivot;

    [Tooltip("Камера игрока")]
    public Camera playerCamera;

    // Внутренние переменные
    private float currentRecoil = 0f;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 recoilDirection;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        // Сохраняем исходную позицию
        if (weaponPivot != null)
        {
            originalPosition = weaponPivot.localPosition;
            originalRotation = weaponPivot.localRotation;
        }
    }

    void Update()
    {
        // Возврат отдачи (каждый кадр)
        HandleRecoilRecovery();
    }

    // 🔥 Вызывать из WeaponController при выстреле
    public void AddRecoil()
    {
        // Накопление отдачи при зажиме
        currentRecoil = Mathf.Min(currentRecoil + recoilAccumulation, maxRecoil);

        // Случайное направление отдачи
        recoilDirection = new Vector3(
            Random.Range(-recoilRandomness, recoilRandomness),
            cameraRecoil + Random.Range(-recoilRandomness, recoilRandomness),
            Random.Range(-recoilRandomness, recoilRandomness)
        );

        // Смещение камеры
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation *= Quaternion.Euler(
                -recoilDirection.y * 0.5f,
                recoilDirection.x * 0.5f,
                0
            );
        }

        // Смещение оружия
        if (weaponPivot != null)
        {
            weaponPivot.localPosition = originalPosition + Vector3.back * weaponRecoil * currentRecoil;
            weaponPivot.localRotation = originalRotation * Quaternion.Euler(
                recoilDirection.x * currentRecoil * 10f,
                recoilDirection.y * currentRecoil * 10f,
                0
            );
        }
    }

    // Возврат отдачи
    void HandleRecoilRecovery()
    {
        if (currentRecoil > 0)
        {
            currentRecoil = Mathf.Lerp(currentRecoil, 0, Time.deltaTime * recoilRecovery);

            if (weaponPivot != null)
            {
                weaponPivot.localPosition = Vector3.Lerp(
                    weaponPivot.localPosition,
                    originalPosition,
                    Time.deltaTime * 10f
                );
                weaponPivot.localRotation = Quaternion.Slerp(
                    weaponPivot.localRotation,
                    originalRotation,
                    Time.deltaTime * 10f
                );
            }
        }
    }

    // Сброс отдачи (при перезарядке)
    public void ResetRecoil()
    {
        currentRecoil = 0f;
    }
}