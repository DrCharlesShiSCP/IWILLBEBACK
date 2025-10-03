using System.Collections;
using UnityEngine;

public class HitscanWeapon : MonoBehaviour
{
    [SerializeField] private WeaponData data;
    [SerializeField] private Transform muzzle;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Camera playerCamera;

    public int AmmoInMag { get; private set; }
    public bool IsReloading { get; private set; }
    public bool IsADS { get; set; }

    float nextFireTime;

    void Awake()
    {
        if (!playerCamera) playerCamera = Camera.main;
        AmmoInMag = data.magazineSize;
        if (!audioSource) audioSource = GetComponentInChildren<AudioSource>();
    }

    public void TryFire()
    {
        if (IsReloading) return;

        if (Time.time < nextFireTime) return;

        if (AmmoInMag <= 0)
        {
            if (data.dryFireSfx && audioSource) audioSource.PlayOneShot(data.dryFireSfx);
            return;
        }

        FireOneShot();
    }

    public void StartReload()
    {
        if (IsReloading) return;
        if (AmmoInMag == data.magazineSize) return;
        StartCoroutine(Co_Reload());
    }

    void FireOneShot()
    {
        nextFireTime = Time.time + (1f / Mathf.Max(0.01f, data.fireRate));
        AmmoInMag--;

        //degrees -> small random cone
        float spread = IsADS ? data.spreadADS : data.spreadHip;
        Vector2 offset = Random.insideUnitCircle * spread;
        Quaternion spreadRot = Quaternion.Euler(offset.y, offset.x, 0f);
        Vector3 dir = spreadRot * playerCamera.transform.forward;

        // Raycast
        if (Physics.Raycast(playerCamera.transform.position, dir, out RaycastHit hit, data.range, data.hitMask, QueryTriggerInteraction.Ignore))
        {
            // Damage
            var health = hit.collider.GetComponentInParent<Health>();
            if (health) health.TakeDamage(data.damage);

            // Impact VFX
            if (data.impactVfxPrefab)
            {
                var vfx = Instantiate(data.impactVfxPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(vfx, 3f);
            }
        }

        // Muzzle VFX
        if (data.muzzleFlashPrefab && muzzle)
        {
            var fx = Instantiate(data.muzzleFlashPrefab, muzzle.position, muzzle.rotation, muzzle);
            Destroy(fx, 2f);
        }

        // SFX
        if (data.shotSfx && audioSource) audioSource.PlayOneShot(data.shotSfx);

        //recoil
        if (playerCamera)
        {
            Vector3 eulers = playerCamera.transform.localEulerAngles;
            eulers.x = WrapAngle(eulers.x) - data.recoilKick; // negative to look up slightly
            playerCamera.transform.localEulerAngles = eulers;
        }
    }

    IEnumerator Co_Reload()
    {
        IsReloading = true;
        if (data.reloadSfx && audioSource) audioSource.PlayOneShot(data.reloadSfx);
        yield return new WaitForSeconds(data.reloadTime);
        AmmoInMag = data.magazineSize;
        IsReloading = false;
    }

    static float WrapAngle(float a) => (a > 180f) ? a - 360f : a;

    //automatic
    public bool CanHoldToFire() => data.automatic;
}
