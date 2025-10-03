using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverfortGames.FirstPersonController.Test
{
    public class RespawnerTest : MonoBehaviour
    {
        [SerializeField]
        private Transform spawnPoint;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<FirstPersonController>(out var controller))
            {
                controller.Teleport(spawnPoint.position);
            }
        }
    }

}
