using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Vitals")]
    public float maxHealth = 10f;

    [Header("Combat")]
    public float attackRange = 12f;       // how close the player must be to shoot
    public float shotsPerSecond = 0.5f;   // 0.5 = 1 shot every 2 seconds
    public float projectileSpeed = 18f;
    public float projectileDamage = 5f;

    [Header("VFX / SFX / Prefabs")]
    public GameObject projectilePrefab;   // must have a Rigidbody + Projectile script
    public AudioClip shootSfx;
}
