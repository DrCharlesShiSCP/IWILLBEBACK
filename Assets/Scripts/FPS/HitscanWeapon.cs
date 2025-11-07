using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class HitscanWeapon : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField] private WeaponData data;           // bound at runtime by BindData
    [SerializeField] private Camera playerCamera;       // set by WeaponManager or fallback to Camera.main
    [SerializeField] private AudioSource audioSource;   // local audio source

    public int AmmoInMag { get; private set; }
    private float nextFireTime;

    void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!playerCamera) playerCamera = Camera.main;
        // Ammo will be assigned by BindData; keep zero-safe default here.
        if (data != null && AmmoInMag == 0) AmmoInMag = data.magazineSize;
    }

    // === NEW: called by WeaponManager whenever we "equip" a new weapon ===
    public void BindData(WeaponData newData, Camera cam = null, AudioSource src = null, bool refillMag = true)
    {
        data = newData;
        if (cam) playerCamera = cam;
        if (src) audioSource = src;
        if (!playerCamera) playerCamera = Camera.main;
        if (!audioSource) audioSource = GetComponent<AudioSource>();

        if (refillMag) AmmoInMag = data.magazineSize;
        nextFireTime = 0f;
    }

    public string DisplayName => data ? data.weaponName : "No Weapon";
    public int MagazineSize => data ? data.magazineSize : 0;

    public bool CanFire()
    {
        if (!data) return false;
        if (Time.time < nextFireTime) return false;
        if (AmmoInMag <= 0) return false;
        return true;
    }

    public void TryFire()
    {
        if (!data) return;
        if (!CanFire())
        {
            // dry fire (one shot per click)
            if (AmmoInMag <= 0 && data.dryFireSfx != null && audioSource)
                audioSource.PlayOneShot(data.dryFireSfx);
            return;
        }

        // fire
        AmmoInMag--;
        nextFireTime = Time.time + 1f / Mathf.Max(0.01f, data.fireRate);

        // play SFX
        if (data.shotSfx && audioSource) audioSource.PlayOneShot(data.shotSfx);

        // hitscan
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out var hit, data.range, data.hitMask))
        {
            if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
            {
                dmg.TakeDamage(data.damage);
            }
        }
    }

    public bool IsReloading { get; private set; }

    public void StartReload()
    {
        if (!data || IsReloading) return;
        if (AmmoInMag >= data.magazineSize) return;
        StartCoroutine(Co_Reload());
    }

    private IEnumerator Co_Reload()
    {
        IsReloading = true;
        if (data.reloadSfx && audioSource) audioSource.PlayOneShot(data.reloadSfx);
        yield return new WaitForSeconds(data.reloadTime);
        AmmoInMag = data.magazineSize;
        IsReloading = false;
    }
}
