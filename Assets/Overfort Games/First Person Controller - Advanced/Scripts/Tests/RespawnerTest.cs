using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverfortGames.FirstPersonController.Test
{
    public class RespawnerTest : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            // Check if the object entering has a PlayerHealth component
            if (other.TryGetComponent<PlayerHealth>(out var health))
            {
                // Set player health to zero
                health.currentHealth = 0;

                // Optionally, if your PlayerHealth has a method to handle death:
                // health.TakeDamage(health.maxHealth);
                // or
                // health.Die();
            }
        }
    }
}
