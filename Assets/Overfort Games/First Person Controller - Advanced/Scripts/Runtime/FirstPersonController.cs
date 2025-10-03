using System;
using System.Collections;
using UnityEngine;

namespace OverfortGames.FirstPersonController
{
    public class FirstPersonController : MonoBehaviour
    {
        public enum ControllerState
        {
            Standing,
            InAir,
            TacticalSprint,
            Crouched,
            Proned,
            Sliding,
            Climb,
            Grappling,
            WallRun
        }

        #region Fields

        //Events
        public event Action<float> OnLand = delegate { };
        public event Action OnJump = delegate { };
        public event Action<int> OnJumpsCountIncrease = delegate { };
        public event Action OnSlide = delegate { }; //Called each frame
        public event Action OnEndSlide = delegate { };
        public event Action OnBeginSlide = delegate { };

        public event Action OnBeginGrapplingLine = delegate { };
        public event Action OnEndGrapplingLine = delegate { };
        public event Action OnEndFailedGrapplingLine = delegate { };
        public event Action OnGrapplingLine = delegate { }; //Called each frame
        public event Action OnBeginGrappling = delegate { };
        public event Action OnEndGrappling = delegate { };
        public event Action OnGrappling = delegate { }; //Called each frame

        public event Action OnBeginWallRun = delegate { };
        public event Action OnWallRun = delegate { }; //Called each frame
        public event Action OnEndWallRun = delegate { };

        public event Action OnClimbBegin = delegate { };
        public event Action OnClimbEnd = delegate { };

        public event Action OnColliderResized = delegate { };

        [Header("References"), Space(5)]

        public CharacterInput characterInput;

        public CameraInput cameraInput;

        public CameraController cameraController;

        public CharacterController characterController;

        public Transform cameraTransform;

        //States variables
        private bool isGrounded;
        private bool isSliding;
        private float horizontal;
        private float vertical;
        private bool isJumpButtonBeingPressed;
        private bool isJumpButtonReleased;
        private bool isJumpButtonDown;
        private bool isRunButtonDoublePressedDown;
        private bool isRunning;
        private bool previousIsSliding;
        private bool previousIsGrounded;
        private bool wishToSlide;
        private bool isTryingToJump;
        private bool isProneButtonBeingPressed;
        private bool isProneButtonPressedDown;

        [Space(5), Header("Misc"), Space(5)]

        [Tooltip("Layer mask for detecting obstacle above the character")]

        public LayerMask ceilingDetectionLayerMask;

        public bool enableCollisionPush = true;

        public float collisionPushPower = 40;

        public float defaultColliderMorphSpeed = 10;


        [Tooltip("The default gravity in the physics settings will be multiplied by this value")]
        public float gravityModifier = 2;

        [Space(5), Header("Settings"), Space(5)]

        public HorizontalSpeedSettings horizontalSpeedSettings;

        [System.Serializable]
        public class HorizontalSpeedSettings
        {
            [Tooltip("Default walking speed")]
            public float defaultSpeed = 5;

            public float backwardsSpeed = 4.5f;

            [Header("Advanced")]

            [Tooltip("Speed deceleration amount when the character stops. A value 0 means no deceleration")]
            public float movementDeceleration = 0.033f;

            public float horizontalMaxSpeed = 30;

        }

        [Space(15)]

        public JumpSettings jumpSettings;

        [System.Serializable]
        public class JumpSettings
        {
            [Tooltip("Set the jump as adaptive. The more the player will press the jump button the higher the character will go")]
            public bool adaptiveJump = false;

            public int jumpsCount = 1;

            public float jumpForce = 10;

            [Header("Advanced")]
            [Tooltip("Maximum jump button press duration for the adaptive jump")]
            public float adaptiveJumpDuration = 0;

            [Tooltip("This is the time the character has for beginning a jump when not grounded")]
            public float coyoteTime = 0.15f;

            [Range(0, 1)]
            public float airControl = 0.5f;
            
            [Range(0, 100)]
            public float airMomentumControl = 0f;

            [Tooltip("When this value is higher than 0 the character will lose momentum while in the air")]
            public float airMomentumFriction = 2;

            public float verticalMaxSpeed = 50f;

            public float inAirColliderHeight = 1f;

        }

        [Space(15)]
        public bool enableRun = true;

        public RunSettings runSettings;

        [System.Serializable]
        public class RunSettings
        {
            public float runSpeed = 8;
            public bool canRunWhileStrafing = true;
        }


        [Space(15)]

        public bool enableTacticalSprint = true;

        public TacticalSprintSettings tacticalSprintSettings;

        [System.Serializable]
        public class TacticalSprintSettings
        {
            public float speed = 18;
            public float duration = 1.5f;
        }

        [Space(15)]
        public bool enableProne = true;

        public ProneSettings proneSettings;

        [System.Serializable]
        public class ProneSettings
        {
            public float speed = 1.5f;
            public float colliderHeight = 0.9f;
            public float colliderMorphSpeed = 10;
        }

        [Space(15)]
        public bool enableCrouch = true;

        public CrouchSettings crouchSettings;

        [System.Serializable]
        public class CrouchSettings
        {
            public float speed = 1.5f;
            public float colliderHeight = 1.2f;
            public float colliderMorphSpeed = 10;
        }


        [Space(15)]

        public bool enableClimb = true;
        public ClimbSettings climbSettings;

        [System.Serializable]
        public class ClimbSettings
        {
            [Tooltip("Maximum climb time duration in seconds")]
            public float maxDuration = 2;

            [Tooltip("The climb time duration is dynamic. The maximum time duration will occur when the character is this distance away from the climb end position")]
            public float durationMaxDistance = 3;

            [Tooltip("How high you can climb objects")]
            public float maxHeight = 2.5f;

            [Header("Advanced")]

            [Tooltip("The value of this curve will be multiplied to the lerp time of the climb animation. A linear curve will result in the character moving from begin point to end point at the same speed")]
            public AnimationCurve animationSpeedCurve;

            public LayerMask climbableObjectLayerMask;

            [Tooltip("Maximum distance from a climbable object in order to trigger the climb animation")]
            public float maxDistanceFromClimbableObject = 0.8f;

            public float cameraInclinationIntensity = 800;

            [Tooltip("This curve represents the speed of the camera pitch during the climb animation. The value of this curve will be multiplied to cameraInclinationSpeed")]
            public AnimationCurve cameraInclinationIntensityCurve;

        }

        [Space(15)]

        public bool enableSlide = true;
        public SlideSettings slideSettings;

        [System.Serializable]
        public class SlideSettings
        {
            [Tooltip("Initial force applied when sliding begins")]
            public float initialForce = 20;

            public float groundFriction = 15;

            public float colliderHeight = 0.9f;

            [Header("Advanced")]

            [Tooltip("Gravity-like force applied when sliding")]
            public float slideGravity = 400;

            [Tooltip("Minimum force the character has to have for sliding (Squared)")]
            public float minimumStopVelocity = 5;

            [Range(0, 1)]
            public float horizontalControl = 0.5f;

            [Tooltip("Friction applied when the camera looks in a different direction the character is moving")]
            public float cameraRotationFrictionFactor = 0.1f;

            public float colliderMorphSpeed = 10;
        }


        [Space(15)]

        public bool enableGrapplingHook = true;

        public GrapplingHookSettings grapplingHookSettings;

        [System.Serializable]
        public class GrapplingHookSettings
        {
            [Tooltip("The cooldown starts when the character launches the hook, whether it hits a target or not")]
            public float cooldown = 3f;

            [Tooltip("Speed of the character while grappled")]
            public float speedWhileHooked = 50;

            public float launchMaxDistance = 22;

            public float grapplingLaunchSpeed = 4;

            public Color crosshairColor = Color.white;

            [Header("Advanced")]

            public float horizontalControlStrength = 10f;

            [Tooltip("While grappled, the character will detach if it exceeds this speed")]
            public float detachSpeedLimitCondition = 27;

            [Tooltip("To extend this angle condition use 'detachTimerCondition'")]
            [Range(0, 90)]
            public float detachAngleCondition = 90;

            [Tooltip("This timer starts when the angle from the attach position > 'detachAngleCondition'. This can be useful in order to make the character rotate around objects while grappled")]
            public float detachTimerCondition = 0.3f;

            [Tooltip("While grappled, the character will detach if it reaches the destination point within this distance")]
            public float detachMinDistanceCondition = 3.25f;

            [Tooltip("Vertical force applied to the character when the grappling get attached to a target")]
            public float initialVerticalForce = 1;

            public LayerMask hookableObjectLayerMask;

            public GameObject hookPrefab;

            public float hookOffsetFromTarget = 0.5f;

            public Transform lineRendererStartPositionTransform;

            public Material lineRendererMaterial;

            public float lineRendererWidth = 0.05f;

            [Tooltip("Controls the quality of the grappling line")]
            public int lineRendererSegmentCount = 100;

            public float lineRendererWaveStiffness = 800;

            public float lineRendererWaveStrength = 10;

            public float lineRendererWaveCount = 3;

        }

        [Space(15)]

        public bool enableWallRun = true;

        public WallRunSettings wallRunSettings;

        [System.Serializable]
        public class WallRunSettings
        {
            public float speedWhileRunning = 12f;

            public float verticalWallJumpForce = 2f;

            public float horizontalWallJumpForce = 12.5f;

            [Header("Advanced")]
            public LayerMask walkableObjectLayerMask;

            [Tooltip("The character will automatically detach from the wall after this amount of time")]
            public float autoDetachTimerCondition = 1;

            [Tooltip("After being auto detached the character will receive this force inverse to the wall direction")]
            public float autoDetachForce = 2;

            [Tooltip("Once detached from the wall, the character cannot attach again for this amount of time (in seconds)")]
            public float cooldown = 1.5f;

            [Tooltip("Gravity while attached to the wall")]
            public float wallRunGravity = 0.5f;

            [Tooltip("Gravity while attached to the wall and moving")]
            public float wallRunGravityWhileMoving = 0.35f;

            [Tooltip("Min vertical boost force received when attaching to the wall. This vertical force depends on the amount of vertical movement the character had while hitting the wall")]
            public float attachVerticalBoostMin = 2.5f;

            [Tooltip("Max vertical boost force received when attaching to the wall. This vertical force depends on the amount of vertical movement the character had while hitting the wall")]
            public float attachVerticalBoostMax = 5;

            [Tooltip("Minimum distance from wall in order to trigger wall run")]
            public float attachMinDistanceCondition = 0.35f;

            [Tooltip("The character will detect walls on its side relative to this angle")]
            public float attachSideAngleCondition = 20;

            [Tooltip("Whether the camera tilt lerp value target changes depending on the angle relative to the character forward direction to the wall. " +
                "If this variable is NOT checked The camera tilt value target is fixed. Rotation value of 0 if the character is looking at the wall within 20 degrees, " +
                "cameraTiltAngle if the character is looking away from the wall")]
            public bool dynamicCameraTilt = true;

            public float cameraTiltAngle = 15;

            public float cameraTiltLerpSpeed = 5;

            public float cameraTiltResetLerpSpeed = 10;
        }

        [Space(15)]

        public bool enableLeaning = true;

        public LeaningSettings leaningSettings;

        [System.Serializable]
        public class LeaningSettings
        {
            [Tooltip("Head local position offset when leaning")]
            public Vector3 headOffset = new Vector3(0.4f, 0, 0);

            public float cameraTiltAngle = 20;

            public float cameraTiltSpeed = 5;

            [Header("Advanced")]

            public float headOffsetLerpSpeed = 10;

            public float headOffsetResetLerpSpeed = 10;

            public float cameraTiltResetSpeed = 10;

