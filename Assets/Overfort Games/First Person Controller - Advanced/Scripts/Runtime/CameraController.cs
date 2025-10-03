using UnityEngine;

namespace OverfortGames.FirstPersonController
{
	public class CameraController : MonoBehaviour
	{
		#region Fields

		// References to Camera component
		[Header("References")]
		[SerializeField]
		private new Camera camera;

		[SerializeField]
		private Transform cameraRoot;

		[SerializeField]
		private Transform bodyRoot;

		[SerializeField]
		private FirstPersonController controller;

		// References to CameraInput component
		[SerializeField]
		private CameraInput cameraInput;

		[Space(5), Header("Settings")]

		[SerializeField]
		private bool stickCameraRootToColliderCeil = true;

		[SerializeField]
		private Vector3 cameraRootOffsetFromColliderCeil = new Vector3(0, -0.35f, -0.1f);

		//Upper vertical limit
		[SerializeField, Range(0f, 90f)]
		private float upperLimit = 90;

		//Lower vertical limit
		[SerializeField, Range(0f, 90f)]
		private float lowerLimit = 90f;

		//This is the turning speed
		[SerializeField]
		private float cameraSpeed = 250f;

		[Space(15)]

		public bool enableHeadbob = true;

		public HeadbobSettings headbobSettings;

		[System.Serializable]
		public class HeadbobSettings
		{
			public Transform target;

			[Tooltip("Minimum distance required for one headbob cycle. Represents the footstep distance of the character")]
			public float cycleDistance = 3;

			public float maxVerticalIntensity = 0.15f;

			public float maxHorizontalIntensity = 0.25f;

			public float intensitySpeedMultiplier = 1;

			[Header("Advanced")]

			public AnimationCurve verticalIntensityCurve;

			public AnimationCurve horizontalIntensityCurve;

			public float lerpSpeed = 5;

			public float resetLerpSpeed = 5;
		}
		
		private Vector3 headbobTargetCurrentLocalPosition;

		private float headbobCurrentFootstepDistance;


		//Current rotation in degrees;
		private float currentAngleX = 0f;
		private float currentAngleY = 0f;

		private Vector3 cameraRootDynamicOffset = Vector3.zero;
		private Vector3 cameraRootInitialLocalPosition = Vector3.zero;

		//References to transform component
		private Transform tr;
		private float defaultFov;

		private float headbobCurrentMovementSpeed;

		private Vector3 headbobCurrentOffset = Vector3.zero;
		
		private float headbobSpeedAcceleration = 5;

		#endregion

		#region Methods

		private void Awake()
		{
			if (cameraRoot == null)
				cameraRoot = transform.parent;

			//Caching transform is faster in older versions of Unity
			tr = transform;

			//Get pre-setup angles
			currentAngleX = tr.localRotation.eulerAngles.x;
			currentAngleY = tr.localRotation.eulerAngles.y;

			cameraRootInitialLocalPosition = cameraRoot.localPosition;

			defaultFov = camera.fieldOfView;

			cameraRoot.localPosition = controller.GetColliderCeilPositionLocal() + cameraRootOffsetFromColliderCeil + cameraRootDynamicOffset;

			controller.OnColliderResized += OnColliderResized;
		}

		private void LateUpdate()
		{
			if (stickCameraRootToColliderCeil == true)
			{
				Vector3 anchorPoint = controller.GetColliderCeilPositionLocal() + cameraRootOffsetFromColliderCeil + cameraRootDynamicOffset;

				if (headbobSettings.target != cameraRoot || enableHeadbob == false)
				{
				//	cameraRoot.localPosition = anchorPoint;

				}

				headbobTargetCurrentLocalPosition = anchorPoint;
			}

			HandleHeadbob();
		}

		private void OnColliderResized()
		{
			if (stickCameraRootToColliderCeil == true)
			{
				Vector3 anchorPoint = controller.GetColliderCeilPositionLocal() + cameraRootOffsetFromColliderCeil + cameraRootDynamicOffset;
				
				if (headbobSettings.target != cameraRoot)
				{
					cameraRoot.localPosition = anchorPoint;
				}

				headbobTargetCurrentLocalPosition = anchorPoint;
			}

		}

