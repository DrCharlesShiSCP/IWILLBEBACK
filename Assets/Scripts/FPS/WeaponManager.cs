// WeaponManager.cs
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using TMPro;

public class WeaponManager : MonoBehaviour
{
    [Header("Active Weapon")]
    [SerializeField] private HitscanWeapon currentWeapon;

    [Header("Setup")]
    [SerializeField] private Transform weaponParent;              // where weapon GameObjects live (optional; defaults to this.transform)
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float adsFov = 55f;

    [Header("Defaults")]
    [SerializeField] private WeaponData defaultWeaponData;        // ¡û assign your Pistol datasheet here

    [Header("HUD")]
    [SerializeField] private TMP_Text ammoHud;                    // drag a TMP Text here

    private readonly List<WeaponData> _owned = new();             // simple inventory of datasheets
    private float defaultFov;

    void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerCamera) defaultFov = playerCamera.fieldOfView;
        if (!weaponParent) weaponParent = transform;
    }

    void Start()
    {
        // Auto-equip default pistol if nothing active
        if (!currentWeapon && defaultWeaponData)
        {
            AddWeapon(defaultWeaponData, equipNow: true);
        }
        else
        {
            UpdateHud();
        }
    }

    void Update()
    {
        if (!currentWeapon) return;

        // ADS
        bool isADS = Input.GetMouseButton(1);
        currentWeapon.IsADS = isADS;
        if (playerCamera)
        {
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                isADS ? adsFov : defaultFov,
                Time.deltaTime * 12f
            );
        }

        // Fire
        if (currentWeapon.CanHoldToFire())
        {
            if (Input.GetMouseButton(0)) currentWeapon.TryFire();
        }
        else
        {
            if (Input.GetMouseButtonDown(0)) currentWeapon.TryFire();
        }

        // Reload
        if (Input.GetKeyDown(KeyCode.R)) currentWeapon.StartReload();

        // HUD
        UpdateHud();
    }

    void UpdateHud()
    {
        if (!ammoHud || !currentWeapon) return;

        var data = currentWeapon.Data;

        // Base display
        string display = $"{data.weaponName}  {currentWeapon.AmmoInMag}/{data.magazineSize}";

        // Add special states
        if (currentWeapon.IsReloading)
        {
            display = $"{data.weaponName}  (Reloading...)  {currentWeapon.AmmoInMag}/{data.magazineSize}";
        }
        else if (currentWeapon.AmmoInMag <= 0)
        {
            display += "   <color=#FF4040>Press R to Reload!</color>";
        }

        ammoHud.text = display;
    }

    // ---------- Public API (used by RollDiceManager) ----------

    /// <summary>Adds a WeaponData to inventory; optionally equips immediately.</summary>
    public void AddWeapon(WeaponData data, bool equipNow = false)
    {
        if (!data) return;
        if (!_owned.Contains(data)) _owned.Add(data);
        if (equipNow) Equip(data);
    }

    /// <summary>Equip by datasheet (spawns a HitscanWeapon child and wires it up).</summary>
    public void Equip(WeaponData data)
    {
        if (!data) return;

        // destroy old
        if (currentWeapon) Destroy(currentWeapon.gameObject);

        // spawn new holder
        var go = new GameObject($"Weapon_{data.name}");
        go.transform.SetParent(weaponParent, false);

        // components
        var weapon = go.AddComponent<HitscanWeapon>();
        var src = go.AddComponent<AudioSource>();

        // Private field injection (keeps your HitscanWeapon unchanged)
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;

        weapon.GetType().GetField("data", flags)?.SetValue(weapon, data);
        weapon.GetType().GetField("audioSource", flags)?.SetValue(weapon, src);

        // Camera (if HitscanWeapon's serialized field is private)
        weapon.GetType().GetField("playerCamera", flags)?.SetValue(weapon, playerCamera);

        currentWeapon = weapon;
        UpdateHud();
    }

    /// <summary>Directly equip an already-instantiated HitscanWeapon.</summary>
    public void Equip(HitscanWeapon weapon)
    {
        if (currentWeapon && currentWeapon != weapon)
            Destroy(currentWeapon.gameObject);

        currentWeapon = weapon;
        UpdateHud();
    }
}