            public LayerMask headObstacleLayerMask;
        }

        [Space(15)]

        public bool zoomEnabled = true;

        public ZoomSettings zoomSettings;

        [System.Serializable]
        public class ZoomSettings
        {
            public float FOVWhileZooming = 36;

            public float FOVLerpSpeed = 20;

            public float FOVResetLerpSpeed = 20;
        }

        public ControllerState currentControllerState { private set; get; }
        private Transform bodyTransform;
        private float edgeFallFactor = 23;
        private Vector3 movement;
        private Vector3 previousMovement;
        private Vector3 momentum;
        private Vector3 releasedMomentum;
        private bool jumpLocked;
        private float currentJumpTimer;
        private bool isMorphingCollider;
        private float defaultColliderHeight;
        private Vector3 currentGroundNormal;
        private Vector3 velocity;
        private float lastTimeGrounded;
        private Vector3 currentGroundNormalController;
        private float colliderMaxRadius;
        private bool colliderLandingMorph = true;

        private bool isClimbingAnimation;
        private float climbDuration;
        private float climbTimer;
        private Vector3 climbStartPoint;
        private float climbStartDistanceSqr;
        private Vector3 climbEndPoint;
        private Vector3 climbEndPointRelativeToTarget;
        private Transform climbTarget;

        private Vector3 grapplingCurrentPoint;
        private float grapplingStartDistanceSqr;
        private Vector3 grapplingDirection;
        private Vector3 grapplingDirectionStart;
        private float grapplingCurrentDistance;
        private bool isGrappled;
        private LineRenderer grapplingLine;
        private GameObject grapplingLineHook;
        private float grapplingCurrentTimer;
        private float grapplingLaunchTimer = 0;
        private float grapplingCurrentDetachTimer;
        private Spring grapplingLineSpring;
        private Transform grapplingTarget;
        private Vector3? grapplingDestinationPoint;
        private Vector3 grapplingDestinationPointTargetLocalPosition;
        private Vector3[] grapplingLineSegmentsPositions;
        private float grapplingLineRendererDamper = 14;
        private float grapplingLineRendererWaveHeight = 1;


        private Transform tr;
        private float defaultGravityModifier;
        private bool callbacksEnabled;
        private int currentJumpsCount;

        private float currentTacticalSprintTimer;

        private float lastTimeGrappling;
        private float currentGroundSlope;
        private Vector3 edgeFallDirection;
        private bool raycastIsGrounded;

        private float cameraHorizontal;
        private float cameraVertical;

        private Vector3 currentWallRunDirection;
        private Vector3 currentWallRunNormal;

        private float lastTimeBeginWallRun;

        private Transform standingPlatform;

        private Vector3 currentPositionInStandingPlaform;
        private Vector3 currentLocalPositionInStandingPlaform;

        private Quaternion currentRotationInStandingPlatform;
        private Quaternion currentLocalRotationInStandingPlatform;

        private Vector3 relativeMovementOnStandingPlatform;

        private float inAirToStandingMorphSpeed = 5;

        private float landPositionY;
        private Vector3 previousVelocity;
        private float lastTimeJump;
        private float jumpEventCooldown = 0.5f;

        #endregion

        #region Methods

        private void Awake()
        {
            //Caching transform is faster in older versions of Unity
            tr = transform;
            bodyTransform = tr;

            defaultGravityModifier = gravityModifier;
            defaultColliderHeight = characterController.height;

            colliderMaxRadius = characterController.radius;
            lastTimeGrappling = -grapplingHookSettings.cooldown;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            //Set up grappling line spring
            grapplingLineSpring = new Spring();
            grapplingLineSpring.SetTarget(0);
        }

        //This method is called during CharacterController.Move()
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            //Gather last collider hit normal occured during CharacterController.Move() 
            currentGroundNormalController = hit.normal;

            if (enableCollisionPush)
            {
                //Call responsible for pushing objects
                TryPushObject(hit);
            }

            //Handle platform detection
            if (hit.moveDirection.y < -0.9 && hit.normal.y > 0.41)
            {
                if (standingPlatform != hit.collider.transform)
                {
                    standingPlatform = hit.collider.transform;
                    UpdateMovingPlatformTransform();
                }
            }
            else
            {
                standingPlatform = null;
            }

        }

        private void Update()
        {
            //Gather inputs data
            horizontal = characterInput.GetHorizontalMovementInput();
            vertical = characterInput.GetVerticalMovementInput();
            isJumpButtonBeingPressed = characterInput.IsJumpButtonBeingPressed();
            isJumpButtonReleased = characterInput.IsJumpButtonReleased();
            isJumpButtonDown = characterInput.IsJumpButtonDown();
            isRunButtonDoublePressedDown = characterInput.IsRunButtonDoublePressedDown();
            isProneButtonBeingPressed = characterInput.IsProneButtonBeingPressed();
            isProneButtonPressedDown = characterInput.IsProneButtonPressedDown();
            isRunning = characterInput.IsRunButtonBeingPressed();
            isSliding = characterInput.IsSlideButtonBeingPressed();
            cameraHorizontal = cameraInput.GetHorizontal();
            cameraVertical = cameraInput.GetVertical();

            Simulate(Time.deltaTime, true);
        }

        private void LateUpdate()
        {
            DrawGrapplingLine(Time.deltaTime);

            HandleEdgeFalling();

            HandlePlatforms();
        }

        //This is where the magic happens
        public void Simulate(float dt, bool callbacksEnabled)
        {
            //Enable events calling like OnSlide, OnJump etc...
            this.callbacksEnabled = callbacksEnabled;

            //Gather current ground normal
            //This will be useful in future calculations
            Vector3 rayOrigin = GetTransformOrigin();
            Ray ray = new Ray(rayOrigin, Vector3.down);
            if (Physics.Raycast(ray, out var hit, 100))
            {
                currentGroundNormal = hit.normal;
            }

            isGrounded = CheckForGround();

            currentControllerState = DetermineControllerState();

            HandleClimb(dt);

            HandleGrappling(dt);

            HandleWallRun(dt);

            HandleTacticalSprint(dt);

            HandleLeaning(dt);

            HandleZoom(dt);

            HandleMovementVelocity();

            HandleJump(dt);

            HandleGravityModifier();

            AddGravity(dt);

            HandleMomentum(dt);

            ApplyMovement(dt);

            //Save last used states 
            previousMovement = movement;
            previousIsSliding = isSliding;
            previousIsGrounded = isGrounded;
            previousVelocity = velocity;

            velocity = characterController.velocity;

            //Handle camera rotation - Camera is locked during climb
            if (currentControllerState != ControllerState.Climb)
                cameraController.RotateCamera(cameraHorizontal, cameraVertical, dt);
        }

        public bool CheckForGround()
        {
            //--------------------------------------------------------------------------------------------------
            //The default 'CharacterController.isGrounded' check happens at the whole base of the collider.
            //Through a raycast we make sure that specifically the center of the collider is also grounded 
            //--------------------------------------------------------------------------------------------------

            //Gather the slope values from the 'CharacterController' collider hit and from a raycast 
            currentGroundSlope = CalculateSlope(currentGroundNormal);
            float controllerGroundNormalSlope = CalculateSlope(currentGroundNormalController);

            //Select the maximum value from the two for a worst case scenario
            float slope = controllerGroundNormalSlope > currentGroundSlope ? controllerGroundNormalSlope : currentGroundSlope;

            //The raycast distance depends on the current slope value of the ground
            float rayMaxDistance = Mathf.Max(slope / characterController.slopeLimit, characterController.stepOffset + 0.01f);

            raycastIsGrounded = Physics.Raycast(new Ray(GetTransformOrigin(), Vector3.down), rayMaxDistance);
            bool controllerIsGrounded = characterController.isGrounded;

            return raycastIsGrounded && controllerIsGrounded;
        }

