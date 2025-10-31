// EnemyController.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private EnemyData data;

    [Header("Targeting")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Transform firePoint; // where bullets spawn; fallback = transform

    private Transform _player;
    private float _shootCooldown;
    private float _currentHealth;

    void Awake()
    {
        _currentHealth = data != null ? data.maxHealth : 10f;
        if (firePoint == null) firePoint = transform;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null) _player = p.transform;
    }

    void Update()
    {
        if (_player == null || data == null || data.projectilePrefab == null) return;

        // Aim at the combined center of all the player's colliders
        Vector3 targetPos = GetColliderCenter(_player);
        Vector3 toTarget = targetPos - transform.position;
        float dist = toTarget.magnitude;

        // Face the target (keep upright)
        if (toTarget.sqrMagnitude > 0.001f)
        {
            Vector3 flatDir = new Vector3(toTarget.x, 0f, toTarget.z);
            if (flatDir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(flatDir.normalized, Vector3.up);
        }

        // Shoot when in range & off cooldown
        if (dist <= data.attackRange)
        {
            _shootCooldown -= Time.deltaTime;
            if (_shootCooldown <= 0f)
            {
                ShootAt(targetPos); // use collider center, not root
                _shootCooldown = 1f / Mathf.Max(0.01f, data.shotsPerSecond);
#if UNITY_EDITOR
                Debug.Log($"Enemy shooting at {Time.time}");
#endif
            }
        }
    }

    private Vector3 GetColliderCenter(Transform t)
    {
        var cols = t.GetComponentsInChildren<Collider>();
        if (cols.Length == 0) return t.position;

        var bounds = cols[0].bounds;
        for (int i = 1; i < cols.Length; i++)
            bounds.Encapsulate(cols[i].bounds);
        return bounds.center; // shoots toward combined collider center
    }

    private void ShootAt(Vector3 targetPos)
    {
        GameObject projGo = Instantiate(data.projectilePrefab, firePoint.position, Quaternion.identity);

        // Ignore self-collision so we don't immediately hit our own colliders
        var myCols = GetComponentsInChildren<Collider>();
        var projCol = projGo.GetComponent<Collider>();
        if (projCol != null)
        {
            foreach (var c in myCols) Physics.IgnoreCollision(projCol, c, true);
        }

        Vector3 dir = (targetPos - firePoint.position).normalized;
        projGo.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        if (projGo.TryGetComponent<Projectile>(out var proj))
        {
            float dmg = data.projectileDamage;
            if (Balance.Instance) dmg *= Balance.Instance.enemyDamageMultiplier; // ¡û add this
            proj.Init(dir, data.projectileSpeed, dmg, gameObject);
        }


        if (data.shootSfx) AudioSource.PlayClipAtPoint(data.shootSfx, firePoint.position);
    }

    // Optional if you later want enemies to take damage
    public void TakeDamage(float dmg)
    {
        _currentHealth -= dmg;
        if (_currentHealth <= 0f) Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.attackRange);
    }
#endif
}
