using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    #region References
    Rigidbody RB;
    CapsuleCollider PlayerCollider;
    ConstantForce _ConstantForce;
    #endregion

    [Header("Movement"), Space]
    [SerializeField] float RunSpeed;
    [SerializeField] float StrafeSpeed;
    [SerializeField] float BackSpeed;
    [SerializeField] float Acceleration;
    [SerializeField] float Friction;
    [SerializeField] float AirFrictionMult;
    [SerializeField] float DirectionCorrectionMult;

    [Header("External Forces"), Space]
    [SerializeField] float ExternalVelocityDecayRate;
    Vector3 FrameTransientVel;
    Vector3 DecayingTransientVel;
    Vector3 TotalTransientVelAppliedLastFrame;
    Vector3 ForceToApplyThisFrame;

    [Header("States"), Space]
    [SerializeField] MovementState CurrentMovementState;
    [SerializeField] PostureState CurrentPostureState;
    [SerializeField] float StateLerpTime;
    float TargetSpeedModifier;
    public enum MovementState {
        Walk,
        Sprint,
        Slide
    }

    public enum PostureState {
        Stand,
        Crouch,
        Air
    }

    [Header("Aiming"), Space]
    [SerializeField] Vector2 MouseSensitivity;
    [SerializeField] Transform CameraHolder;
    [SerializeField] float CameraFOV;
    [SerializeField] float CamLerpSpeed;
    public Vector3 AimDir;
    Camera Cam;
    Vector3 CameraDefaultPos;
    float xRot = 0f;

    [Header("Sprinting"), Space]
    [SerializeField] float SprintSpeedModifier;
    [SerializeField] float SprintFOV;
    [SerializeField] float FOVLerpPower;
    [SerializeField] float MinSprintVel;
    [SerializeField] float MaxSprintVel;
    [SerializeField] float SprintAccelModifier;
    [SerializeField] float SprintLerpTime;

    [Header("Crouching & Sliding"), Space]
    [SerializeField] float CrouchSpeedModifier;
    [SerializeField] float CrouchCamModifier;
    [SerializeField] Transform CrouchOverlapPoint;
    [SerializeField] float CrouchOverlapRayDist;
    [SerializeField] float ColliderHeightModifier;
    [SerializeField] float SlideAccelModifier;
    [SerializeField] float SlideSpeedModifier;
    [SerializeField] float MinSlideVel;
    [SerializeField] float MaxSlideVel;
    [SerializeField] float CrouchLerpTime;
    [SerializeField] float SlideLerpTime;
    Vector3 DefaultColliderCentre;
    float DefaultColliderHeight;

    [Header("Grounding"), Space]
    [SerializeField] Transform GroundCheckOrigin;
    [SerializeField] int RayNum;
    [SerializeField] float RayOriginDist;
    [SerializeField] float GroundRayDist;
    [SerializeField] LayerMask GroundLayer;
    [SerializeField] float JumpCoyoteTime;
    [SerializeField] float JumpBufferTime;
    float LastGroundedTime;
    float LastPressedJumpTime;

    [Header("Jumping"), Space]
    [SerializeField] float JumpForce;
    [SerializeField] float ExtraConstantGravity;
    [SerializeField] float JumpCutForce;
    [SerializeField] float FallGravityForce;
    [SerializeField] float ApexModifier;
    [SerializeField] float ApexThreshold;
    [SerializeField] float ApexGravityForce;
    bool IsJumping;

    [Header("UI")]
    public TMP_Text UI;
    public TMP_Text VelMeter;

    void Awake() {
        RB = GetComponent<Rigidbody>();
        Cam = CameraHolder.GetComponentInChildren<Camera>();
        PlayerCollider = GetComponent<CapsuleCollider>();

        CameraDefaultPos = CameraHolder.transform.localPosition;
        DefaultColliderHeight = PlayerCollider.height;
        DefaultColliderCentre = PlayerCollider.center;

        CrouchOverlapPoint.localPosition = CameraHolder.localPosition + Vector3.down * CrouchCamModifier;

        _ConstantForce = GetComponent<ConstantForce>();
    }

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        Look();
        CheckJump();

        HandleCrouch();
        DEBUGLOGSTATES();

        HandleStates();
    }

    void FixedUpdate() {
        RemoveTransientVelocity();
        CheckGrounded();
        Move();
        ForceToApplyThisFrame = Vector3.zero;
    }

    void LateUpdate()
    {
        Timer();
    }

    #region Movement

    void Move() {
        if (ForceToApplyThisFrame != Vector3.zero)
        {
            RB.linearVelocity += AdditionalFrameVelocities();
            RB.AddForce(ForceToApplyThisFrame * RB.mass, ForceMode.Impulse);
            return;
        }

        var ExtraForce = new Vector3(0f, 0f, 0f);

        if(IsJumping) {
            if(RB.linearVelocity.y > 0 && InputManager.Instance.JumpInputRelease) {
                ExtraForce.y = -JumpCutForce;
            } else if(RB.linearVelocity.y < 0) {
                ExtraForce.y = -FallGravityForce;
            } else if(Mathf.Abs(RB.linearVelocity.y) < ApexThreshold) {
                ExtraForce.y = ApexGravityForce;
            }
        }
        _ConstantForce.force = ExtraForce * RB.mass;

        var Input = InputManager.Instance.MoveInput;

        // Calculate movement direction in local space
        Vector3 MoveDir = new Vector3(Input.x, 0f, Input.y).normalized;

        // Transform local direction to world space
        Vector3 WorldMoveDir = transform.TransformDirection(MoveDir);

        // Calculate target speeds for X (strafe) and Z (forward/backward) axes
        float TargetStrafeSpeed = StrafeSpeed;
        float TargetZSpeed = Input.y > 0 ? RunSpeed : BackSpeed;

        // Apply state-based speed modifiers
        float CurrentModifier = 1f;

        if (CurrentMovementState == MovementState.Sprint) CurrentModifier = SprintSpeedModifier;
        if (CurrentPostureState == PostureState.Crouch) CurrentModifier = CrouchSpeedModifier;
        if (CurrentMovementState == MovementState.Slide) CurrentModifier = SlideSpeedModifier;

        TargetSpeedModifier = Mathf.Lerp(TargetSpeedModifier, CurrentModifier, Time.deltaTime / StateLerpTime);

        // Apply modifiers to speeds
        TargetStrafeSpeed *= TargetSpeedModifier;
        TargetZSpeed *= TargetSpeedModifier;

        // Calculate acceleration/friction
        Vector3 Step = new Vector3(
            (Input.x == 0 ? Friction : Acceleration) * Time.deltaTime,
            0,
            (Input.y == 0 ? Friction : Acceleration) * Time.deltaTime
        );

        if (LastGroundedTime != JumpCoyoteTime) Step *= AirFrictionMult;
        if (CurrentMovementState == MovementState.Slide) { Step *= SlideAccelModifier; }

        // Get trimmed velocity (ignore vertical movement)
        Vector3 TrimmedVel = transform.InverseTransformDirection(new Vector3(RB.linearVelocity.x, 0, RB.linearVelocity.z));

        // Apply direction correction
        if (Vector3.Dot(TrimmedVel.normalized, WorldMoveDir) < 0) Step *= DirectionCorrectionMult;

        // Calculate target velocity in world space
        Vector3 TargetVelocity = new Vector3(
            MoveDir.x * TargetStrafeSpeed,
            0,
            MoveDir.z * TargetZSpeed
        );

        // Blend between current and target velocity
        Vector3 Smoothed = transform.TransformDirection(new Vector3(Mathf.MoveTowards(TrimmedVel.x, TargetVelocity.x, Step.x),
                                        0f,
                                        Mathf.MoveTowards(TrimmedVel.z, TargetVelocity.z, Step.z)));

        // Apply final velocity
        RB.linearVelocity = Smoothed + Vector3.up * RB.linearVelocity.y + AdditionalFrameVelocities();
    }
    #endregion

    #region External Velocity

    void RemoveTransientVelocity() {
        // Store the current velocity before applying changes
        Vector3 currentVelocity = RB.linearVelocity;
        Vector3 velocityBeforeReduction = currentVelocity;

        // Subtract the transient velocity applied in the last frame
        currentVelocity -= TotalTransientVelAppliedLastFrame;
        RB.linearVelocity = currentVelocity;

        // Reset transient velocity for this frame
        FrameTransientVel = Vector3.zero;
        TotalTransientVelAppliedLastFrame = Vector3.zero;

        // Decay the transient velocity over time
        float decay = Friction * AirFrictionMult * ExternalVelocityDecayRate; // Adjust decay rate as needed
        if ((velocityBeforeReduction.x < 0 && DecayingTransientVel.x < velocityBeforeReduction.x) ||
            (velocityBeforeReduction.x > 0 && DecayingTransientVel.x > velocityBeforeReduction.x) ||
            (velocityBeforeReduction.y < 0 && DecayingTransientVel.y < velocityBeforeReduction.y) ||
            (velocityBeforeReduction.y > 0 && DecayingTransientVel.y > velocityBeforeReduction.y) ||
            (velocityBeforeReduction.z < 0 && DecayingTransientVel.z < velocityBeforeReduction.z) ||
            (velocityBeforeReduction.z > 0 && DecayingTransientVel.z > velocityBeforeReduction.z)) {
            decay *= 5; // Increase decay if velocity is moving away from the transient velocity
        }

        // Smoothly reduce the decaying transient velocity
        DecayingTransientVel = Vector3.MoveTowards(DecayingTransientVel, Vector3.zero, decay * Time.deltaTime);
    }
    
    Vector3 AdditionalFrameVelocities()
    {
        TotalTransientVelAppliedLastFrame = FrameTransientVel + DecayingTransientVel;
        return TotalTransientVelAppliedLastFrame;
    }

    public void AddFrameForce(Vector3 Force, bool resetVelocity = false)
    {
        if (resetVelocity) RB.linearVelocity = Vector2.zero;
        ForceToApplyThisFrame += Force;
    }

    #endregion

    #region Aiming

    void Look() {
        Vector2 Input = InputManager.Instance.ViewInput;

        float MouseX = Input.x * MouseSensitivity.x * Time.deltaTime;
        float MouseY = Input.y * MouseSensitivity.y * Time.deltaTime;

        xRot -= MouseY;
        xRot = Mathf.Clamp(xRot, -75f, 75f);

        CameraHolder.localRotation = Quaternion.Slerp(CameraHolder.localRotation, Quaternion.Euler(xRot, 0f, 0f), CamLerpSpeed);
        transform.Rotate(Vector3.up * MouseX);
        RB.MoveRotation(Quaternion.Slerp(RB.rotation, Quaternion.Euler(RB.rotation.eulerAngles + Vector3.up * MouseX), CamLerpSpeed));

        AimDir = CameraHolder.forward;

        var T = Time.deltaTime / StateLerpTime;

        var TargetFOV = CameraFOV;

        if(CurrentMovementState == MovementState.Sprint) {
            TargetFOV = SprintFOV;
            T = Time.deltaTime / SprintLerpTime;
        }

        T = 1 - Mathf.Exp(-FOVLerpPower * T);

        Cam.fieldOfView = Mathf.Lerp(Cam.fieldOfView,
                                    TargetFOV,
                                    T);
    }

    #endregion

    #region State Handling

    void HandleStates() {
        bool OverlapWhileCrouching = CheckOverlap(CrouchOverlapPoint.position, Vector3.up, CrouchOverlapRayDist) && CurrentPostureState == PostureState.Crouch;
        
        if (LastGroundedTime < 0 && CurrentMovementState != MovementState.Sprint) {
            CurrentPostureState = PostureState.Air;
        } else if(InputManager.Instance.CrouchInput || OverlapWhileCrouching) {
            CurrentPostureState = PostureState.Crouch;
        } else {
            CurrentPostureState = PostureState.Stand;
        }

        bool VelAboveMinBound = CurrentMovementState == MovementState.Sprint && RB.linearVelocity.magnitude >= MinSprintVel;
        bool VelAboveMaxBound = CurrentMovementState != MovementState.Sprint && RB.linearVelocity.magnitude >= MaxSprintVel; // This is to prevent instant switching over the boundary (Schmitt Trigger)

        bool SprintVelWithinBounds = VelAboveMinBound || VelAboveMaxBound;
        bool ShouldSprint = false;

        VelAboveMinBound = CurrentMovementState == MovementState.Slide && RB.linearVelocity.magnitude >= MinSlideVel;
        VelAboveMaxBound = CurrentMovementState != MovementState.Slide && RB.linearVelocity.magnitude >= MaxSlideVel;

        bool SlideVelWithinBounds = VelAboveMinBound || VelAboveMaxBound;
        
        if (InputManager.Instance.SprintInput && SprintVelWithinBounds) {
            ShouldSprint = true;
        }
        else {
            CurrentMovementState = MovementState.Walk;
        }
        

        if(CurrentPostureState == PostureState.Crouch && SlideVelWithinBounds) {
            CurrentMovementState = MovementState.Slide;
        } else if(ShouldSprint && (CurrentPostureState == PostureState.Air && CurrentMovementState == MovementState.Slide || CurrentPostureState == PostureState.Stand && CurrentMovementState != MovementState.Slide)) {
            CurrentMovementState = MovementState.Sprint;
        }
    }

    #endregion

    #region Crouching

    void HandleCrouch() {
        var T = Time.deltaTime / CrouchLerpTime;
        
        Vector3 TargetCamPos;
        Vector3 TargetColliderCentre;
        float TargetColliderHeight;

        if (!InputManager.Instance.CrouchInput && !CheckOverlap(CrouchOverlapPoint.position, Vector3.up, CrouchOverlapRayDist)) {
            TargetCamPos = CameraDefaultPos;
            TargetColliderCentre = DefaultColliderCentre;
            TargetColliderHeight = DefaultColliderHeight;
        }
        else {
            TargetCamPos = CameraDefaultPos + Vector3.down * CrouchCamModifier;
            TargetColliderHeight = DefaultColliderHeight * ColliderHeightModifier;
            TargetColliderCentre = DefaultColliderCentre + Vector3.down * ((DefaultColliderHeight - TargetColliderHeight) / 2f);
        }
        
        CameraHolder.localPosition = Vector3.Lerp(CameraHolder.localPosition, TargetCamPos, T);
        PlayerCollider.height = Mathf.Lerp(PlayerCollider.height, TargetColliderHeight, T);
        PlayerCollider.center = Vector3.Lerp(PlayerCollider.center, TargetColliderCentre, T); 
    }

    #endregion

    #region Collision
    List<Vector3> SpreadSpawnPositionsAroundOrigin() {
        List<Vector3> RaySpawnPositions = new List<Vector3>
        {
            Vector3.zero
        };

        for (int i = 0; i < RayNum; i++)
        {
            float Angle = i * (Mathf.PI * 2 / RayNum);

            Vector3 Offset = new Vector3(Mathf.Sin(Angle), 0f, Mathf.Cos(Angle));
            Offset *= RayOriginDist;

            RaySpawnPositions.Add(Offset);
        }

        return RaySpawnPositions;
    }


    bool CheckOverlap(Vector3 Origin, Vector3 Direction, float RayDist) {
        List<Vector3> RaySpawnPositions = SpreadSpawnPositionsAroundOrigin();

        foreach (var Offset in RaySpawnPositions)
        {
            if(Physics.RaycastAll(Origin + Offset, Direction, RayDist).Where(Coll => Coll.transform != transform).ToArray().Length > 0) { return true; }
        }

        return false;
    }

    void CheckGrounded() {
        if(CheckOverlap(GroundCheckOrigin.position, Vector3.down, GroundRayDist)) {
            LastGroundedTime = JumpCoyoteTime;

            _ConstantForce.force = Vector2.zero;
            
            if (IsJumping && RB.linearVelocity.y <= 0.1f) {
                IsJumping = false;
            }

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, GroundRayDist)) {
                float distanceToGround = hit.distance;
                float requiredCorrection = GroundRayDist - distanceToGround;
                if (requiredCorrection > 0) {
                    RB.MovePosition(RB.position + Vector3.down * requiredCorrection);
                }
            }
        }
    }

    #endregion

    #region Jumping

    void CheckJump() {
        if (InputManager.Instance.JumpInputPress) {
            OnJump();
        }

        if (CurrentPostureState != PostureState.Air && LastPressedJumpTime > 0 && !IsJumping) {
            Jump();
        }
    }

    void OnJump() {
        LastPressedJumpTime = JumpBufferTime;
    }

    void Jump() {
        RB.linearVelocity = new Vector3(RB.linearVelocity.x, JumpForce, RB.linearVelocity.z);
        LastGroundedTime = 0;
        LastPressedJumpTime = 0;
        IsJumping = true;
    }

    #endregion

    void DEBUGLOGSTATES() {
        string UITXT;
        if (CurrentPostureState == PostureState.Air) {
            if(IsJumping) {
                if(CurrentMovementState == MovementState.Walk) {
                    UITXT = "Jumping";
                } else if(CurrentMovementState == MovementState.Sprint) {
                    UITXT = "Sprint Jumping";
                } else {
                    UITXT = "Slide Jumping";
                }
            } else {
                if(CurrentMovementState == MovementState.Walk) {
                    UITXT = "Falling";
                } else if(CurrentMovementState == MovementState.Sprint) {
                    UITXT = "Sprint Falling";
                } else {
                    UITXT = "Slide Falling";
                }
            }
        } else if(CurrentPostureState == PostureState.Crouch) {
            if(CurrentMovementState == MovementState.Walk) {
                UITXT = "Crouching";
            } else {
                UITXT = "Sliding";
            }
        } else {
            if(CurrentMovementState == MovementState.Walk) {
                UITXT = "Walking";
            } else if(CurrentMovementState == MovementState.Sprint) {
                UITXT = "Sprinting";
            } else {
                UITXT = "Sliding";
            }
        }

        UI.text = UITXT;
        VelMeter.text = Mathf.Round(RB.linearVelocity.magnitude * 1000f) / 1000f  + " M/S";
    }

    void Timer() {
        LastGroundedTime -= Time.deltaTime;
        LastPressedJumpTime -= Time.deltaTime;
    }
}