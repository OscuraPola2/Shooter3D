using UnityEngine;

public class WeaponCameraSetup_Unity6 : MonoBehaviour
{
    [Header("=== ССЫЛКИ ===")]
    public Camera weaponCamera;
    public Transform weaponPivot;

    [Header("=== ПОЗИЦИЯ КАМЕРЫ ===")]
    public Vector3 weaponCameraOffset = new Vector3(0f, 0f, 0f);

    void LateUpdate()
    {
        if (weaponCamera != null && weaponPivot != null)
        {
            weaponCamera.transform.position = weaponPivot.position + weaponCameraOffset;
            weaponCamera.transform.rotation = weaponPivot.rotation;
        }
    }

    void Start()
    {
        if (weaponPivot != null)
        {
            int weaponLayer = LayerMask.NameToLayer("Weapon");
            if (weaponLayer != -1)
            {
                SetLayerRecursive(weaponPivot.gameObject, weaponLayer);
                Debug.Log("✅ Слой Weapon назначен!");
            }
        }
    }

    void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    public void SetWeaponFOV(float fov)
    {
        if (weaponCamera != null)
        {
            weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, fov, Time.deltaTime * 5f);
        }
    }
}