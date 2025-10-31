using TMPro;
using UnityEngine;

namespace OverfortGames.FirstPersonController
{
    public class SpeedUI : MonoBehaviour
    {
        FirstPersonController controller;

        public TextMeshProUGUI text;
        public string suffix = "u/s";

        private void Awake()
        {
            controller = FindAnyObjectByType<FirstPersonController>();
        }

        private void LateUpdate()
        {
            Vector3 velocity = controller.GetVelocity();
            velocity.y = 0;
            text.text = velocity.magnitude.ToString("0.0") + suffix;
        }
    }

}
