using UnityEngine;

[CreateAssetMenu(menuName = "FPS/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName = "Rifle";

    [Header("Ballistics")]
    public float damage = 20f;
    public float range = 200f;             // meters
    public float fireRate = 10f;           // shots/sec
    public bool automatic = true;

    [Header("Accuracy & Feel")]
    [Range(0f, 10f)] public float spreadHip = 1.5f;  // degrees
    [Range(0f, 10f)] public float spreadADS = 0.2f;  // degrees
    public float recoilKick = 0.8f;                  // quick camera kick

    [Header("Ammo")]
    public int magazineSize = 30;
    public float reloadTime = 1.8f;

    [Header("FX & Audio")]
    public GameObject muzzleFlashPrefab;
    public GameObject impactVfxPrefab;
    public AudioClip shotSfx;
    public AudioClip reloadSfx;
    public AudioClip dryFireSfx;

    [Header("Hit Mask")]
    public LayerMask hitMask = ~0; // default: everything
}