        public ControllerState DetermineControllerState()
        {
            switch (currentControllerState)
            {
                case ControllerState.Standing:

                    //Standing ---> Grappling
                    if (isGrappled == true)
                    {
                        AddMomentum(tr.up * grapplingHookSettings.initialVerticalForce);
                        return ControllerState.Grappling;
                    }

                    //Standing ---> InAir
                    if (isGrounded == false)
                    {
                        return ControllerState.InAir;
                    }

                    //Standing ---> Sliding
                    if (enableSlide && enableRun && isRunning && IsSlidingButtonPressedDown() && isMorphingCollider == false && vertical > 0)
                    {
                        //Morph the collider to slide height
                        SetColliderHeightAutoLerp(slideSettings.colliderHeight, slideSettings.colliderMorphSpeed);

                        //Preset the momentum
                        SetMomentum(new Vector3(GetVelocity().x, 0, GetVelocity().z) + new Vector3(GetVelocity().x, 0, GetVelocity().z).normalized * slideSettings.initialForce);

                        if (callbacksEnabled)
                            OnBeginSlide();

                        return ControllerState.Sliding;
                    }

                    //Standing ---> Crouched
                    if (enableCrouch && IsSlidingButtonPressedDown() && isMorphingCollider == false)
                    {
                        //We want to morph the collider to crouched
                        SetColliderHeightAutoLerp(crouchSettings.colliderHeight, crouchSettings.colliderMorphSpeed);

                        return ControllerState.Crouched;
                    }

                    //Standing ---> Tactical Sprint
                    if (isRunButtonDoublePressedDown && enableTacticalSprint)
                    {
                        return ControllerState.TacticalSprint;
                    }

                    //Standing ---> Proned
                    if (isProneButtonPressedDown && enableProne && isMorphingCollider == false)
                    {
                        SetColliderHeightAutoLerp(proneSettings.colliderHeight, proneSettings.colliderMorphSpeed);
                        return ControllerState.Proned;
                    }

                    return ControllerState.Standing;

                case ControllerState.InAir:

                    //Morph or reset the collider height while landing
                    if (movement.y < 0 && isMorphingCollider == false)
                    {
                        if (colliderLandingMorph)
                        {
                            SetColliderHeightAutoLerp(defaultColliderHeight, inAirToStandingMorphSpeed, true);
                        }
                        else
                        {
                            StopColliderHeightAutoLerp();
                            ResizeCollider(defaultColliderHeight);
                        }
                    }

                    //InAir ---> Climb
                    if (enableClimb && CheckClimb())
                    {
                        return ControllerState.Climb;
                    }

                    //InAir ---> Grappling
                    if (isGrappled == true)
                    {
                        SetColliderHeightAutoLerp(defaultColliderHeight, defaultColliderMorphSpeed);
                        return ControllerState.Grappling;
                    }

                    //We want to go from InAir to Sliding directly
                    if (enableSlide && IsSlidingButtonPressedDown())
                        wishToSlide = true;

                    //InAir ---> Sliding (go directly to Sliding when falling if the sliding button was pressed)
                    if (enableSlide && isGrounded && wishToSlide && isMorphingCollider == false)
                    {
                        //Preset momentum
                        SetMomentum(new Vector3(GetVelocity().x, 0, GetVelocity().z).normalized * runSettings.runSpeed);

                        wishToSlide = false;

                        //Morph the collider to slide height
                        SetColliderHeightAutoLerp(slideSettings.colliderHeight, slideSettings.colliderMorphSpeed);

                        if (callbacksEnabled)
                            OnBeginSlide();

                        return ControllerState.Sliding;
                    }

                    //InAir ---> Standing
                    if (isGrounded && wishToSlide == false)
                    {
                        //There is enough space for the character to be standing
                        if (IsColliderSpaceFree(defaultColliderHeight))
                        {
                            SetColliderHeightAutoLerp(defaultColliderHeight, defaultColliderMorphSpeed);
                            return ControllerState.Standing;
                        }

                        //There is enough space for the character to be crouched
                        if (IsColliderSpaceFree(crouchSettings.colliderHeight) && enableCrouch)
                        {
                            SetColliderHeightAutoLerp(crouchSettings.colliderHeight, crouchSettings.colliderMorphSpeed);
                            return ControllerState.Crouched;
                        }

                        //Minimum space limit supported state
                        if (enableProne)
                        {
                            SetColliderHeightAutoLerp(proneSettings.colliderHeight, proneSettings.colliderMorphSpeed);
                            return ControllerState.Proned;
                        }

                    }

                    //InAir ---> WallRun
                    if (enableWallRun)
                    {
                        Ray inAirWallRunRayCheckCenter = new Ray(GetColliderCenterPosition(), InputToMovementDirection());
                        Ray inAirWallRunRayCheckRight = new Ray(GetColliderCenterPosition(), Quaternion.AngleAxis(wallRunSettings.attachSideAngleCondition, bodyTransform.up)
                            * inAirWallRunRayCheckCenter.direction);
                        Ray inAirWallRunRayCheckLeft = new Ray(GetColliderCenterPosition(), Quaternion.AngleAxis(-wallRunSettings.attachSideAngleCondition, bodyTransform.up)
                            * inAirWallRunRayCheckCenter.direction);

                        float radius = 0.25f;

                        //We are looking towards a wall and the wall climb cooldown is expired
                        if ((Physics.SphereCast(inAirWallRunRayCheckCenter, radius, out var inAirWallRunHit, wallRunSettings.attachMinDistanceCondition, wallRunSettings.walkableObjectLayerMask) ||
                            Physics.SphereCast(inAirWallRunRayCheckLeft, radius, out inAirWallRunHit, wallRunSettings.attachMinDistanceCondition, wallRunSettings.walkableObjectLayerMask) ||
                            Physics.SphereCast(inAirWallRunRayCheckRight, radius, out inAirWallRunHit, wallRunSettings.attachMinDistanceCondition, wallRunSettings.walkableObjectLayerMask)
                            )
                            && Time.time > lastTimeBeginWallRun + wallRunSettings.cooldown)
                        {
                            float verticalForce = 0;

                            //Give vertical force relative to the movement we had when we attached to the wall
                            if (movement.y > 0)
                            {
                                verticalForce = wallRunSettings.attachVerticalBoostMax * movement.y / (jumpSettings.jumpForce * wallRunSettings.wallRunGravity / defaultGravityModifier / 2);
                            }

                            verticalForce = Mathf.Clamp(verticalForce, wallRunSettings.attachVerticalBoostMin, wallRunSettings.attachVerticalBoostMax);
                            movement.y = verticalForce;

                            lastTimeBeginWallRun = Time.time;

                            if (callbacksEnabled)
                            {
                                OnBeginWallRun();
                            }

                            return ControllerState.WallRun;
                        }
                    }

                    return ControllerState.InAir;

                case ControllerState.TacticalSprint:

                    //TacticalSprint ---> Grappling
                    if (isGrappled == true)
                    {
                        AddMomentum(tr.up * grapplingHookSettings.initialVerticalForce);
                        return ControllerState.Grappling;
                    }

                    //TacticalSprint ---> InAir
                    if (isGrounded == false)
                    {
                        currentTacticalSprintTimer = 0;
                        return ControllerState.InAir;
                    }

                    //TacticalSprint ---> Standing (duration over)
                    if (IsTacticalSprintDurationOver())
                    {
                        currentTacticalSprintTimer = 0;
                        return ControllerState.Standing;
                    }

                    //TacticalSprint ---> Sliding
                    if (enableSlide && IsSlidingButtonPressedDown() && isMorphingCollider == false && vertical > 0)
                    {
                        currentTacticalSprintTimer = 0;

                        //Morph the collider to slide height
                        SetColliderHeightAutoLerp(slideSettings.colliderHeight, slideSettings.colliderMorphSpeed);

                        //Preset the momentum
                        SetMomentum(new Vector3(GetVelocity().x, 0, GetVelocity().z) + new Vector3(GetVelocity().x, 0, GetVelocity().z).normalized * slideSettings.initialForce);

                        if (callbacksEnabled)
                            OnBeginSlide();

                        return ControllerState.Sliding;
                    }

                    //TacticalSprint ---> Crouched
                    if (enableCrouch && IsSlidingButtonPressedDown() && isMorphingCollider == false)
                    {
                        currentTacticalSprintTimer = 0;

                        //Morph the collider to crouch height
                        SetColliderHeightAutoLerp(crouchSettings.colliderHeight, crouchSettings.colliderMorphSpeed);

                        return ControllerState.Crouched;
                    }

                    //TacticalSprint ---> Proned
                    if (isProneButtonPressedDown && enableProne && isMorphingCollider == false)
                    {
                        currentTacticalSprintTimer = 0;

                        //Morph the collider to prone height
                        SetColliderHeightAutoLerp(proneSettings.colliderHeight, proneSettings.colliderMorphSpeed);
                        return ControllerState.Proned;
                    }

                    return ControllerState.TacticalSprint;

                case ControllerState.Crouched:

                    //Crouched ---> Grappling
                    if (isGrappled == true)
                    {
                        AddMomentum(tr.up * grapplingHookSettings.initialVerticalForce);

                        //Morph the collider to standing height
                        SetColliderHeightAutoLerp(defaultColliderHeight, defaultColliderMorphSpeed);

                        return ControllerState.Grappling;
                    }

                    //Crouched ---> InAir
                    if (isGrounded == false && isMorphingCollider == false)
                    {
                        return ControllerState.InAir;
                    }

                    //Crouched ---> Standing
                    if (isGrounded == true && IsSlidingButtonPressedDown() && isMorphingCollider == false && IsColliderSpaceFree(defaultColliderHeight))
                    {
                        //Morph the collider to standing height
                        SetColliderHeightAutoLerp(defaultColliderHeight, defaultColliderMorphSpeed);

                        return ControllerState.Standing;
                    }

                    //Crouched ---> Standing (running)
                    if (isGrounded == true && enableRun && isRunning && isMorphingCollider == false && IsColliderSpaceFree(defaultColliderHeight))
                    {
                        //Morph the collider to standing height
                        SetColliderHeightAutoLerp(defaultColliderHeight, defaultColliderMorphSpeed);

                        return ControllerState.Standing;
                    }

                    //Crouched ---> Proned
                    if (isProneButtonPressedDown && enableProne && isMorphingCollider == false)
                    {
                        //Morph the collider to prone height
                        SetColliderHeightAutoLerp(proneSettings.colliderHeight, proneSettings.colliderMorphSpeed);

                        return ControllerState.Proned;
                    }

                    return ControllerState.Crouched;

                case ControllerState.Proned:

                    //Proned ---> Grappling
                    if (isGrappled == true)
                    {
                        AddMomentum(tr.up * grapplingHookSettings.initialVerticalForce);

                        //Morph the collider to standing height
                        SetColliderHeightAutoLerp(defaultColliderHeight, defaultColliderMorphSpeed);

                        return ControllerState.Grappling;
                    }

                    //Proned ---> InAir
                    if (isGrounded == false)
                    {
                        return ControllerState.InAir;
                    }

                    //Proned ---> Standing (jumping or pressing the prone button)
                    if ((isProneButtonPressedDown || isJumpButtonDown) && isMorphingCollider == false && IsColliderSpaceFree(defaultColliderHeight))
                    {
                        SetColliderHeightAutoLerp(defaultColliderHeight, defaultColliderMorphSpeed);
                        return ControllerState.Standing;
                    }

                    //Proned ---> Crouched
                    if (enableCrouch && IsSlidingButtonPressedDown() && isMorphingCollider == false && IsColliderSpaceFree(crouchSettings.colliderHeight))
                    {
                        //Morph the collider to crouch height
                        SetColliderHeightAutoLerp(crouchSettings.colliderHeight, crouchSettings.colliderMorphSpeed);

                        return ControllerState.Crouched;
                    }

                    return ControllerState.Proned;

                case ControllerState.Sliding:

                    //Sliding ---> Grappling
                    if (isGrappled == true)
                    {
                        if (callbacksEnabled)
                            OnEndSlide();

                        AddMomentum(tr.up * grapplingHookSettings.initialVerticalForce);

                        //Morph the collider to standing height
                        SetColliderHeightAutoLerp(defaultColliderHeight, defaultColliderMorphSpeed);

                        return ControllerState.Grappling;
                    }

                    //Sliding ---> Standing (minimum stop velocity)
                    if (GetVelocity().sqrMagnitude < slideSettings.minimumStopVelocity * slideSettings.minimumStopVelocity && isMorphingCollider == false)
                    {
                        //Reset the momentum
                        SetMomentum(Vector3.zero);

                        if (callbacksEnabled)
                            OnEndSlide();

                        //There is enough space for the character to be standing
                        if (IsColliderSpaceFree(defaultColliderHeight))
                        {
                            SetColliderHeightAutoLerp(defaultColliderHeight, defaultColliderMorphSpeed);

                            return ControllerState.Standing;
                        }

                        //There is enough space for the character to be crouched
                        if (IsColliderSpaceFree(crouchSettings.colliderHeight) && enableCrouch)
                        {
                            SetColliderHeightAutoLerp(crouchSettings.colliderHeight, crouchSettings.colliderMorphSpeed);

                            return ControllerState.Crouched;
                        }

                        //Minimum space limit supported state
                        if (enableProne)
                        {
                            SetColliderHeightAutoLerp(proneSettings.colliderHeight, proneSettings.colliderMorphSpeed);

                            return ControllerState.Proned;
                        }
                    }

                    //Sliding ---> InAir
                    if (IsSlidingButtonPressedDown() && isMorphingCollider == false && isGrounded == false)
                    {
                        if (callbacksEnabled)
                            OnEndSlide();

                        return ControllerState.InAir;
                    }

                    //Sliding ---> Standing (stop sliding by re-pressing the slide button)
                    if (IsSlidingButtonPressedDown() && isMorphingCollider == false && IsColliderSpaceFree(defaultColliderHeight))
                    {
                        //Reset momentum
                        SetMomentum(Vector3.zero);

                        if (callbacksEnabled)
                            OnEndSlide();

                        //Morph the collider to standing height
                        SetColliderHeightAutoLerp(defaultColliderHeight, defaultColliderMorphSpeed);

                        return ControllerState.Standing;
                    }

                    //OnSlide per update callback
                    if (callbacksEnabled && isGrounded)
                        OnSlide();

                    //Sliding ---> InAir
                    if (isJumpButtonDown && IsColliderSpaceFree(defaultColliderHeight))
                    {
                        if (callbacksEnabled)
                            OnEndSlide();

                        return ControllerState.InAir;
                    }

                    return ControllerState.Sliding;

                case ControllerState.Climb:

                    //Climb ---> Standing (climb animation has ended)
                    if (isClimbingAnimation == false)
                    {
                        if (colliderLandingMorph)
                        {

                            //There is enough space for the character to be standing
                            if (IsColliderSpaceFree(defaultColliderHeight))
                            {
                                SetColliderHeightAutoLerp(defaultColliderHeight, defaultColliderMorphSpeed);
                                return ControllerState.Standing;
                            }

                            //There is enough space for the character to be crouched
                            if (IsColliderSpaceFree(crouchSettings.colliderHeight) && enableCrouch)
                            {
                                SetColliderHeightAutoLerp(crouchSettings.colliderHeight, crouchSettings.colliderMorphSpeed);
                                return ControllerState.Crouched;
                            }

                            //Minimum space limit supported state
                            if (enableProne)
                            {
                                SetColliderHeightAutoLerp(proneSettings.colliderHeight, proneSettings.colliderMorphSpeed);
                                return ControllerState.Proned;
                            }

                        }
                        else
                        {
                            ResizeCollider(defaultColliderHeight);
                        }

                        return ControllerState.Standing;
                    }

                    return ControllerState.Climb;

                case ControllerState.Grappling:

                    //Set collider height to inAirHeight while grappling
                    if (characterController.height > jumpSettings.inAirColliderHeight)
                    {
                        StopColliderHeightAutoLerp();
                        ResizeCollider(jumpSettings.inAirColliderHeight);
                    }

                    if (isGrappled == false)
                    {
                        //Grappling ---> InAir
                        if (isGrounded == false)
                        {
                            movement.y = 0;
                            SetMomentum(GetVelocity());
                            if (colliderLandingMorph)
                            {
                                SetColliderHeightAutoLerp(defaultColliderHeight, inAirToStandingMorphSpeed, true);
                            }
                            else
                            {
                                StopColliderHeightAutoLerp();
                                ResizeCollider(defaultColliderHeight);
                            }
                            return ControllerState.InAir;
                        }
                        else //Grappling ---> Standing
                        {
                            if (colliderLandingMorph)
                            {
                                SetColliderHeightAutoLerp(defaultColliderHeight, inAirToStandingMorphSpeed, true);
                            }
                            else
                            {
                                StopColliderHeightAutoLerp();
                                ResizeCollider(defaultColliderHeight);
                            }
                            return ControllerState.Standing;
                        }
                    }

                    return ControllerState.Grappling;

                case ControllerState.WallRun:

                    //WallRun ---> Climb
                    if (enableClimb && CheckClimb())
                    {
                        return ControllerState.Climb;
                    }

                    //WallRun ---> InAir
                    if (Time.time > lastTimeBeginWallRun + wallRunSettings.autoDetachTimerCondition)
                    {
                        SetMomentum(-currentWallRunDirection * wallRunSettings.autoDetachForce);
                        currentWallRunDirection = Vector3.zero;

                        if (callbacksEnabled)
                        {
                            OnEndWallRun();
                        }
                        return ControllerState.InAir;
                    }

                    //Wallrun ---> Grappling
                    if (isGrappled)
                    {
                        if (callbacksEnabled)
                        {
                            OnEndWallRun();
                        }
                        return ControllerState.Grappling;
                    }

                    //WallRun ---> Standing
                    if (isGrounded == true)
                    {
                        currentWallRunDirection = Vector3.zero;

                        if (callbacksEnabled)
                        {
                            OnEndWallRun();
                        }

                        SetColliderHeightAutoLerp(defaultColliderHeight, defaultColliderMorphSpeed);
                        return ControllerState.Standing;
                    }

                    //Wallrun ---> Standing (no walls hit)
                    Vector3 previousWallRunNormal = currentWallRunNormal;
                    Vector3 previousWallRunDirection = currentWallRunDirection;
                    if (CheckWallRunRaycast(bodyTransform.right, out var wallRunRay, out var hitWall) ||
                        CheckWallRunRaycast(-bodyTransform.right, out wallRunRay, out hitWall) ||
                        CheckWallRunRaycast(bodyTransform.forward, out wallRunRay, out hitWall) ||
                        CheckWallRunRaycast(-bodyTransform.forward, out wallRunRay, out hitWall))
                    {

                        //Gather wall direction and normal
                        currentWallRunDirection = wallRunRay.direction;
                        currentWallRunNormal = hitWall.normal;

                    }
                    else //No walls was hit
                    {
                        currentWallRunDirection = Vector3.zero;

                        if (callbacksEnabled)
                        {
                            OnEndWallRun();
                        }

                        //Morph collider to standing height
                        SetColliderHeightAutoLerp(defaultColliderHeight, defaultColliderMorphSpeed);
                        return ControllerState.Standing;
                    }


                    //WallRun ---> Standing (going around a corner)
                    float angleFromPreviousFrame = Vector3.Angle(previousWallRunNormal, currentWallRunNormal);
                    if (previousWallRunDirection != Vector3.zero && angleFromPreviousFrame > 20)
                    {
                        currentWallRunDirection = Vector3.zero;

                        if (callbacksEnabled)
                        {
                            OnEndWallRun();
                        }

                        SetColliderHeightAutoLerp(defaultColliderHeight, defaultColliderMorphSpeed);
                        return ControllerState.Standing;
                    }

                    //WallRun ---> Standing (looking at opposite direction from wall)
                    if (previousWallRunDirection != Vector3.zero && Vector3.Dot(currentWallRunDirection, InputToMovementDirection()) < -0.6)
                    {
                        currentWallRunDirection = Vector3.zero;

                        if (callbacksEnabled)
                        {
                            OnEndWallRun();
                        }

                        SetColliderHeightAutoLerp(defaultColliderHeight, defaultColliderMorphSpeed);
                        return ControllerState.Standing;
                    }

                    //Wallrun ---> InAir (wall run jump)
                    if (isJumpButtonDown)
                    {
                        SetMomentum(Vector3.ClampMagnitude(-currentWallRunDirection * wallRunSettings.horizontalWallJumpForce + Vector3.up * wallRunSettings.verticalWallJumpForce + velocity, wallRunSettings.speedWhileRunning));
                        lastTimeBeginWallRun = Time.time - wallRunSettings.cooldown + 0.25f;
                        currentWallRunDirection = Vector3.zero;

                        if (callbacksEnabled)
                        {
                            OnEndWallRun();
                        }
                        return ControllerState.InAir;
                    }

                    //Keep in mind this callback gets called each determination of controller state (each frame)
                    if (callbacksEnabled)
                    {
                        OnWallRun();
                    }

                    return ControllerState.WallRun;

                default:
                    return ControllerState.Standing;
            }
        }

