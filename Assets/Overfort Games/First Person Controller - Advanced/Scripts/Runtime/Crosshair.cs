using UnityEngine;
using UnityEngine.UI;

namespace OverfortGames.FirstPersonController
{
    public class Crosshair : MonoBehaviour
    {
        private static Crosshair _instance;

        [SerializeField]
        private Image crosshair;

        [SerializeField]
        private Image crosshairFill;

        private Color defaultCrosshairColor;

        private void Awake()
        {
            if (_instance != null)
                Debug.Log("More than one Crosshair instances found");

            _instance = this;

            if(crosshair != null)
                defaultCrosshairColor = crosshair.color;

            HideCrosshairFill();
        }

        public static void SetCrosshairColor(Color color, bool ignoreAlpha = false)
        {
            if (_instance.crosshair == null)
                return;

            if (ignoreAlpha)
            {
                _instance.crosshair.color = new Color(color.r, color.g, color.b, _instance.crosshair.color.a);
            }
            else
            {
                _instance.crosshair.color = color;
            }
        }

        public static void SetCrosshairToDefaultColor()
        {
            if (_instance.crosshair == null)
                return;

            _instance.crosshair.color = _instance.defaultCrosshairColor;
        }

        public static void SetCrosshairFillColor(Color color, bool ignoreAlpha = false)
        {
            if (_instance.crosshairFill == null)
                return;

            if (ignoreAlpha)
            {
                _instance.crosshairFill.color = new Color(color.r, color.g, color.b, _instance.crosshairFill.color.a);
            }
            else
            {
                _instance.crosshairFill.color = color;
            }
        }

        public static void ShowCrosshairFillAndSetColor(Color color, bool ignoreAlpha = false)
        {
            if (_instance.crosshairFill == null)
                return;

            _instance.crosshairFill.enabled = true;
            if (ignoreAlpha)
            {
                _instance.crosshairFill.color = new Color(color.r, color.g, color.b, _instance.crosshairFill.color.a);
            }
            else
            {
                _instance.crosshairFill.color = color;
            }
        }

        public static void HideCrosshairFill()
        {
            if (_instance.crosshairFill == null)
                return;

            _instance.crosshairFill.enabled = false;
        }
    }
}
