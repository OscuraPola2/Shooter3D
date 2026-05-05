using UnityEngine;

/// <summary>
/// Скрипт для отображения спрайта вспышки выстрела.
/// Повесьте на дочерний объект у дула оружия.
/// </summary>
public class MuzzleFlashSprite : MonoBehaviour
{
    [Header("=== НАСТРОЙКИ ВСПЫШКИ ===")]
    [Tooltip("Как долго показывать вспышку (сек)")]
    public float flashDuration = 0.05f;

    [Tooltip("Случайный размер (мин-макс множитель)")]
    public Vector2 randomScale = new Vector2(0.8f, 1.2f);

    [Tooltip("Случайный поворот (градусы)")]
    public float randomRotation = 45f;

    [Tooltip("Цвет вспышки")]
    public Color flashColor = Color.white;

    [Header("=== ССЫЛКИ ===")]
    [Tooltip("SpriteRenderer для вспышки")]
    public SpriteRenderer spriteRenderer;

    [Tooltip("Точка дула (для позиции)")]
    public Transform muzzlePoint;

    // Внутренние переменные
    private float flashTimer = 0f;
    private bool isFlashing = false;

    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
            spriteRenderer.color = flashColor;
        }
    }

    void Update()
    {
        // 🔥 Таймер отключения вспышки
        if (isFlashing)
        {
            flashTimer -= Time.deltaTime;

            if (flashTimer <= 0)
            {
                HideFlash();
            }
        }
    }

    // 🔥 Вызывать из WeaponController при выстреле
    public void ShowFlash()
    {
        if (spriteRenderer == null)
        {
            Debug.LogWarning("MuzzleFlashSprite: SpriteRenderer не назначен!");
            return;
        }

        // 🔥 Позиция у дула
        if (muzzlePoint != null)
        {
            transform.position = muzzlePoint.position;
            transform.forward = muzzlePoint.forward;
        }

        // 🔥 Случайный размер
        float randomSize = Random.Range(randomScale.x, randomScale.y);
        transform.localScale = Vector3.one * randomSize;

        // 🔥 Случайный поворот
        float randomRot = Random.Range(-randomRotation, randomRotation);
        transform.rotation *= Quaternion.Euler(0, 0, randomRot);

        // 🔥 Показать вспышку
        spriteRenderer.enabled = true;
        spriteRenderer.color = flashColor;

        isFlashing = true;
        flashTimer = flashDuration;
    }

    // 🔥 Скрыть вспышку
    void HideFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        isFlashing = false;
    }

    // 🔥 Публичный метод для принудительного скрытия
    public void ForceHide()
    {
        HideFlash();
    }
}