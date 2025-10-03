using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace OverfortGames.FirstPersonController
{
#if ENABLE_INPUT_SYSTEM
    [System.Serializable]
    public class CameraInputSystemSettings
    {
        public InputActionReference lookInputAction;
    }
#endif

    public class CameraInput : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        private string mouseHorizontalInput = "Mouse X";

        [SerializeField]
        private string mouseVerticalInput = "Mouse Y";

        //The inputs will be multiplied by this value
        [SerializeField]
        private float sensitivity = 0.01f;

        [SerializeField]
        private bool invertInputHorizontal;

        [SerializeField]
        private bool invertInputVertical;

#if ENABLE_INPUT_SYSTEM
        public bool useInputSystem = false;

        [SerializeField]
        private CameraInputSystemSettings inputSystemSettings;
#endif

        #endregion

        #region Methods

        public float GetHorizontal()
        {
            float input = 0;

#if ENABLE_INPUT_SYSTEM
            if (useInputSystem)
            {
                input = inputSystemSettings.lookInputAction.action.ReadValue<Vector2>().x;
            }
            else
#endif
            {
                input = Input.GetAxisRaw(mouseHorizontalInput);

                //Adjust input on timeScale
                if (Time.timeScale > 0f)
                {
                    if (Time.deltaTime > 0)
                        input /= Time.deltaTime;

                    input *= Time.timeScale;
                }
                else
                {
                    input = 0;
                }


            }

            //Apply sensitivity
            input *= sensitivity;

            if (invertInputHorizontal)
                input *= -1f;

            return input;
        }

        public float GetVertical()
        {
            float input = 0;

#if ENABLE_INPUT_SYSTEM
            if (useInputSystem)
            {
                input = inputSystemSettings.lookInputAction.action.ReadValue<Vector2>().y;
            }
            else
#endif
            {
                //Get inverted input - this will result example in: move mouse up - look up ; move mouse down - look down
                input = -Input.GetAxisRaw(mouseVerticalInput);

                //Adjust input on timeScale
                if (Time.timeScale > 0)
                {
                    if (Time.deltaTime > 0)
                        input /= Time.deltaTime;

                    input *= Time.timeScale;
                }
                else
                {
                    return input;
                }
            }

            //Apply sensitivity
            input *= sensitivity;

            if (invertInputVertical)
                input *= -1f;

            return input;
        }

        #endregion
    }
}