        private void HandleClimb(float dt)
        {
            //We are not climbing
            if (enableClimb == false || currentControllerState != ControllerState.Climb)
                return;

            //Begin climb animation
            if (isClimbingAnimation == false)
            {
                //Reset any movement
                SetMomentum(Vector3.zero);
                movement = Vector3.zero;

                isClimbingAnimation = true;

                OnClimbBegin();
            }
            else
            {
                //Climbing animation in progress
                if (climbTimer < 1)
                {
                    //Pitch the camera. The rotation amount is determined by the cameraInclinationSpeedCurve 
                    //Example: time 0 value 1 | time 1 value 0 , means the camera will rotate up and then down for the same amount
                    float cameraPitchAmount = climbSettings.cameraInclinationIntensity * climbSettings.cameraInclinationIntensityCurve.Evaluate(climbTimer)
                        * (climbStartDistanceSqr / (climbSettings.durationMaxDistance * climbSettings.durationMaxDistance)) * dt;

                    cameraController.AddCameraPitch(cameraPitchAmount);
                    climbTimer += dt / climbDuration;

                    //Get custom climbing curve value
                    float lerpValue = climbSettings.animationSpeedCurve.Evaluate(climbTimer);

                    climbEndPoint = climbTarget.TransformPoint(climbEndPointRelativeToTarget);

                    //Move the character from the start point to the end point
                    Teleport(Vector3.Lerp(climbStartPoint, climbEndPoint, Mathf.Min(lerpValue, 1)));
                }
                else //Climbing animation has ended
                {
                    //Reset
                    climbTimer = 0;
                    isClimbingAnimation = false;
                    OnClimbEnd();
                }
            }
        }

        private void HandleGrappling(float dt)
        {
            //Grappling is not enabled
            if (enableGrapplingHook == false)
                return;

            //Handle launching the line
            if (IsGrapplingOnCooldown() == false)
            {
                //Get ray from camera direction
                Ray ray = cameraController.GetCamera().ViewportPointToRay(new Vector3(0.5f, 0.5f));

                //The character is in range of hitting something
                if (Physics.Raycast(ray, out var hit, grapplingHookSettings.launchMaxDistance, grapplingHookSettings.hookableObjectLayerMask))
                {
                    Crosshair.SetCrosshairColor(grapplingHookSettings.crosshairColor);

                    //The player has hit the hook button
                    if (characterInput.IsHookButtonDown())
                    {
                        GrapplingLineBegin(hit.point);
                        grapplingTarget = hit.collider.transform;

                        //Set the relative position from the target hit by the hook (in case the target is a moving platform)
                        grapplingDestinationPointTargetLocalPosition = grapplingTarget.InverseTransformPoint(hit.point);
                    }
                }
                else //The character is not in range of hitting something. The rope will be launched anyway but the character will not move
                {
                    Crosshair.SetCrosshairToDefaultColor();

                    //The player has hit the hook button
                    if (characterInput.IsHookButtonDown())
                    {
                        GrapplingLineBegin(ray.origin + ray.direction * grapplingHookSettings.launchMaxDistance);
                        grapplingTarget = null;
                    }
                }

            }
            else //The grappling is on cooldown
            {
                Crosshair.SetCrosshairToDefaultColor();
            }

            //Handle line movement
            if (grapplingDestinationPoint != null)
            {
                //If we have a target update the destination point for the line (in case of a moving target)
                if (grapplingTarget != null)
                {
                    grapplingDestinationPoint = grapplingTarget.TransformPoint(grapplingDestinationPointTargetLocalPosition);
                }

                OnGrapplingLine();

                //The speed of the line is proportional to the distance from the destination point
                grapplingLaunchTimer += dt * grapplingHookSettings.grapplingLaunchSpeed * (grapplingHookSettings.launchMaxDistance * grapplingHookSettings.launchMaxDistance / grapplingStartDistanceSqr);
                grapplingCurrentPoint = Vector3.Lerp(GetGrapplingLineStartPosition(), grapplingDestinationPoint.Value, grapplingLaunchTimer);

                //The line has reached the destination point
                if ((grapplingCurrentPoint - grapplingDestinationPoint.Value).sqrMagnitude < 0.1f)
                {
                    grapplingDirectionStart = (grapplingCurrentPoint - GetTransformOrigin()).normalized;

                    //If we have a target start the grappling movement of the character, 
                    //the character will be set to Grappling state in the next DetermineControllerState() call
                    if (grapplingTarget != null)
                    {
                        OnEndGrapplingLine();
                        OnBeginGrappling();
                        isGrappled = true;
                    }
                    else //We didn't have a target
                    {
                        OnEndFailedGrapplingLine();
                    }

                    grapplingDestinationPoint = null;
                    grapplingLaunchTimer = 0;
                }
            }

            //Handle grappled character movement

            if (isGrappled == false) //We are not grappled yet
            {
                grapplingCurrentDetachTimer = 0;
                grapplingCurrentTimer = 0;
                return;
            }

            //Update the destination to follow the target (in case of a moving platform)
            if (grapplingTarget != null)
            {
                grapplingCurrentPoint = grapplingTarget.TransformPoint(grapplingDestinationPointTargetLocalPosition);
            }

            grapplingDirection = (grapplingCurrentPoint - GetTransformOrigin()).normalized;
            grapplingCurrentDistance = Vector3.Distance(grapplingCurrentPoint, tr.position);
            grapplingCurrentTimer += dt;

            OnGrappling();

            //If the current distance is below the threshold or the speed of the character exceed the limit, detach the character 
            if (grapplingCurrentDistance <= grapplingHookSettings.detachMinDistanceCondition
                || GetCurrentSpeedSqr() > grapplingHookSettings.detachSpeedLimitCondition * grapplingHookSettings.detachSpeedLimitCondition
                || momentum.sqrMagnitude > grapplingHookSettings.detachSpeedLimitCondition * grapplingHookSettings.detachSpeedLimitCondition)
            {
                OnEndGrappling();
                isGrappled = false;
                grapplingDestinationPoint = null;
            }


            //If the character is spinning around an object (the angle from the attach position > 90) and the timer exceed the time limit, detach the character
            if (Vector3.Angle(grapplingDirection, grapplingDirectionStart) > grapplingHookSettings.detachAngleCondition)
            {
                grapplingCurrentDetachTimer += dt;
                if (grapplingCurrentDetachTimer > grapplingHookSettings.detachTimerCondition)
                {
                    OnEndGrappling();
                    isGrappled = false;
                    grapplingDestinationPoint = null;
                }
            }
        }

