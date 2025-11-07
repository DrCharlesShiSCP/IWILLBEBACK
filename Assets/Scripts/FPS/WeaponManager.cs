using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeaponManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private HitscanWeapon currentWeapon;    // reference to the single, persistent weapon component
    [SerializeField] private AudioSource weaponAudio;        // optional: a dedicated audio source for weapon SFX
    private HitscanWeapon Hitscanweapon;

    [Header("Defaults & UI")]
    [SerializeField] private WeaponData defaultWeaponData;
    [SerializeField] private TMP_Text hudText;
    [SerializeField] private GameObject hinttext;// "Pistol 7/12" etc.
    [SerializeField] private TMP_Text reloadHint;            // "Press R to reload" prompt

    private readonly List<WeaponData> unlocked = new();
    private float defaultFov;

    void Awake()
    {
        if (!playerCamera) playerCamera = Camera.main;
        if (!weaponAudio) weaponAudio = GetComponentInChildren<AudioSource>();
        if (!currentWeapon)
        {
            // If you don't already have a HitscanWeapon on the player, create one once and keep it forever.
            currentWeapon = gameObject.AddComponent<HitscanWeapon>();
        }
        defaultFov = playerCamera ? playerCamera.fieldOfView : 60f;
    }

    void Start()
    {
        // ensure at least the default exists and is equipped
        if (defaultWeaponData != null && !unlocked.Contains(defaultWeaponData))
            unlocked.Add(defaultWeaponData);

        Equip(defaultWeaponData, refillMag: true);
        UpdateHud();
    }

    void Update()
    {
        // simple input example; adapt to your input system
        if (Input.GetMouseButton(0)) currentWeapon.TryFire();

        // R to reload (and show hint if mag empty)
        if (currentWeapon != null)
        {
            Debug.Log("Ammo in mag: " + currentWeapon.AmmoInMag);

            if (currentWeapon.AmmoInMag <= 0 && reloadHint)
            {
                Debug.Log("Showing reload hint");
                hinttext.SetActive(true);
                reloadHint.text = "Press R to reload";
            }
            else if (reloadHint)
            {
                reloadHint.text = "";
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                currentWeapon.StartReload();
            }
        }

        UpdateHud();
    }

    public void AddWeapon(WeaponData data, bool equipNow)
    {
        if (data == null) return;
        if (!unlocked.Contains(data)) unlocked.Add(data);
        if (equipNow) Equip(data, refillMag: true);
    }

    public void Equip(WeaponData data, bool refillMag = true)
    {
        if (!data) return;
        if (!currentWeapon)
            currentWeapon = gameObject.AddComponent<HitscanWeapon>();

        currentWeapon.BindData(data, playerCamera, weaponAudio, refillMag);
        // (Optionally) tweak FOV or sensitivity per-weapon using data if you have it
        if (playerCamera) playerCamera.fieldOfView = defaultFov;

        UpdateHud();
    }

    private void UpdateHud()
    {
        if (!hudText || currentWeapon == null) return;
        hudText.text = $"{currentWeapon.DisplayName}  {currentWeapon.AmmoInMag}/{currentWeapon.MagazineSize}";
    }

    // Call this from your respawn flow after teleport/heal to ensure state is stable
    public void OnPlayerRespawned()
    {
        Hitscanweapon = FindAnyObjectByType<HitscanWeapon>();
        // nothing to recreate¡ªcomponent persists. Just refresh HUD in case UI was rebuilt.
        UpdateHud();
        Hitscanweapon.CancelReload();
    }
}
