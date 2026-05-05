using UnityEngine;
using System.Collections;

public class WeaponController : MonoBehaviour
{
    [Header("=== ОГОНЬ ===")]
    public float fireRate = 600f;
    public int maxAmmo = 30;

    [Header("=== АНИМАТОРЫ ===")]
    public Animator animator1;
    public Animator animator2;

    [Header("=== ССЫЛКИ ===")]
    public Transform muzzlePoint;
    public Camera playerCamera;
    public ParticleSystem muzzleFlash;
    public AudioClip shootSound;
    public AudioSource audioSource;

    [Header("=== ВСПЫШКА (СПРАЙТ) ===")]
    public MuzzleFlashSprite muzzleFlashSprite;

    [Header("=== ДЕКАЛИ ===")]
    public WeaponDecal weaponDecal;

    [Header("=== ОТДАЧА ОРУЖИЯ ===")]
    public WeaponRecoil weaponRecoil;

    // 🔥 УДАЛЕНО: public CameraRecoil cameraRecoil; (чтобы не было ошибки)

    [Header("=== ПРИЦЕЛИВАНИЕ (ADS) ===")]
    [Tooltip("Кнопка прицеливания")]
    public KeyCode aimKey = KeyCode.Mouse1;

    [Tooltip("FOV при прицеливании")]
    public float aimFOV = 40f;

    [Tooltip("Скорость смены FOV")]
    public float aimFOVSpeed = 10f;

    [Header("=== ЗВУК ===")]
    [Range(0f, 1f)]
    public float shootVolume = 1f;

    [Range(0f, 0.5f)]
    public float pitchRandom = 0.1f;

    [Header("=== ПЕРЕЗАРЯДКА ===")]
    public float reloadTime = 2f;
    public KeyCode reloadKey = KeyCode.R;

    [Header("=== ЗАЩИТА ОТ РАССИНХРОНА ===")]
    public float minTriggerInterval = 0.05f;

    [Header("=== FPS ===")]
    public int targetFPS = -1;

    // Внутренние переменные
    private int currentAmmo;
    private bool isReloading = false;
    private bool isFiring = false;
    private float nextFireTime = 0f;
    private float fireInterval;
    private float lastTriggerTime = 0f;

    // Переменные прицеливания
    private bool isAiming = false;
    private float originalFOV;

    // Хэши
    private static readonly int Fire1Hash = Animator.StringToHash("Fire1");
    private static readonly int Fire2Hash = Animator.StringToHash("Fire2");
    private static readonly int Fire3Hash = Animator.StringToHash("Fire3");
    private static readonly int StopFireHash = Animator.StringToHash("StopFire");
    private static readonly int ReloadHash = Animator.StringToHash("Reload");
    private static readonly int IsReloadingHash = Animator.StringToHash("IsReloading");
    private static readonly int AimHash = Animator.StringToHash("Aim");

    void Start()
    {
        currentAmmo = maxAmmo;

        if (animator1 == null) animator1 = GetComponent<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        if (playerCamera == null) playerCamera = Camera.main;

        // Сохраняем оригинальный FOV
        originalFOV = playerCamera.fieldOfView;

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS;

        fireInterval = 60f / fireRate;
    }

    void Update()
    {
        // 🔥 1. Прицеливание (ПКМ)
        HandleAiming();

        // 🔥 2. Перезарядка (R)
        if (Input.GetKeyDown(reloadKey) && !isReloading && currentAmmo < maxAmmo)
        {
            StartCoroutine(ReloadCoroutine());
            return;
        }

        // 🔥 3. Стрельба (ЛКМ)
        if (!isReloading && currentAmmo > 0)
        {
            bool wantToFire = Input.GetButton("Fire1");

            if (wantToFire && !isFiring)
            {
                isFiring = true;
            }
            else if (!wantToFire && isFiring)
            {
                isFiring = false;
                StopFire();
            }

            if (isFiring && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireInterval;
            }
        }
        else if (currentAmmo <= 0 && !isReloading)
        {
            if (Input.GetButton("Fire1"))
            {
                StartCoroutine(ReloadCoroutine());
            }
        }
    }

