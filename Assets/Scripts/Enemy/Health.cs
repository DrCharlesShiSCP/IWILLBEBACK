using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    float hp;

    void Awake() => hp = maxHealth;

    public void TakeDamage(float amount)
    {
        hp -= amount;
        if (hp <= 0f) Die();
    }

    void Die()
    {
        // placeholder ¨C replace with ragdoll, despawn, etc.
        Destroy(gameObject);
    }
}
