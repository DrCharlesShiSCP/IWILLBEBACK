using UnityEngine;
using UnityEngine.UI;

namespace OverfortGames.FirstPersonController
{
    public class GrapplingHookUICooldownIndicator : MonoBehaviour
    {
        public Image filledImage;
        public Image backgroundImage;

        private FirstPersonController controller;

        private void Awake()
        {
            controller = FindAnyObjectByType<FirstPersonController>();

            if (controller.enableGrapplingHook == false)
            {
                if (filledImage.enabled)
                    filledImage.enabled = false;

                if (backgroundImage.enabled)
                    backgroundImage.enabled = false;
            }
        }

        private void LateUpdate()
        {
            if (controller.enableGrapplingHook == false)
            {
                if (filledImage.enabled)
                    filledImage.enabled = false;

                if (backgroundImage.enabled)
                    backgroundImage.enabled = false;

                return;
            }

            if (controller.IsGrapplingOnCooldown() == false)
            {
                filledImage.fillAmount = 1;
                return;
            }

            filledImage.fillAmount = (Time.time - controller.GetLastTimeGrappling()) / controller.grapplingHookSettings.cooldown;
        }
    }

}
