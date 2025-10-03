using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace OverfortGames.FirstPersonController
{
#if ENABLE_INPUT_SYSTEM
	[System.Serializable]
	public class CharacterInputSystemSettings
	{
		public InputActionReference movementInputAction;
		public InputActionReference slideInputAction;
		public InputActionReference jumpInputAction;
		public InputActionReference runInputAction;
		public InputActionReference proneInputAction;
		public InputActionReference hookInputAction;
		public InputActionReference leanLeftInputAction;
		public InputActionReference leanRightInputAction;
		public InputActionReference zoomInputAction;
	}
#endif

	public class CharacterInput : MonoBehaviour
	{
		#region Fields

		//True = no smoothing 
		[SerializeField]
		private bool useRawInput = true;

		[SerializeField]
		private string horizontalInputKey = "Horizontal";

		[SerializeField]
		private string verticalInputKey = "Vertical";

		[SerializeField]
		private string slideInputKey = "Slide";

		[SerializeField]
		private string jumpInputKey = "Jump";

		[SerializeField]
		private string runInputKey = "Run";

		[SerializeField]
		private string proneInputKey = "Prone";

		[SerializeField]
		private string hookInputKey = "Hook";

		[SerializeField]
		private string leanLeftKey = "LeanLeft";

		[SerializeField]
		private string leanRightKey = "LeanRight";

		[SerializeField]
		private string zoomKey = "Zoom";

#if ENABLE_INPUT_SYSTEM
		public bool useInputSystem = false;

		[SerializeField]
		private CharacterInputSystemSettings inputSystemSettings;
#endif

		private float horizontalInput;
		private float verticalInput;

		private bool isRunButtonBeingPressed;
		private bool isJumpButtonBeingPressed;
		private bool isJumpButtonReleased;
		private bool isJumpButtonDown;
		private bool isSlideButtonBeingPressed;
		private bool isProneButtonBeingPressed;
		private bool isProneButtonPressedDown;
		private bool isHookButtonDown;
		private bool isRunButtonPressedDown;
		private bool isRunButtonDoublePressedDown;
		private bool isLeanLeftButtonBeingPressed;
		private bool isLeanRightButtonBeingPressed;
		private bool isZoomButtonBeingPressed;

		private float lastTimeRunButtonBeingPressed;
		private float minimumTimerForDoublePress = 0.5f;

		#endregion

		#region Methods
		private void Update()
		{
#if ENABLE_INPUT_SYSTEM
			if (useInputSystem)
			{
				Vector2 movementInput = inputSystemSettings.movementInputAction.action.ReadValue<Vector2>();
				horizontalInput = movementInput.x;
				verticalInput = movementInput.y;

				isRunButtonPressedDown = inputSystemSettings.runInputAction.action.WasPressedThisFrame();
				isRunButtonBeingPressed = inputSystemSettings.runInputAction.action.IsPressed();

				isJumpButtonBeingPressed = inputSystemSettings.jumpInputAction.action.IsPressed();
				isJumpButtonReleased = inputSystemSettings.jumpInputAction.action.WasReleasedThisFrame();
				isJumpButtonDown = inputSystemSettings.jumpInputAction.action.WasPressedThisFrame();

				isSlideButtonBeingPressed = inputSystemSettings.slideInputAction.action.IsPressed();

				isProneButtonBeingPressed = inputSystemSettings.proneInputAction.action.IsPressed();
				isProneButtonPressedDown = inputSystemSettings.proneInputAction.action.WasPressedThisFrame();

				isHookButtonDown = inputSystemSettings.hookInputAction.action.WasPressedThisFrame();

				isLeanLeftButtonBeingPressed = inputSystemSettings.leanLeftInputAction.action.IsPressed();
				isLeanRightButtonBeingPressed = inputSystemSettings.leanRightInputAction.action.IsPressed();

				isZoomButtonBeingPressed = inputSystemSettings.zoomInputAction.action.IsPressed();
			}
			else
#endif
			{
				//Force positive and negative input values respectively to 1 and -1
				if (useRawInput)
				{
					horizontalInput = Input.GetAxisRaw(horizontalInputKey);
					verticalInput = Input.GetAxisRaw(verticalInputKey);
				}
				else
				{
					horizontalInput = Input.GetAxisRaw(horizontalInputKey);
					verticalInput = Input.GetAxis(verticalInputKey);
				}

				isRunButtonPressedDown = Input.GetButtonDown(runInputKey);
				isRunButtonBeingPressed = Input.GetButton(runInputKey);

				isJumpButtonBeingPressed = Input.GetButton(jumpInputKey);
				isJumpButtonReleased = Input.GetButtonUp(jumpInputKey);
				isJumpButtonDown = Input.GetButtonDown(jumpInputKey);

				isSlideButtonBeingPressed = Input.GetButton(slideInputKey);

				isProneButtonBeingPressed = Input.GetButton(proneInputKey);
				isProneButtonPressedDown = Input.GetButtonDown(proneInputKey);

				isHookButtonDown = Input.GetButtonDown(hookInputKey);

				isLeanLeftButtonBeingPressed = Input.GetButton(leanLeftKey);
				isLeanRightButtonBeingPressed = Input.GetButton(leanRightKey);

				isZoomButtonBeingPressed = Input.GetButton(zoomKey);
			}

			if (isRunButtonPressedDown)
			{
				if (lastTimeRunButtonBeingPressed + minimumTimerForDoublePress > Time.time)
				{
					isRunButtonDoublePressedDown = true;
				}

				lastTimeRunButtonBeingPressed = Time.time;
			}
		}

		private void LateUpdate()
		{
			//Reset Up/Down input flags
			isRunButtonDoublePressedDown = false;
		}

		public float GetHorizontalMovementInput()
		{
			return horizontalInput;
		}

		public float GetVerticalMovementInput()
		{
			return verticalInput;
		}

		public bool IsJumpButtonBeingPressed()
		{
			return isJumpButtonBeingPressed;
		}

		public bool IsJumpButtonReleased()
		{
			return isJumpButtonReleased;
		}

		public bool IsJumpButtonDown()
		{
			return isJumpButtonDown;
		}

		public bool IsZoomButtonBeingPressed()
		{
			return isZoomButtonBeingPressed;
		}
		public bool IsRunButtonBeingPressed()
		{
			return isRunButtonBeingPressed;
		}

		public bool IsRunButtonDoublePressedDown()
		{
			return isRunButtonDoublePressedDown;
		}

		public bool IsProneButtonBeingPressed()
		{
			return isProneButtonBeingPressed;
		}
		public bool IsProneButtonPressedDown()
		{
			return isProneButtonPressedDown;
		}

		public bool IsRunButtonPressedDown()
		{
			return isRunButtonPressedDown;
		}

		public bool IsSlideButtonBeingPressed()
		{
			return isSlideButtonBeingPressed;
		}

		public bool IsLeanLeftButtonBeingPressed()
		{
			return isLeanLeftButtonBeingPressed;
		}

		public bool IsLeanRightButtonBeingPressed()
		{
			return isLeanRightButtonBeingPressed;
		}

		public bool IsHookButtonDown()
		{
			return isHookButtonDown;
		}

		#endregion
	}
}
