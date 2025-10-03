using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverfortGames.FirstPersonController.Test
{
    public class OnTriggerDebugBreak : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Debug.Break();
        }
    }

}
