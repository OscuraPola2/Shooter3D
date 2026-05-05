using UnityEngine;

public class WeaponDecal : MonoBehaviour
{
    [Header("=== НАСТРОЙКИ ДЕКАЛЕЙ ===")]
    public GameObject decalPrefab;
    public float decalSize = 0.1f;
    public Vector2 randomScale = new Vector2(0.8f, 1.2f);
    public float randomRotation = 360f;
    public float decalLifetime = 0f;
    public int maxDecals = 50;

    [Tooltip("Отступ от поверхности")]
    public float surfaceOffset = 0.01f;

    [Header("=== ЭФФЕКТЫ ===")]
    [Tooltip("Префаб частиц пыли/искры")]
    public GameObject dustPrefab;

    [Tooltip("Масштаб эффекта пыли")]
    public float dustScale = 1.0f;

    [Header("=== ССЫЛКИ ===")]
    public Transform decalParent;

    private int currentDecalCount = 0;

    void Start()
    {
        if (decalParent == null)
        {
            GameObject decalContainer = new GameObject("Decals");
            decalParent = decalContainer.transform;
        }
    }

    // 🔥 ЭТОТ МЕТОД УЖЕ БЫЛ У ВАС — мы просто добавили пыль внутрь
    public void SpawnDecal(Vector3 position, Vector3 normal, GameObject hitObject)
    {
        if (decalPrefab == null)
        {
            Debug.LogWarning("WeaponDecal: Decal Prefab не назначен!");
            return;
        }

        // Ограничение количества декалей
        if (currentDecalCount >= maxDecals)
        {
            if (decalParent.childCount > 0)
            {
                Destroy(decalParent.GetChild(0).gameObject);
                currentDecalCount--;
            }
        }

        // Разворачиваем декаль к камере
        Quaternion rotation = Quaternion.LookRotation(-normal);

        // Спавн декали
        GameObject decal = Instantiate(decalPrefab, position, rotation, decalParent);

        // Смещение от поверхности
        decal.transform.position += normal * surfaceOffset;

        // Случайный размер
        float randomSize = Random.Range(randomScale.x, randomScale.y);
        decal.transform.localScale = Vector3.one * decalSize * randomSize;

        // Случайный поворот
        float randomRot = Random.Range(0f, randomRotation);
        decal.transform.Rotate(0, 0, randomRot, Space.Self);

        // Время жизни декали
        if (decalLifetime > 0)
        {
            Destroy(decal, decalLifetime);
        }

        currentDecalCount++;

        // 🔥 НОВОЕ: СПАВН ПЫЛИ/ЭФФЕКТА
        if (dustPrefab != null)
        {
            GameObject dust = Instantiate(dustPrefab, position, Quaternion.LookRotation(normal), decalParent);
            dust.transform.localScale *= dustScale;
            Destroy(dust, 2f);  // Удалить эффект через 2 секунды
        }
    }

    public void ClearAllDecals()
    {
        if (decalParent != null)
        {
            foreach (Transform child in decalParent)
            {
                Destroy(child.gameObject);
            }
            currentDecalCount = 0;
        }
    }
}