		private void HandleHeadbob()
		{
			if (enableHeadbob == false)
				return;

			if ((controller.currentControllerState != FirstPersonController.ControllerState.Standing &&
				controller.currentControllerState != FirstPersonController.ControllerState.Crouched &&
				controller.currentControllerState != FirstPersonController.ControllerState.TacticalSprint) || 
				controller.IsTryingToJumpThisFrame())
			{
				ResetHeadbob();
				return;
			}

			Vector3 vel = controller.GetVelocity();

			//Extract the horizotal velocity from velocity
			Vector3 horizontalVelocity = new Vector3(vel.x, 0, vel.z);

			float magnitude = horizontalVelocity.magnitude;

			if (magnitude > 0.01f)
			{
				DoHeadbob(horizontalVelocity.magnitude);
			}
			else
			{
				ResetHeadbob();
			}

		}

		private void ResetHeadbob()
		{
			headbobCurrentOffset = Vector3.MoveTowards(headbobCurrentOffset, Vector3.zero, headbobSettings.resetLerpSpeed * Time.deltaTime);
			headbobSettings.target.localPosition = headbobTargetCurrentLocalPosition + headbobCurrentOffset;

			headbobCurrentFootstepDistance = 0;
		}


		private void DoHeadbob(float currentMovementSpeed)
		{
			headbobCurrentMovementSpeed = Mathf.Lerp(headbobCurrentMovementSpeed, currentMovementSpeed, headbobSpeedAcceleration * Time.deltaTime);

			headbobCurrentFootstepDistance += Time.deltaTime * headbobCurrentMovementSpeed;

			if (headbobCurrentFootstepDistance > headbobSettings.cycleDistance)
				headbobCurrentFootstepDistance = 0;

			float speedPercentage = 0;

			if (headbobCurrentFootstepDistance < headbobSettings.cycleDistance / 2)
			{
				speedPercentage = headbobCurrentFootstepDistance / (headbobSettings.cycleDistance / 2);
			}
			else
			{ 
				speedPercentage = Mathf.Abs(headbobCurrentFootstepDistance - headbobSettings.cycleDistance ) / (headbobSettings.cycleDistance / 2);
			}

			float currentIntensityHorizontal = 0;

			Vector2 absInputDirection = new Vector2(Mathf.Abs(controller.GetInputDirection().x), Mathf.Abs(controller.GetInputDirection().y));
			if (absInputDirection.x > 0 || (absInputDirection.x == 0 && absInputDirection.y == 0))
			{
				currentIntensityHorizontal = 0;
			}
			else
			{
				currentIntensityHorizontal = headbobSettings.maxHorizontalIntensity * headbobSettings.horizontalIntensityCurve.Evaluate(speedPercentage);
			}

			float currentIntensityVertical = headbobSettings.maxVerticalIntensity * headbobSettings.verticalIntensityCurve.Evaluate(speedPercentage);
			Vector3 targetOffset = new Vector3(currentIntensityHorizontal * (Mathf.Pow(currentMovementSpeed / controller.horizontalSpeedSettings.defaultSpeed, headbobSettings.intensitySpeedMultiplier)), 
				currentIntensityVertical * (Mathf.Pow(currentMovementSpeed / controller.horizontalSpeedSettings.defaultSpeed, headbobSettings.intensitySpeedMultiplier)), 0);

			headbobCurrentOffset = Vector3.Lerp(headbobCurrentOffset, targetOffset, headbobSettings.lerpSpeed * Time.deltaTime);
			headbobSettings.target.localPosition = headbobTargetCurrentLocalPosition + headbobCurrentOffset;
		}

		//Get input and handle camera rotation
		public void InputHandleRotation()
		{
			//Get input
			float horizontal = cameraInput.GetHorizontal();
			float vertical = cameraInput.GetVertical();

			RotateCamera(horizontal, vertical, Time.deltaTime);
		}