        private void HandleWallRun(float dt)
        {
            //Wall run is not enabled
            if (enableWallRun == false)
                return;

            //Reset cooldown when grounded
            if (isGrounded)
            {
                lastTimeBeginWallRun = 0;
            }

            //Handle camera tilt
            //The camera will tilt depending on the angle the character is looking at the wall
            if (currentControllerState == ControllerState.WallRun && Vector3.Dot(currentWallRunNormal, bodyTransform.forward) < 0.5f)
            {
                float angle = Vector3.SignedAngle(bodyTransform.forward, currentWallRunDirection, bodyTransform.up);

                //The camera tilt value target changes depending on the angle 
                if (wallRunSettings.dynamicCameraTilt)
                {
                    cameraController.SetCameraRootTiltLerped(angle / 90 * wallRunSettings.cameraTiltAngle, wallRunSettings.cameraTiltLerpSpeed, dt);
                }
                else //The camera tilt value target is fixed. 0 if the character is looking at the wall within 20 degrees, cameraTiltAngle if the character is looking away from the wall
                {
                    float unsignedAngle = Mathf.Abs(angle);
                    if (unsignedAngle > 20)
                    {
                        cameraController.SetCameraRootTiltLerped(Mathf.Sign(angle) * wallRunSettings.cameraTiltAngle, wallRunSettings.cameraTiltLerpSpeed, dt);
                    }
                    else
                    {
                        cameraController.SetCameraRootTiltLerped(0, wallRunSettings.cameraTiltResetLerpSpeed, dt);
                    }
                }
            }
            else
            {
                cameraController.SetCameraRootTiltLerped(0, wallRunSettings.cameraTiltResetLerpSpeed, dt);
            }
        }

        private void HandleTacticalSprint(float dt)
        {
            if (currentControllerState != ControllerState.TacticalSprint)
                return;

            //Increase the timer value used to determine whether the tactical sprint should end
            currentTacticalSprintTimer += dt;
        }

        private void HandleLeaning(float dt)
        {
            //Can't lean while wall running
            if (enableLeaning == false || currentControllerState == ControllerState.WallRun)
            {
                cameraController.SetCameraRootOffsetLerped(Vector3.zero, leaningSettings.headOffsetResetLerpSpeed, dt);
                return;
            }

            //Left lean
            if (characterInput.IsLeanLeftButtonBeingPressed())
            {
                //Check if there is free space for the head
                if (Physics.CheckSphere(GetColliderCeilPosition() - bodyTransform.right * leaningSettings.headOffset.x +
                        bodyTransform.up * leaningSettings.headOffset.y + bodyTransform.forward * leaningSettings.headOffset.z, 0.35f, leaningSettings.headObstacleLayerMask) == false)
                {
                    cameraController.SetCameraRootTiltLerped(leaningSettings.cameraTiltAngle, leaningSettings.cameraTiltSpeed, dt);
                    cameraController.SetCameraRootOffsetLerped(new Vector3(-leaningSettings.headOffset.x, leaningSettings.headOffset.y, leaningSettings.headOffset.z), leaningSettings.headOffsetLerpSpeed, dt);
                }
                else //No free space, reset lean
                {
                    cameraController.SetCameraRootTiltLerped(0, leaningSettings.cameraTiltResetSpeed, dt);
                    cameraController.SetCameraRootOffsetLerped(Vector3.zero, leaningSettings.headOffsetResetLerpSpeed, dt);
                }
            }
            //Right lean
            else if (characterInput.IsLeanRightButtonBeingPressed())
            {
                //Check if there is free space for the head
                if (Physics.CheckSphere(GetColliderCeilPosition() + bodyTransform.right * leaningSettings.headOffset.x +
                        bodyTransform.up * leaningSettings.headOffset.y + bodyTransform.forward * leaningSettings.headOffset.z, 0.35f, leaningSettings.headObstacleLayerMask) == false)
                {
                    cameraController.SetCameraRootTiltLerped(-leaningSettings.cameraTiltAngle, leaningSettings.cameraTiltSpeed, dt);
                    cameraController.SetCameraRootOffsetLerped(leaningSettings.headOffset, leaningSettings.headOffsetLerpSpeed, dt);
                }
                else //No free space, reset lean
                {
                    cameraController.SetCameraRootTiltLerped(0, leaningSettings.cameraTiltResetSpeed, dt);
                    cameraController.SetCameraRootOffsetLerped(Vector3.zero, leaningSettings.headOffsetResetLerpSpeed, dt);
                }
            }
            else //No buttons were pressed, reset lean
            {
                cameraController.SetCameraRootTiltLerped(0, leaningSettings.cameraTiltResetSpeed, dt);
                cameraController.SetCameraRootOffsetLerped(Vector3.zero, leaningSettings.headOffsetResetLerpSpeed, dt);
            }

        }

        private void HandleZoom(float dt)
        {
            //Zoom is not enabled
            if (zoomEnabled == false)
            {
                cameraController.ResetFOVLerped(zoomSettings.FOVResetLerpSpeed, dt);
                return;
            }

            if (characterInput.IsZoomButtonBeingPressed())
            {
                cameraController.SetFOVLerped(cameraController.GetDefaultFOV() - Camera.HorizontalToVerticalFieldOfView(zoomSettings.FOVWhileZooming, cameraController.GetCamera().aspect), zoomSettings.FOVLerpSpeed, dt);
            }
            else
                cameraController.ResetFOVLerped(zoomSettings.FOVResetLerpSpeed, dt);
        }

        private void HandleMovementVelocity()
        {
            Vector3 direction = InputToMovementDirection();

            float speedMultiplier;

            if (currentControllerState == ControllerState.Proned)
            {
                //Set 'speedMultiplier' to prone speed
                speedMultiplier = proneSettings.speed;

                movement = new Vector3(direction.x * speedMultiplier, this.movement.y, direction.z * speedMultiplier);
            }

            if (currentControllerState == ControllerState.TacticalSprint)
            {
                //Set 'speedMultiplier' to tactical speed or normal speed or backpedal speed
                if (vertical < 0)
                    speedMultiplier = horizontalSpeedSettings.backwardsSpeed;
                else
                {
                    //We are strafing
                    if (horizontal > 0.05f || horizontal < -0.05f)
                    {
                        //We are strafing and pressing forward input
                        if (runSettings.canRunWhileStrafing && vertical > 0)
                        {
                            speedMultiplier = tacticalSprintSettings.speed;
                        }
                        else
                        {
                            speedMultiplier = horizontalSpeedSettings.defaultSpeed;
                        }
                    }
                    else //We are just pressing forward input
                    {
                        speedMultiplier = tacticalSprintSettings.speed;
                    }
                }

                movement = new Vector3(direction.x * speedMultiplier, this.movement.y, direction.z * speedMultiplier);
            }

            if (currentControllerState == ControllerState.Standing)
            {
                //Set 'speedMultiplier' to run speed or normal speed or backpedal speed
                if (vertical < 0)
                    speedMultiplier = horizontalSpeedSettings.backwardsSpeed;
                else
                {
                    if (isRunning && enableRun)
                    {
                        //We are strafing
                        if (horizontal > 0.05f || horizontal < -0.05f)
                        {
                            //We are strafing and pressing forward input
                            if (runSettings.canRunWhileStrafing && vertical > 0)
                            {
                                speedMultiplier = runSettings.runSpeed;
                            }
                            else
                            {
                                speedMultiplier = horizontalSpeedSettings.defaultSpeed;
                            }
                        }
                        else //We are just pressing forward input
                        {
                            speedMultiplier = runSettings.runSpeed;
                        }
                    }
                    else
                    {
                        speedMultiplier = horizontalSpeedSettings.defaultSpeed;
                    }
                }

                movement = new Vector3(direction.x * speedMultiplier, this.movement.y, direction.z * speedMultiplier);
            }

            if (currentControllerState == ControllerState.Crouched)
            {
                speedMultiplier = crouchSettings.speed;

                movement = new Vector3(direction.x * speedMultiplier, this.movement.y, direction.z * speedMultiplier);
            }

            if (currentControllerState == ControllerState.InAir)
            {
                speedMultiplier = horizontalSpeedSettings.defaultSpeed;

                //Apply air control
                movement = new Vector3(direction.x * speedMultiplier * jumpSettings.airControl, this.movement.y, direction.z * speedMultiplier * jumpSettings.airControl);
            }

            if (currentControllerState == ControllerState.WallRun)
            {
                Vector3 projection = Vector3.ProjectOnPlane(direction, currentWallRunNormal);

                movement = new Vector3(projection.x * wallRunSettings.speedWhileRunning, this.movement.y, projection.z * wallRunSettings.speedWhileRunning);
            }
        }

        private void HandleJump(float dt)
        {
            float currentJumpSpeed = jumpSettings.jumpForce;

            if (previousVelocity.y >= 0 && velocity.y < 0)
            {
                landPositionY = GetColliderCeilPosition().y;
            }

            //Handle landing
            if (previousIsGrounded == false && isGrounded == true)
            {
                if (callbacksEnabled && isMorphingCollider == false)
                    Land();
            }

            if (jumpSettings.jumpsCount == 0)
                return;

            //Handle begin jump
            if (previousIsGrounded == true && isGrounded == false)
            {
                if (callbacksEnabled && isMorphingCollider == false)
                    BeginJump();
            }

            //Ceiling movement constraint
            Ray ray = new Ray(GetColliderCeilPosition(), bodyTransform.up);
            if (Physics.Raycast(ray, 0.05f))
            {
                if (movement.y > 0)
                {
                    jumpLocked = true;
                    movement.y = 0;
                }
            }

            if (isGrounded)
            {
                //Reset jump variables
                currentJumpTimer = 0;
                jumpLocked = false;
                lastTimeGrounded = Time.time;
                currentJumpsCount = 0;
            }

            //Handle multiple jumps
            if (isJumpButtonDown)
            {
                currentJumpsCount++;
            }

            if (isJumpButtonDown)
            {
                if (currentJumpsCount > jumpSettings.jumpsCount)
                {
                    jumpLocked = true;
                }
                else
                {
                    currentJumpTimer = 0;
                }
            }

            //Adjust jump speed depending on the gravity modifier while in wall run (so that a jump height will always be the same depending on the gravity)
            if (currentControllerState == ControllerState.WallRun)
            {
                currentJumpSpeed = jumpSettings.jumpForce * wallRunSettings.wallRunGravity / defaultGravityModifier;
            }

            //Adaptive jump means the character will jump further if you hold down the jump button
            if (jumpSettings.adaptiveJump)
            {
                //Add jump speed only if it still has jumping timer and hasn't released the jump button yet, and the character is grounded or has jumping edge timer
                if ((isJumpButtonBeingPressed && currentJumpTimer < jumpSettings.adaptiveJumpDuration)
                    && (isGrounded || IsJumpEdgeTimer() || (currentJumpsCount < jumpSettings.jumpsCount && jumpSettings.jumpsCount > 1))
                    && jumpLocked == false && currentControllerState != ControllerState.Proned && IsColliderSpaceFree(defaultColliderHeight * 1.5f))
                {
                    if (currentJumpsCount <= jumpSettings.jumpsCount)
                    {
                        OnJumpsCountIncrease(currentJumpsCount);
                    }

                    if (movement.y < 0)
                        movement.y = 0;

                    //Increase jump timer
                    currentJumpTimer += dt;

                    //Add jump speed
                    movement.y += currentJumpSpeed * dt;

                    isTryingToJump = true;

                    StopColliderHeightAutoLerp();
                    ResizeCollider(jumpSettings.inAirColliderHeight);
                }
                else
                {
                    isTryingToJump = false;
                }
            }
            else
            {
                if (isJumpButtonDown
                    && (isGrounded || IsJumpEdgeTimer() || (currentJumpsCount < jumpSettings.jumpsCount && jumpSettings.jumpsCount > 1))
                    && jumpLocked == false && currentControllerState != ControllerState.Proned && IsColliderSpaceFree(defaultColliderHeight * 1.5f))
                {
                    if (currentJumpsCount <= jumpSettings.jumpsCount)
                    {
                        OnJumpsCountIncrease(currentJumpsCount);
                    }

                    movement.y = currentJumpSpeed;
                    isTryingToJump = true;
                    StopColliderHeightAutoLerp();
                    ResizeCollider(jumpSettings.inAirColliderHeight);
                }
                else
                {
                    isTryingToJump = false;
                }
            }

            //Clamp maximum vertical speed
            if (movement.y > jumpSettings.verticalMaxSpeed)
                movement.y = jumpSettings.verticalMaxSpeed;
        }

