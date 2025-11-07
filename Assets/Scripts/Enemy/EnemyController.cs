// EnemyController.cs
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]
public class EnemyController : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyData data;

    [Header("Targeting")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Transform firePoint; // where bullets spawn; fallback = transform

    private Transform _player;
    private Vector3 _spawnPos;
    private Quaternion _spawnRot;
    private float _maxHealth;
    private float _currentHealth;
    private float _shootCooldown;
    private Rigidbody _rb;
    private NavMeshAgent _agent;

    void Awake()
    {
        _spawnPos = transform.position;
        _spawnRot = transform.rotation;

        _maxHealth = (data != null) ? data.maxHealth : 10f;
        _currentHealth = _maxHealth;

        if (!firePoint) firePoint = transform;
        _rb = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p) _player = p.transform;
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
                Debug.Log($"[EnemyController] Shoot @ {Time.time:0.00}");
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
        // Spawn projectile
        GameObject projGo = Instantiate(data.projectilePrefab, firePoint.position, Quaternion.identity);

        // Ignore self-collision so we don't immediately hit our own colliders
        var myCols = GetComponentsInChildren<Collider>();
        var projCol = projGo.GetComponent<Collider>();
        if (projCol != null)
        {
            foreach (var c in myCols) Physics.IgnoreCollision(projCol, c, true);
        }

        // Orient and init projectile
        Vector3 dir = (targetPos - firePoint.position).normalized;
        projGo.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        if (projGo.TryGetComponent<Projectile>(out var proj))
        {
            // Your Projectile.cs exposes: Init(Vector3 direction, float speed, float damage, GameObject owner)
            proj.Init(dir, data.projectileSpeed, data.projectileDamage, gameObject);
        }

        // Audio (optional)
        if (data.shootSfx)
            AudioSource.PlayClipAtPoint(data.shootSfx, firePoint.position);
    }

    public void ResetToSpawn()
    {
        gameObject.SetActive(true);

        // transform reset
        transform.SetPositionAndRotation(_spawnPos, _spawnRot);

        // physics reset
        if (_rb)
        {
#if UNITY_6000_0_OR_NEWER || UNITY_2023_3_OR_NEWER
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
#else
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
#endif
        }
        if (_agent)
        {
            _agent.Warp(_spawnPos);
            _agent.ResetPath();
        }

        // combat state reset
        _currentHealth = _maxHealth;
        _shootCooldown = 0f;
        enabled = true;
    }

    public void TakeDamage(float dmg)
    {
        _currentHealth -= dmg;
        if (_currentHealth <= 0f) Die();
    }

    private void Die()
    {
        // disable (not destroy) so EnemyResetManager can bring it back
        gameObject.SetActive(false);
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
