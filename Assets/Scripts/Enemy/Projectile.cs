using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;   // safety despawn

    private Rigidbody _rb;
    private float _damage;
    private GameObject _owner;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        // Recommended settings for fast trigger projectiles
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.interpolation = RigidbodyInterpolation.None;
    }

    public void Init(Vector3 direction, float speed, float damage, GameObject owner)
    {
        _damage = damage;
        _owner = owner;

        SetVelocity(direction.normalized * speed);
        CancelInvoke();
        Invoke(nameof(Despawn), lifetime);
    }

    // ！！ Trigger path (Collider must be IsTrigger = true) ！！
    void OnTriggerEnter(Collider other)
    {
        // Ignore owner (self) collisions
        if (other.attachedRigidbody && other.attachedRigidbody.gameObject == _owner) return;

        // If we touched the player (root or any child), apply damage then destroy
        if (IsPlayer(other.gameObject))
        {
            TryDamage(other.gameObject);
            Despawn();
            return;
        }

        // Any other collider: just destroy self (walls, props, enemies, etc.)
        Despawn();
    }

    private bool IsPlayer(GameObject go)
    {
        // Tag check first (fast), then fall back to components on parent chain
        if (go.CompareTag("Player")) return true;

        // If your player isn¨t tagged or hit a child, climb the hierarchy
        if (go.GetComponentInParent<PlayerHealth>() != null) return true;
        if (go.GetComponentInParent<IDamageable>() != null) return true;

        return false;
    }

    private void TryDamage(GameObject target)
    {
        // This object´
        if (target.TryGetComponent<IDamageable>(out var d))
        {
            d.TakeDamage(_damage);
            return;
        }

        // ´or any parent (for child hitboxes under the player root)
        var parentD = target.GetComponentInParent<IDamageable>();
        if (parentD != null)
        {
            parentD.TakeDamage(_damage);
        }
    }

    private void Despawn()
    {
        Destroy(gameObject);
    }

    // ！！！ velocity helper (uses linearVelocity when available) ！！！
    private void SetVelocity(Vector3 v)
    {
#if UNITY_6000_0_OR_NEWER || UNITY_2023_3_OR_NEWER
        _rb.linearVelocity = v;
#else
        _rb.velocity = v;
#endif
    }
}
