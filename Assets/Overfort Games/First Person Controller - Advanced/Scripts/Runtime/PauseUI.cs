using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace OverfortGames.FirstPersonController
{
    public class PauseUI : MonoBehaviour
    {
        [Space(5), Header("References")]

        public Canvas canvas;

        public GameObject optionsPanel;

        public GameObject settingsPanel;

        public GameObject SelectionMenu;

        public Slider mouseSensitivitySlider;
        public TMP_InputField mouseSensitivityInputField;
        public Slider fovSlider;
        public TMP_InputField fovInputField;

        public Slider volumeSlider;
        public TMP_InputField volumeInputField;

        private bool inPause;

        public float pauseTimeScaleLerpSpeed = 10;

        [Space(5), Header("Mouse Sensitivity")]

        public float mouseSensitivityMaxValue = 160;
        public float mouseSensitivityMinValue = 10;

        [Space(5), Header("FOV")]

        public float fovMaxValue = 100;
        public float fovMinValue = 60;

        private float mouseSensitivityDefaultValue;
        private float fovDefaultValue;

        private CameraController cameraController;

        private void Awake()
        {
            cameraController = FindAnyObjectByType<CameraController>();

            StartCoroutine(Co_UpdateUnscaled());
        }

        private void Start()
        {
            optionsPanel.SetActive(true);
            settingsPanel.SetActive(false);
            Resume();

            mouseSensitivityDefaultValue = cameraController.GetCameraSpeed();
            fovDefaultValue = Camera.VerticalToHorizontalFieldOfView(cameraController.GetCamera().fieldOfView, cameraController.GetCamera().aspect);

            mouseSensitivitySlider.value = (mouseSensitivityDefaultValue - mouseSensitivityMinValue) / (mouseSensitivityMaxValue - mouseSensitivityMinValue);
            fovSlider.value = (fovDefaultValue - fovMinValue) / (fovMaxValue - fovMinValue);
            volumeSlider.value = AudioListener.volume;

            mouseSensitivityInputField.text = mouseSensitivityDefaultValue.ToString("F0");
            fovInputField.text = fovDefaultValue.ToString("F0");
            volumeInputField.text = AudioListener.volume.ToString("F0");

            mouseSensitivitySlider.onValueChanged.AddListener((x) =>
            {
                float newValue = ((mouseSensitivityMaxValue - mouseSensitivityMinValue) * x) + mouseSensitivityMinValue;
                cameraController.SetCameraSpeed(newValue);
                mouseSensitivityInputField.text = newValue.ToString("F0");
            }
            );

            fovSlider.onValueChanged.AddListener((x) =>
            {
                float newValue = ((fovMaxValue - fovMinValue) * x) + fovMinValue;
                cameraController.SetDefaultFOV(Camera.HorizontalToVerticalFieldOfView(newValue, cameraController.GetCamera().aspect));
                fovInputField.text = newValue.ToString("F0");
            }
            );

            volumeSlider.onValueChanged.AddListener((x) =>
            {
                float newValue = x;
                AudioListener.volume = x;
                volumeInputField.text = newValue.ToString("0.0");
            }
             );

            SetVolumeDefaultValue();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !SelectionMenu.activeSelf)
            {
                if (optionsPanel.activeSelf)
                {
                    TogglePause(!inPause);
                }
                else
                if (settingsPanel.activeSelf)
                {
                    GoToOptionsPanel();
                }
            }
        }


        private float previousUnscaledDeltaTime;
        private IEnumerator Co_UpdateUnscaled()
        {
            while (true)
            {
                float unscaledDeltaTime = Time.realtimeSinceStartup - previousUnscaledDeltaTime;

                previousUnscaledDeltaTime = Time.realtimeSinceStartup;

                if (inPause)
                {
                    Time.timeScale = Mathf.Lerp(Time.timeScale, 0, 1 * pauseTimeScaleLerpSpeed);
                }

                yield return null;
            }
        }

        private void TogglePause(bool active)
        {
            inPause = active;

            canvas.enabled = inPause;

            Cursor.visible = inPause;
            Cursor.lockState = inPause ? CursorLockMode.None : CursorLockMode.Locked;

            if (!inPause)
                Time.timeScale = 1;

            if (inPause)
                AudioLibrary.MuteAll2D();
            else
                AudioLibrary.UnmuteAll2D();

        }

        public void Resume()
        {
            TogglePause(false);
        }

        public void GoToOptionsPanel()
        {
            settingsPanel.SetActive(false);
            optionsPanel.SetActive(true);
        }

        public void GoToSettingsPanel()
        {
            settingsPanel.SetActive(true);
            optionsPanel.SetActive(false);
        }

        public void SetMouseSensitivityDefaultValue()
        {
            cameraController.SetCameraSpeed(fovDefaultValue);
            mouseSensitivitySlider.value = (mouseSensitivityDefaultValue - mouseSensitivityMinValue) / (mouseSensitivityMaxValue - mouseSensitivityMinValue);
            mouseSensitivityInputField.text = mouseSensitivityDefaultValue.ToString("F0");
        }

        public void SetFOVDefaultValue()
        {
            cameraController.SetDefaultFOV(Camera.HorizontalToVerticalFieldOfView(fovDefaultValue, cameraController.GetCamera().aspect));
            fovSlider.value = (fovDefaultValue - fovMinValue) / (fovMaxValue - fovMinValue);
            fovInputField.text = fovDefaultValue.ToString("F0");
        }

        public void SetVolumeDefaultValue()
        {
            volumeSlider.value = 0.5f;
            AudioListener.volume = 0.5f;
            volumeInputField.text = 0.5.ToString();
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
      Application.Quit();
#endif
        }
    }

}