        private void HandleGravityModifier()
        {
            //Modify gravity while wall running
            if (currentControllerState == ControllerState.WallRun)
            {
                if (InputToMovementDirection().sqrMagnitude > 0.01f)
                {
                    gravityModifier = wallRunSettings.wallRunGravityWhileMoving;
                }
                else
                {
                    gravityModifier = wallRunSettings.wallRunGravity;
                }

                return;
            }

            gravityModifier = defaultGravityModifier;
        }

        private void AddGravity(float dt)
        {
            if (currentControllerState == ControllerState.Grappling)
                return;

            //Add gravity only if not grounded
            if (isGrounded == false)
            {
                movement.y -= -Physics.gravity.y * gravityModifier * dt;
            }
            else
            {
                //If grounded and not trying to jump reset movement y to a arbitrary value
                if (isTryingToJump == false)
                    movement.y = -0.1f;
            }
        }

        private void HandleMomentum(float dt)
        {
            if (currentControllerState == ControllerState.Grappling)
            {
                momentum += grapplingDirection * grapplingHookSettings.speedWhileHooked * dt * grapplingCurrentTimer;
                momentum += Vector3.ProjectOnPlane(cameraTransform.right, tr.up).normalized * horizontal * grapplingHookSettings.horizontalControlStrength * dt;

                releasedMomentum = momentum;
            }

            if (currentControllerState == ControllerState.Sliding)
            {
                //Calculate the direction the slope is direct to 
                Vector3 slideDirection = Vector3.ProjectOnPlane(-tr.up, currentGroundNormal).normalized;

                //Calculate character forward direction projected on the ground
                Vector3 groundDirection = Vector3.ProjectOnPlane(cameraTransform.forward, currentGroundNormal).normalized;

                //We are sliding on a slope
                if (slideDirection.x < 0 || slideDirection.z < 0)
                {
                    momentum += slideDirection * dt * dt * slideSettings.slideGravity;
                }
                else
                {
                    //We are not in a slope, slow down the momentum using 'slideFriction' value
                    momentum = IncrementVectorTowardTargetVector(momentum, slideSettings.groundFriction, dt, Vector3.zero);

                    //Then slow it down again based on the angle the character is facing while sliding. 
                    //If the character is sliding in a direction but he turns the camera the opposite way, he will slow down much faster
                    momentum = IncrementVectorTowardTargetVector(momentum, Vector3.Angle(momentum.normalized, groundDirection) * slideSettings.cameraRotationFrictionFactor, dt, Vector3.zero);
                }

                //Apply horizontal movement control to the momentum
                momentum += Vector3.ProjectOnPlane(cameraTransform.right, tr.up).normalized * horizontal * horizontalSpeedSettings.defaultSpeed * slideSettings.horizontalControl * dt;

                momentum.y = 0;

                //We will use released momentum in the final velocity calculation while sliding
                releasedMomentum = momentum;
            }

            if (currentControllerState == ControllerState.InAir)
            {
                if (momentum.sqrMagnitude > 0.01f)
                {
                    //Slow down the momentum using 'airMomentumFriction' value
                    momentum = IncrementVectorTowardTargetVector(momentum, jumpSettings.airMomentumFriction, dt, Vector3.zero);
                    momentum = IncrementVectorTowardTargetVector(momentum, Mathf.Lerp(0, 100, jumpSettings.airMomentumControl), dt, new Vector3(movement.x, 0, movement.z));
                   
                    //Release the current momentum
                    releasedMomentum = momentum;
                }
                else
                {
                    momentum = releasedMomentum = new Vector3(movement.x, 0, movement.z);
                }

            }

            if (currentControllerState == ControllerState.Standing || currentControllerState == ControllerState.Crouched || currentControllerState == ControllerState.TacticalSprint || currentControllerState == ControllerState.Proned)
            {
                if (InputToMovementDirection().sqrMagnitude < 0.01f)
                {
                    momentum = IncrementVectorTowardTargetVector(momentum, 1 / Mathf.Max(0.01f, horizontalSpeedSettings.movementDeceleration), dt, Vector3.zero);
                    releasedMomentum = momentum;
                }
                else
                {
                    //While in normal locomotion (walking, running etc...), the "stored" momentum will be the horizontal movement
                    Vector3 horizontalMovement = previousMovement - ExtractDotVector(previousMovement, tr.up);
                    momentum = horizontalMovement;
                    releasedMomentum = Vector3.zero;
                }
            }

            momentum = Vector3.ClampMagnitude(momentum, horizontalSpeedSettings.horizontalMaxSpeed);
        }

        private void ApplyMovement(float dt)
        {
            Vector3 finalMovement = Vector3.zero;

            if (currentControllerState == ControllerState.Sliding)
            {
                //Just use the released momentum as movement while sliding
                finalMovement = releasedMomentum;

                //When sliding while in the air use the current gravity force as vertical movement
                if (isGrounded == false || isTryingToJump)
                    finalMovement.y = movement.y;
            }

            if (currentControllerState == ControllerState.Standing || currentControllerState == ControllerState.Crouched)
            {
                Vector3 momentumMovement = movement + releasedMomentum;

                finalMovement = momentumMovement;
            }

            if (currentControllerState == ControllerState.InAir)
            {
                Vector3 verticalMovement = ExtractDotVector(movement, Vector3.up);
                finalMovement = verticalMovement + releasedMomentum;
            }

            if (currentControllerState == ControllerState.TacticalSprint)
            {
                Vector3 momentumMovement = movement + releasedMomentum;

                Vector3 verticalMovement = ExtractDotVector(momentumMovement, Vector3.up);

                //Clamp the horizontal movement to run speed or normal speed while not sliding
                Vector3 clampedHorizontalMovement = Vector3.ClampMagnitude(momentumMovement - verticalMovement, tacticalSprintSettings.speed);

                finalMovement = verticalMovement + clampedHorizontalMovement;
            }

            if (currentControllerState == ControllerState.Proned)
            {
                Vector3 momentumMovement = movement + releasedMomentum;

                Vector3 verticalMovement = ExtractDotVector(momentumMovement, Vector3.up);

                //Clamp the horizontal movement to run speed or normal speed while not sliding
                Vector3 clampedHorizontalMovement = Vector3.ClampMagnitude(momentumMovement - verticalMovement, proneSettings.speed);

                finalMovement = verticalMovement + clampedHorizontalMovement;
            }

            if (currentControllerState == ControllerState.Grappling)
            {
                finalMovement = releasedMomentum;
            }

            if (currentControllerState == ControllerState.WallRun)
            {
                finalMovement = movement;
            }

            //Stick the character on the ground while not jumping to avoid bumps while going down a slope
            if (isGrounded && isTryingToJump == false && isGrappled == false && currentControllerState != ControllerState.Climb)
            {
                finalMovement.y = -2000;
            }

            //Clamp the horizontal movement to maximum horizontal speed
            Vector3 finalMovementVertical = ExtractDotVector(finalMovement, Vector3.up);
            Vector3 finalMovementHorizontalClamped = Vector3.ClampMagnitude(finalMovement - finalMovementVertical, horizontalSpeedSettings.horizontalMaxSpeed);
            finalMovement = finalMovementVertical + finalMovementHorizontalClamped;

            //Clamp the vertical movement to maximum vertical speed
            if (finalMovement.y > jumpSettings.verticalMaxSpeed)
            {
                finalMovement.y = jumpSettings.verticalMaxSpeed;
            }

            //Move the controller
            characterController.Move(finalMovement * dt);

        }

        private void HandlePlatforms()
        {
            //We are on a platform
            if (standingPlatform != null)
            {
                //This is the player world position
                Vector3 newGlobalPlatformPoint = standingPlatform.TransformPoint(currentLocalPositionInStandingPlaform);

                //Movement relative to the previous frame position 
                relativeMovementOnStandingPlatform = newGlobalPlatformPoint - currentPositionInStandingPlaform;

                //If a movement occurred teleport the character so that it stays on the platform
                if (relativeMovementOnStandingPlatform.magnitude > 0.001f)
                {
                    Teleport(tr.position + relativeMovementOnStandingPlatform);
                }

                //Support moving platform rotation
                Quaternion newGlobalPlatformRotation = standingPlatform.rotation * currentLocalRotationInStandingPlatform;
                Quaternion rotationDiff = newGlobalPlatformRotation * Quaternion.Inverse(currentRotationInStandingPlatform);

                //Prevent rotation of the local up vector
                rotationDiff = Quaternion.FromToRotation(rotationDiff * Vector3.up, Vector3.up) * rotationDiff;
                transform.rotation = rotationDiff * transform.rotation;
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

                UpdateMovingPlatformTransform();
            }
            else
            {
                if (relativeMovementOnStandingPlatform.magnitude > 0.01f)
                {
                    relativeMovementOnStandingPlatform = Vector3.Lerp(relativeMovementOnStandingPlatform, Vector3.zero, Time.deltaTime);
                    Teleport(tr.position + relativeMovementOnStandingPlatform);
                }
            }
        }

        private void HandleEdgeFalling()
        {
            //If we are not grounded but the CharacterController says we are grounded it means we are on an edge and we should fall down
            //The CharacterController ground check is much more less accurate, for more information about the Unity CharacterController see
            //https://docs.nvidia.com/gameworks/content/gameworkslibrary/physx/guide/Manual/CharacterControllers.html#kinematic-character-controller
            if (raycastIsGrounded == false && characterController.isGrounded && isClimbingAnimation == false)
            {
                //Create a ray pointing down world space
                if (Physics.SphereCast(GetTransformOrigin() + characterController.center, characterController.radius - characterController.skinWidth, Vector3.down, out var hit, characterController.height * 0.7f))
                {
                    edgeFallDirection = (hit.normal + Vector3.down).normalized;
                    AddMomentum(edgeFallDirection * edgeFallFactor * Time.deltaTime);
                }
            }
        }

        //Use this function to move the character around by any distance. Do not use transform.position as it gets overrided by 'CharacterController' 
        //It can be used to move the character each frame without issues
        public void Teleport(Vector3 worldPosition)
        {
            characterController.enabled = false;
            tr.position = worldPosition;
            characterController.enabled = true;
        }