    // 🔥 ОБРАБОТКА ПРИЦЕЛИВАНИЯ
    void HandleAiming()
    {
        bool wantToAim = Input.GetKey(aimKey);

        if (wantToAim && !isAiming)
        {
            isAiming = true;
            SetBoolOnBoth(AimHash, true);
        }
        else if (!wantToAim && isAiming)
        {
            isAiming = false;
            SetBoolOnBoth(AimHash, false);
        }

        // Плавная смена FOV
        float targetFOV = isAiming ? aimFOV : originalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * aimFOVSpeed);
    }

    void Shoot()
    {
        if (Time.time - lastTriggerTime < minTriggerInterval)
        {
            return;
        }
        lastTriggerTime = Time.time;

        currentAmmo--;

        if (muzzleFlashSprite != null)
        {
            muzzleFlashSprite.ShowFlash();
        }
        else if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        if (shootSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(1f - pitchRandom, 1f + pitchRandom);
            audioSource.volume = shootVolume;
            audioSource.PlayOneShot(shootSound);
        }

        int animVariant = Random.Range(1, 4);

        if (animVariant == 1)
        {
            ResetTriggerOnBoth(Fire1Hash);
            SetTriggerOnBoth(Fire1Hash);
        }
        else if (animVariant == 2)
        {
            ResetTriggerOnBoth(Fire2Hash);
            SetTriggerOnBoth(Fire2Hash);
        }
        else
        {
            ResetTriggerOnBoth(Fire3Hash);
            SetTriggerOnBoth(Fire3Hash);
        }

        // 🔥 Отдача оружия (если есть)
        if (weaponRecoil != null)
        {
            weaponRecoil.AddRecoil();
        }

        // 🔥 УДАЛЕНО: cameraRecoil.AddRecoil();

        Vector3 rayOrigin = muzzlePoint != null ? muzzlePoint.position : playerCamera.transform.position;
        Vector3 rayDir = muzzlePoint != null ? muzzlePoint.forward : playerCamera.transform.forward;

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDir, out hit, 100f))
        {
            if (weaponDecal != null)
            {
                weaponDecal.SpawnDecal(hit.point, hit.normal, hit.collider.gameObject);
            }
        }
    }

    void StopFire()
    {
        ResetTriggerOnBoth(StopFireHash);
        SetTriggerOnBoth(StopFireHash);

        if (muzzleFlashSprite != null)
        {
            muzzleFlashSprite.ForceHide();
        }
    }

    IEnumerator ReloadCoroutine()
    {
        isReloading = true;

        SetBoolOnBoth(IsReloadingHash, true);
        SetTriggerOnBoth(ReloadHash);

        if (weaponRecoil != null)
        {
            weaponRecoil.ResetRecoil();
        }

        // 🔥 УДАЛЕНО: cameraRecoil.ResetRecoil();

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        SetBoolOnBoth(IsReloadingHash, false);
    }

    void SetBoolOnBoth(int hash, bool value)
    {
        if (animator1 != null) animator1.SetBool(hash, value);
        if (animator2 != null) animator2.SetBool(hash, value);
    }

    void SetTriggerOnBoth(int hash)
    {
        if (animator1 != null) animator1.SetTrigger(hash);
        if (animator2 != null) animator2.SetTrigger(hash);
    }

    void ResetTriggerOnBoth(int hash)
    {
        if (animator1 != null) animator1.ResetTrigger(hash);
        if (animator2 != null) animator2.ResetTrigger(hash);
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.green;
        GUI.Label(new Rect(10, 10, 500, 50),
            $"Ammo: {currentAmmo} / {maxAmmo} | FPS: {1.0f / Time.unscaledDeltaTime:F0} | Aim: {isAiming}", style);
    }
}