		// Rotate Camera using dt as update frequency
		public void RotateCamera(float horizontalInput, float verticalInput, float dt)
		{

			//Add input
			currentAngleX += verticalInput * cameraSpeed * dt;
			currentAngleY += horizontalInput * cameraSpeed * dt;

			//Clamp rotation using limits
			currentAngleX = Mathf.Clamp(currentAngleX, -upperLimit, lowerLimit);

			UpdateRotation();
		}

		//Update camera rotation based on currentAngleX and currentAngleY
		private void UpdateRotation()
		{
			bodyRoot.localRotation = Quaternion.Euler(new Vector3(0, currentAngleY, 0));
			tr.localRotation = Quaternion.Euler(new Vector3(currentAngleX, 0, 0));
		}

		public void SetFOV(float fov)
		{
			camera.fieldOfView = fov;
		}

		public void SetDefaultFOV(float fov)
		{
			defaultFov = fov;
			SetFOV(defaultFov);
		}

		public void SetFOVLerped(float fov, float lerpSpeed, float dt)
		{
			camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, fov, lerpSpeed * dt);
		}

		public void ResetFOVLerped(float lerpSpeed, float dt)
		{
			SetFOVLerped(defaultFov, lerpSpeed, dt);
		}

		//Set rotation angles directly
		public void SetRotationAngles(float angleX, float angleY)
		{
			currentAngleX = angleX;
			currentAngleY = angleY;

			UpdateRotation();
		}

		public float GetDefaultFOV() => defaultFov;

		public float GetCurrentAngleX()
		{
			return currentAngleX;
		}

		public float GetCurrentAngleY()
		{
			return currentAngleY;
		}

		public Camera GetCamera()
		{
			return camera;
		}

		public Transform GetRoot()
		{
			return cameraRoot;
		}

		public float GetCameraSpeed() => cameraSpeed;
		public void SetCameraSpeed(float value) { cameraSpeed = value; }

		public void SetCameraRootTilt(float angle)
		{
			cameraRoot.transform.localRotation = Quaternion.Euler(cameraRoot.transform.localRotation.eulerAngles.x, cameraRoot.transform.localRotation.eulerAngles.y, angle);
		}

		public void SetCameraRootTiltLerped(float angle, float speed, float dt)
		{
			cameraRoot.transform.localRotation = Quaternion.Lerp(cameraRoot.transform.localRotation, Quaternion.Euler(cameraRoot.transform.localRotation.eulerAngles.x, cameraRoot.transform.localRotation.eulerAngles.y, angle), speed * dt);
		}

		public void AddCameraPitch(float amount)
		{
			tr.localRotation *= Quaternion.Euler(Vector3.right * amount);
		}

		public void SetCameraRootOffsetLerped(Vector3 newOffset, float speed, float dt)
		{
			cameraRootDynamicOffset = Vector3.Lerp(cameraRootDynamicOffset, newOffset, speed * dt);
			if (stickCameraRootToColliderCeil == false)
			{
				cameraRoot.localPosition = cameraRootInitialLocalPosition + cameraRootDynamicOffset;
			}
		}

		//Utilities
		public static float GetSignedAngle(Vector3 vec1, Vector3 vec2, Vector3 normal)
		{
			//Calculate angle and sign
			float angle = Vector3.Angle(vec1, vec2);
			float sign = Mathf.Sign(Vector3.Dot(normal, Vector3.Cross(vec1, vec2)));

			//Combine angle and sign
			float signedAngle = angle * sign;

			return signedAngle;
		}

		//Remove all parts from a vector that are pointing in the same direction as 'dir'
		public static Vector3 RemoveDotVector(Vector3 vec, Vector3 dir)
		{
			if (dir.sqrMagnitude != 1)
				dir.Normalize();

			float amount = Vector3.Dot(vec, dir);

			vec -= dir * amount;

			return vec;
		}

		#endregion
	}
}