        private void TryPushObject(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;

            if (body == null || body.isKinematic)
            {
                return;
            }

            //We dont want to push objects below us
            if (hit.moveDirection.y < -0.3)
            {
                return;
            }

            //Calculate push direction from move direction,
            //we only push objects to the sides never up and down
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

            //Apply the push
            body.AddForceAtPosition(pushDir * collisionPushPower, hit.point);

        }

        public void StopColliderHeightAutoLerp()
        {
            StopAllCoroutines();
            isMorphingCollider = false;
        }

        public void SetColliderHeightAutoLerp(float target, float morphSpeed)
        {
            StopColliderHeightAutoLerp();
            StartCoroutine(Co_SetColliderHeight(target, morphSpeed, false));
        }

        public void SetColliderHeightAutoLerp(float target, float speed, bool resize = false)
        {
            StopColliderHeightAutoLerp();
            StartCoroutine(Co_SetColliderHeight(target, speed, resize));
        }

        private IEnumerator Co_SetColliderHeight(float target, float speed, bool resize)
        {
            float threshold = 0.001f;
            isMorphingCollider = true;

            //Loop until the character height is almost equal to target
            while (Mathf.Abs(characterController.height - target) > threshold)
            {
                float oldHeight = characterController.height;
                float newHeight = Mathf.MoveTowards(characterController.height, target, speed * Time.deltaTime);

                characterController.radius = Mathf.Min(newHeight / 2f, colliderMaxRadius);
                characterController.height = newHeight;

                //Re-center the collider in order to maintain the transform position on the base of the collider
                characterController.center = new Vector3(characterController.center.x, characterController.height / 2, characterController.center.z);

                //Since we adjusted the collider center we must also move the character up or down in order to maintain the same position
                if (resize)
                {
                    if (characterController.height < oldHeight)
                    {
                        Teleport(tr.position + Vector3.up * (oldHeight - characterController.height));
                    }
                    else
                    {
                        Teleport(tr.position - Vector3.up * (characterController.height - oldHeight));
                    }
                }

                yield return null;
            }

            characterController.height = target;

            isMorphingCollider = false;

        }

        //Resize the collider height without interpolation
        private void ResizeCollider(float newHeight)
        {
            StopColliderHeightAutoLerp();

            float oldHeight = characterController.height;
            characterController.radius = Mathf.Min(newHeight / 2f, colliderMaxRadius);
            characterController.height = newHeight;
            characterController.center = new Vector3(characterController.center.x, characterController.height / 2, characterController.center.z);

            if (characterController.height < oldHeight)
            {
                Teleport(tr.position + Vector3.up * (oldHeight - characterController.height));
            }
            else
            {
                Teleport(tr.position + Vector3.up * (characterController.height - oldHeight));
            }

            OnColliderResized();
        }

        public void SetMomentum(Vector3 newMomentum)
        {
            momentum = newMomentum;
        }

        public void AddMomentum(Vector3 addMomentum)
        {
            momentum += addMomentum;
        }

        public Vector3 GetPosition()
        {
            return tr.position;
        }

        private float CalculateSlope(Vector3 normal)
        {
            return Vector3.Angle(normal, Vector3.up);
        }

        public Vector3 GetVelocity()
        {
            return velocity;
        }

        public bool IsGrounded()
        {
            return isGrounded;
        }

        //Use this function to check whether with the target collider height the character collides with an object
        private bool IsColliderSpaceFree(float targetHeight)
        {
            float targetRadius = characterController.radius = Mathf.Min(targetHeight / 2f, colliderMaxRadius);

            Vector3 p1 = GetColliderBasePosition() + (Vector3.up * targetRadius) + (Vector3.up * 0.25f);
            Vector3 p2 = GetColliderBasePosition() + (Vector3.up * targetHeight) - (Vector3.up * targetRadius);

            climbGizmosP1 = p1;
            climbGizmosP2 = p2;

            if (Physics.CheckCapsule(p1, p2, characterController.radius, ceilingDetectionLayerMask))
            {
                return false;
            }

            return true;
        }

        //World position of the collider base
        public Vector3 GetColliderBasePosition()
        {
            return tr.TransformPoint(characterController.center - Vector3.up * characterController.height / 2);
        }

        //World position of the collider center
        public Vector3 GetColliderCenterPosition()
        {
            return tr.TransformPoint(characterController.center);
        }

        //World position of the collider ceil
        public Vector3 GetColliderCeilPosition()
        {
            return transform.TransformPoint(characterController.center + Vector3.up * characterController.height / 2);
        }

        public Vector3 GetColliderCeilPositionLocal()
        {
            return characterController.center + (Vector3.up * characterController.height / 2);
        }

        private Vector3 GetTransformOrigin()
        {
            //If it's crouching/sliding the origin position will be at the base of the collider, if it's not it will be at the transform position.
            //We add vertically 0.05f just to make sure to avoid the ray starting under ground in the case of the character being grounded
            return GetColliderBasePosition() + Vector3.up * 0.05f;
        }

        private Vector3 GetGrapplingLineStartPosition()
        {
            return grapplingHookSettings.lineRendererStartPositionTransform.position;
        }

        private bool CheckClimb(bool autoClimb = false)
        {
            //Climbing requires the player pressing the forward input and the character being in air 
            //For example jumping or falling while moving towards a wall

            if ((vertical > 0 && (isGrounded == false || isJumpButtonDown)) || autoClimb)
            {
                Vector3 p1 = GetColliderBasePosition() + Vector3.up * (characterController.radius);
                Vector3 p2 = GetColliderBasePosition() + Vector3.up * (characterController.height) - Vector3.up * (characterController.radius);

                //This capsule cast checks if the character has a climbable object in front of him
                if (Physics.CapsuleCast(p1, p2, characterController.radius, bodyTransform.forward, out var climbDetectionRayHit, climbSettings.maxDistanceFromClimbableObject, climbSettings.climbableObjectLayerMask))
                {
                    //It must face a wall-like obstacle
                    if (CalculateSlope(climbDetectionRayHit.normal) > 80)
                    {
                        int wallCheckSegmentsMaxIterations = 5;
                        for (int i = 0; i < wallCheckSegmentsMaxIterations; i++)
                        {
                            Ray wallCheckSegmentRay = new Ray(GetColliderBasePosition() + Vector3.up * (characterController.stepOffset + 0.01f + climbSettings.maxHeight * i / wallCheckSegmentsMaxIterations), bodyTransform.forward);

                            float wallCheckSegmentRayMaxDistance = climbSettings.maxDistanceFromClimbableObject + 0.01f + characterController.radius;

                            Debug.DrawRay(wallCheckSegmentRay.origin, wallCheckSegmentRay.direction * wallCheckSegmentRayMaxDistance, Color.red);

                            if (!Physics.Raycast(wallCheckSegmentRay, wallCheckSegmentRayMaxDistance, climbSettings.climbableObjectLayerMask))
                            {
                                float climbCharacterRadius = characterController.radius = Mathf.Min(jumpSettings.inAirColliderHeight / 2f, colliderMaxRadius);
                                int ledgeCheckSegmentsMaxIterations = 8;

                                for (int k = 1; k <= ledgeCheckSegmentsMaxIterations; k++)
                                {
                                    Ray ledgeDetectionRay = new Ray(wallCheckSegmentRay.origin + wallCheckSegmentRay.direction * wallCheckSegmentRayMaxDistance * k / ledgeCheckSegmentsMaxIterations, Vector3.down);
                                    // Debug.Break();
                                    Debug.DrawRay(ledgeDetectionRay.origin, ledgeDetectionRay.direction, Color.cyan);

                                    //Get the destination point and begin climbing
                                    if (Physics.Raycast(ledgeDetectionRay, out var freeSpaceHit, Mathf.Abs(wallCheckSegmentRay.origin.y - GetColliderBasePosition().y), climbSettings.climbableObjectLayerMask))
                                    {
                                        Vector3 freeSpaceHitPoint = freeSpaceHit.point;
                                        float distanceFromHitPointToSlopeLevel = climbCharacterRadius * Mathf.Sin(Mathf.Deg2Rad * CalculateSlope(freeSpaceHit.normal)) / Mathf.Sin(Mathf.Deg2Rad * (90 - CalculateSlope(freeSpaceHit.normal)));
                                        Vector3 capsuleCheckFreeSpacePoint1 = new Vector3(freeSpaceHitPoint.x, freeSpaceHitPoint.y + 0.05f + (climbCharacterRadius) + distanceFromHitPointToSlopeLevel, freeSpaceHitPoint.z);
                                        Vector3 capsuleCheckFreeSpacePoint2 = new Vector3(freeSpaceHitPoint.x, freeSpaceHitPoint.y + 0.05f + jumpSettings.inAirColliderHeight - (climbCharacterRadius), freeSpaceHitPoint.z);

                                        if (Physics.CheckCapsule(capsuleCheckFreeSpacePoint1, capsuleCheckFreeSpacePoint2, climbCharacterRadius + 0.01f, climbSettings.climbableObjectLayerMask) == false)
                                        {
                                            Vector3 checkFreeSpaceAboveCharacterOrigin = GetColliderCeilPosition();
                                            Vector3 checkFreeSpaceAboveCharacterDestination = checkFreeSpaceAboveCharacterOrigin + Vector3.up * (freeSpaceHitPoint.y - checkFreeSpaceAboveCharacterOrigin.y + defaultColliderHeight / 2);

                                            if (Physics.Linecast(checkFreeSpaceAboveCharacterOrigin, checkFreeSpaceAboveCharacterDestination, climbSettings.climbableObjectLayerMask) == false)
                                            {
                                                StopColliderHeightAutoLerp();
                                                ResizeCollider(jumpSettings.inAirColliderHeight);
                                                climbEndPoint = freeSpaceHit.point;
                                                climbStartPoint = tr.position;
                                                climbStartDistanceSqr = (climbStartPoint - climbEndPoint).sqrMagnitude;

                                                //Increase timer. 'dt / climbDuration' means the progress is duration based and not speed based. 
                                                climbDuration = Mathf.Min((climbSettings.maxDuration * (climbStartDistanceSqr / (climbSettings.durationMaxDistance * climbSettings.durationMaxDistance))),
                                                    climbSettings.maxDuration);

                                                climbTarget = freeSpaceHit.collider.transform;
                                                climbEndPointRelativeToTarget = climbTarget.InverseTransformPoint(climbEndPoint);

                                                return true;
                                            }
                                        }
                                    }

                                }


                            }

                        }


                    }
                }
            }
            return false;
        }

        private bool IsJumpEdgeTimer()
        {
            return Time.time < lastTimeGrounded + jumpSettings.coyoteTime;
        }

        public bool IsGrapplingOnCooldown()
        {
            return lastTimeGrappling + grapplingHookSettings.cooldown > Time.time;
        }

        private bool IsTacticalSprintDurationOver()
        {
            return currentTacticalSprintTimer >= tacticalSprintSettings.duration;
        }

        private bool CheckWallRunRaycast(Vector3 direction, out Ray ray, out RaycastHit hit)
        {
            ray = new Ray(GetColliderCenterPosition(), direction);
            return Physics.SphereCast(ray, 0.25f, out hit, wallRunSettings.attachMinDistanceCondition + 0.1f, wallRunSettings.walkableObjectLayerMask);
        }

        private void BeginJump()
        {
            if (currentControllerState == ControllerState.Sliding)
            {
                OnEndSlide();
            }

            if (lastTimeJump + jumpEventCooldown < Time.time)
            {
                lastTimeJump = Time.time;

                OnJump();
            }
        }

        private void Land()
        {
            if (currentControllerState == ControllerState.Sliding)
            {
                OnBeginSlide();
                return;
            }

            OnLand(Mathf.Abs(landPositionY - GetColliderCeilPosition().y));
        }

        private void UpdateMovingPlatformTransform()
        {
            currentPositionInStandingPlaform = transform.position;

            //Calculate local position relative to the standing platform
            currentLocalPositionInStandingPlaform = standingPlatform.InverseTransformPoint(transform.position);

            currentRotationInStandingPlatform = transform.rotation;

            //Calculate local rotation relative to the standing platform
            currentLocalRotationInStandingPlatform = Quaternion.Inverse(standingPlatform.rotation) * transform.rotation;
        }

        private void DrawGrapplingLine(float dt)
        {
            if (grapplingLine != null)
            {
                //Enable the line renderer and the hook only when the character is grappled or the destination point has been evaluated
                grapplingLine.enabled = grapplingDestinationPoint.HasValue || isGrappled;
                grapplingLineHook.SetActive(grapplingLine.enabled);

                if (grapplingLine.enabled)
                {

                    //Set up grappling line renderer and spring
                    if (grapplingLine.positionCount == 0)
                    {
                        grapplingLineSpring.SetVelocity(grapplingHookSettings.lineRendererWaveStrength);
                        grapplingLine.positionCount = grapplingHookSettings.lineRendererSegmentCount + 1;
                    }

                    grapplingLineSpring.SetDamper(grapplingLineRendererDamper);
                    grapplingLineSpring.SetStrength(grapplingHookSettings.lineRendererWaveStiffness);
                    grapplingLineSpring.Update(dt);

                    //Find grapple point
                    var grapplePoint = grapplingCurrentPoint - (grapplingCurrentPoint - GetGrapplingLineStartPosition()).normalized * grapplingHookSettings.hookOffsetFromTarget;
                    var grappleStartPosition = GetGrapplingLineStartPosition();
                    var up = Quaternion.LookRotation((grapplePoint - grappleStartPosition).normalized) * Vector3.up;

                    if (grapplingLineSegmentsPositions == null)
                        grapplingLineSegmentsPositions = new Vector3[grapplingHookSettings.lineRendererSegmentCount + 1];

                    //Animate the line renderer segments to simulate a rope
                    for (var i = 0; i < grapplingHookSettings.lineRendererSegmentCount + 1; i++)
                    {
                        var delta = i / (float)grapplingHookSettings.lineRendererSegmentCount;
                        var offset = up * grapplingLineRendererWaveHeight * Mathf.Sin(delta * grapplingHookSettings.lineRendererWaveCount * Mathf.PI) * grapplingLineSpring.Value;

                        Vector3 segmentPosition = Vector3.Lerp(grappleStartPosition, grapplePoint, delta);
                        grapplingLine.SetPosition(i, segmentPosition + offset);

                        grapplingLineSegmentsPositions[i] = segmentPosition;
                    }

                    //Set the hook position to the last segment of the line renderer
                    grapplingLineHook.transform.position = grapplingLineSegmentsPositions[grapplingLineSegmentsPositions.Length - 1];
                }
            }
        }

        //This method gets called when the grappling line has been launched
        private void GrapplingLineBegin(Vector3 target)
        {
            lastTimeGrappling = Time.time;
            grapplingCurrentPoint = GetGrapplingLineStartPosition();
            grapplingDestinationPoint = target;
            grapplingStartDistanceSqr = (grapplingCurrentPoint - grapplingDestinationPoint.Value).sqrMagnitude;

            //If the grappling line hasn't been instantiated yet, instantiate it and set up its properties
            if (grapplingLine == null)
            {
                grapplingLine = new GameObject("Grappling Hook Line").AddComponent<LineRenderer>();
                grapplingLine.material = grapplingHookSettings.lineRendererMaterial;
                grapplingLine.startWidth = grapplingHookSettings.lineRendererWidth;
                grapplingLine.endWidth = grapplingHookSettings.lineRendererWidth;
                grapplingLine.textureMode = LineTextureMode.Tile;
                grapplingLine.alignment = LineAlignment.View;
                grapplingLine.generateLightingData = true;
                grapplingLine.positionCount = grapplingHookSettings.lineRendererSegmentCount + 1;

                grapplingLineHook = Instantiate(grapplingHookSettings.hookPrefab, grapplingLine.transform);
            }

            //Reset the grappling line spring
            grapplingLineSpring.Reset();
            if (grapplingLine.positionCount > 0)
                grapplingLine.positionCount = 0;

            //Rotate the hook to look towards the direction it has been fired
            grapplingLineHook.transform.rotation = Quaternion.LookRotation((grapplingDestinationPoint.Value - grapplingCurrentPoint).normalized, Vector3.up);

            OnBeginGrapplingLine();
        }

        public float GetLastTimeGrappling()
        {
            return lastTimeGrappling;
        }

        private Vector3 InputToMovementDirection()
        {
            Vector3 direction = Vector3.zero;

            //Calculate x and z movement. Projecting on plane guarantees the direction to be horizontal in relation to the character upward direction. 
            //For example if the character was upside down this would guarantee it to still move accurately
            direction += Vector3.ProjectOnPlane(cameraTransform.right, tr.up).normalized * horizontal;
            direction += Vector3.ProjectOnPlane(cameraTransform.forward, tr.up).normalized * vertical;
            direction.y = 0;
            direction.Normalize();

            return direction;
        }

        public Vector2 GetInputDirection()
        {
            return new Vector2(horizontal, vertical);
        }

        private bool IsSlidingButtonPressedDown()
        {
            bool result = isSliding == true && previousIsSliding == false;
            return result;
        }

        public float GetCurrentSpeedSqr()
        {
            return velocity.sqrMagnitude;
        }

        public bool IsTryingToJumpThisFrame()
        {
            return isTryingToJump;
        }

        //Utility

        public Vector3 IncrementVectorTowardTargetVector(Vector3 _currentVector, float _speed, float _deltaTime, Vector3 _targetVector)
        {
            return Vector3.MoveTowards(_currentVector, _targetVector, _speed * _deltaTime);
        }

        public Vector3 ExtractDotVector(Vector3 _vector, Vector3 _direction)
        {
            if (_direction.sqrMagnitude != 1)
                _direction.Normalize();

            float _amount = Vector3.Dot(_vector, _direction);

            return _direction * _amount;
        }

        public static float SignedAngle360(Vector3 from, Vector3 to, Vector3 normal)
        {
            float angle = Vector3.SignedAngle(from, to, normal); //Returns the angle between -180 and 180.
            if (angle < 0)
            {
                angle = 360 - angle * -1;
            }

            return angle;
        }

        public static bool ConeCast(Ray ray, float maxRadius, out RaycastHit hit, float maxDistance, float coneAngle, int layerMask)
        {
            RaycastHit[] sphereCastHits = Physics.SphereCastAll(new Ray(ray.origin, ray.direction), maxRadius, maxDistance, layerMask);

            if (sphereCastHits.Length > 0)
            {
                for (int i = 0; i < sphereCastHits.Length; i++)
                {
                    Vector3 hitPoint = sphereCastHits[i].point;
                    Vector3 directionToHit = hitPoint - ray.origin;
                    float angleToHit = Vector3.Angle(ray.direction, directionToHit);

                    if (angleToHit < coneAngle)
                    {
                        hit = sphereCastHits[i];
                        return true;
                    }
                }
            }

            hit = default;
            return false;
        }

        public static void DrawWireCapsule(Vector3 p1, Vector3 p2, float radius)
        {
#if UNITY_EDITOR
            // Special case when both points are in the same position
            if (p1 == p2)
            {
                // DrawWireSphere works only in gizmo methods
                Gizmos.DrawWireSphere(p1, radius);
                return;
            }
            using (new UnityEditor.Handles.DrawingScope(Gizmos.color, Gizmos.matrix))
            {
                Quaternion p1Rotation = Quaternion.LookRotation(p1 - p2);
                Quaternion p2Rotation = Quaternion.LookRotation(p2 - p1);
                // Check if capsule direction is collinear to Vector.up
                float c = Vector3.Dot((p1 - p2).normalized, Vector3.up);
                if (c == 1f || c == -1f)
                {
                    // Fix rotation
                    p2Rotation = Quaternion.Euler(p2Rotation.eulerAngles.x, p2Rotation.eulerAngles.y + 180f, p2Rotation.eulerAngles.z);
                }
                // First side
                UnityEditor.Handles.DrawWireArc(p1, p1Rotation * Vector3.left, p1Rotation * Vector3.down, 180f, radius);
                UnityEditor.Handles.DrawWireArc(p1, p1Rotation * Vector3.up, p1Rotation * Vector3.left, 180f, radius);
                UnityEditor.Handles.DrawWireDisc(p1, (p2 - p1).normalized, radius);
                // Second side
                UnityEditor.Handles.DrawWireArc(p2, p2Rotation * Vector3.left, p2Rotation * Vector3.down, 180f, radius);
                UnityEditor.Handles.DrawWireArc(p2, p2Rotation * Vector3.up, p2Rotation * Vector3.left, 180f, radius);
                UnityEditor.Handles.DrawWireDisc(p2, (p1 - p2).normalized, radius);
                // Lines
                UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.down * radius, p2 + p2Rotation * Vector3.down * radius);
                UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.left * radius, p2 + p2Rotation * Vector3.right * radius);
                UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.up * radius, p2 + p2Rotation * Vector3.up * radius);
                UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.right * radius, p2 + p2Rotation * Vector3.left * radius);
            }
#endif
        }

        private bool IsInsideMeshCollider(MeshCollider col, Vector3 point)
        {
            var temp = Physics.queriesHitBackfaces;
            Ray ray = new Ray(point, Vector3.back);

            bool hitFrontFace = false;
            RaycastHit hit = default;

            Physics.queriesHitBackfaces = true;
            bool hitFrontOrBackFace = col.Raycast(ray, out RaycastHit hit2, 100f);
            if (hitFrontOrBackFace)
            {
                Physics.queriesHitBackfaces = false;
                hitFrontFace = col.Raycast(ray, out hit, 100f);
            }
            Physics.queriesHitBackfaces = temp;

            if (!hitFrontOrBackFace)
            {
                return false;
            }
            else if (!hitFrontFace)
            {
                return true;
            }
            else
            {
                // This can happen when, for instance, the point is inside the torso but there's a part of the mesh (like the tail) that can still be hit on the front
                if (hit.distance > hit2.distance)
                {
                    return true;
                }
                else
                    return false;
            }

        }

        Vector3 climbGizmosP1;
        Vector3 climbGizmosP2;

        private void OnDrawGizmos()
        {
            return;
            Gizmos.color = Color.red;
            DrawWireCapsule(climbGizmosP1, climbGizmosP2, characterController.radius);
        }

        #endregion

        public class Spring
        {
            private float strength;
            private float damper;
            private float target;
            private float velocity;

            public void Update(float deltaTime)
            {
                var direction = target - Value >= 0 ? 1f : -1f;
                var force = Mathf.Abs(target - Value) * strength;
                velocity += (force * direction - velocity * damper) * deltaTime;
                Value += velocity * deltaTime;
            }

            public void Reset()
            {
                velocity = 0f;
                Value = 0f;
            }

            public void SetValue(float value)
            {
                this.Value = value;
            }

            public void SetTarget(float target)
            {
                this.target = target;
            }

            public void SetDamper(float damper)
            {
                this.damper = damper;
            }

            public void SetStrength(float strength)
            {
                this.strength = strength;
            }

            public void SetVelocity(float velocity)
            {
                this.velocity = velocity;
            }

            public float Value { get; private set; }
        }

    }

}
