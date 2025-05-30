using KinematicCharacterController;
using UnityEngine;
using TMPro;
using System;

#region State Management
public enum InputType
{
    None, Toggle, Hold
}

public enum Stance
{
    Stand, Sprint, Crouch, Slide
}

public struct CharacterState
{
    public bool IsGrounded;
    public Stance Stance;
    public Vector3 Velocity;
    public Vector3 Acceleration;
}
public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector3 Move;
    public bool Jump;
    public bool JumpSustain;
    public InputType Crouch;
    public InputType Sprint;
}

#endregion

public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    #region References
    [Header("References")]
    [SerializeField] KinematicCharacterMotor Motor;
    [SerializeField] Transform Root;
    [SerializeField] Transform CameraTarget;
    #endregion

    [Space]

    #region Player Settings
    [Header("Movement Settings")]
    [SerializeField] float ForwardSpeed;
    [SerializeField] float BackwardSpeed;
    [SerializeField] float StrafeSpeed;
    [SerializeField] float WalkAccelerationResponse;
    [SerializeField] float WalkDecelerationResponse;
    [SerializeField] float WalkThreshold;
    [SerializeField][Range(0f, 1f)] float VelocityFilterTau;
    [SerializeField][Range(0f, 1f)] float NormalFilterTau;
    Vector3 GroundNormal;

    [Space]

    [Header("Aerial Settings")]
    [SerializeField] float Gravity;
    [SerializeField] float AirSpeed;
    [SerializeField] float AirAcceleration;
    [SerializeField] float AirDeceleration;
    [SerializeField] float MaxFallSpeed;

    [Space]

    [Header("Jump Settings")]
    [SerializeField] float JumpSpeed;
    [SerializeField] float CoyoteTime;
    [SerializeField] float ApexJumpThreshold;
    [SerializeField] float ApexSpeedModifier;
    [SerializeField] float ApexAccelerationModifier;
    [Range(0f, 1f)][SerializeField] float JumpSustainGravity;

    [Space]

    [Header("Crouch Settings")]
    public InputType CrouchInputType;
    [SerializeField] float CrouchForwardSpeed;
    [SerializeField] float CrouchBackwardSpeed;
    [SerializeField] float CrouchStrafeSpeed;
    [SerializeField] float CrouchAccelerationResponse;
    [SerializeField] float CrouchDecelerationResponse;

    [SerializeField] float StandHeight;
    [SerializeField] float CrouchHeight;

    [Range(0f, 1f)][SerializeField] float StandCameraTargetHeight;
    [Range(0f, 1f)][SerializeField] float CrouchCameraTargetHeight;

    [SerializeField] float CrouchHeightResponse;

    [SerializeField] float SlamForce;
    [SerializeField] float SlamHeightThreshold;

    [Space]

    [Header("Slide Settings")]
    [SerializeField] float SlideSpeed;
    [SerializeField] float SlideEndSpeed;
    [SerializeField] float SlideAcceleration;
    [SerializeField] float SlideFriction;
    [SerializeField] float SlideSteerAcceleration;
    [SerializeField] float SlideGravity;
    [SerializeField] float SlopeAngleThreshold;
    [SerializeField] float UphillFactor;
    [SerializeField] float DownhillFactor;

    [Space]

    [Header("Sprint Settings")]
    public InputType SprintInputType;
    [SerializeField] float SprintForwardSpeed;
    [SerializeField] float SprintBackwardSpeed;
    [SerializeField] float SprintStrafeSpeed;
    [SerializeField] float SprintAccelerationResponse;
    [SerializeField] float SprintDecelerationResponse;
    [SerializeField] float SprintMinimumSpeed;
    [SerializeField] float SprintJumpSpeed;
    #endregion

    [Space]

    #region Debugging
    [Header("Debug UI")]

    [SerializeField] TMP_Text StanceText;
    [SerializeField] TMP_Text GroundedText;
    [SerializeField] TMP_Text VelocityText;
    #endregion

    #region States
    [HideInInspector] public CharacterState State;
    CharacterState LastState;
    CharacterState TempState;
    #endregion

    #region Requested Inputs
    Quaternion RequestedRotation;
    Vector3 RequestedMovement;
    bool RequestedJump;
    bool RequestedSustainedJump;
    bool RequestedCrouch;
    bool RequestedCrouchInAir;
    bool RequestedSlide;
    bool RequestedSlam;
    bool RequestedSprint;
    #endregion

    #region Jump Timers
    float TimeSinceUngrounded;
    float TimeSinceJumpRequest;
    bool UngroundedDueToJump;
    #endregion

    Collider[] UncrouchOverlapResults;

    public void Initialise()
    {
        Motor = GetComponent<KinematicCharacterMotor>();
        Motor.CharacterController = this;

        State.Stance = Stance.Stand;

        GroundNormal = Motor.GroundingStatus.GroundNormal;

        UncrouchOverlapResults = new Collider[8];
    }

    public void UpdateInput(CharacterInput Input)
    {
        RequestedRotation = Input.Rotation;

        RequestedMovement = new Vector3(Input.Move.x, 0, Input.Move.y).normalized;
        RequestedMovement = Input.Rotation * RequestedMovement;

        var WasRequestingJump = RequestedJump;
        RequestedJump = Input.Jump || RequestedJump;

        if (RequestedJump && !WasRequestingJump)
        {
            TimeSinceJumpRequest = 0f;
        }

        RequestedSustainedJump = Input.JumpSustain;

        var WasRequestedCrouch = RequestedCrouch;

        if (Input.Crouch == InputType.Toggle)
        {
            RequestedCrouch = !RequestedCrouch;
        }
        else if (CrouchInputType == InputType.Hold)
        {
            RequestedCrouch = Input.Crouch == InputType.Hold;
        }

        if (RequestedCrouch && !WasRequestedCrouch)
        {
            RequestedCrouchInAir = !State.IsGrounded;

            if (RequestedCrouchInAir)
            {
                RequestedSlam = true;
            }
        }
        else if (!RequestedCrouch && WasRequestedCrouch)
        {
            RequestedCrouchInAir = false;
        }

        if (Input.Sprint == InputType.Toggle)
        {
            RequestedSprint = !RequestedSprint;
        }
        else if (SprintInputType == InputType.Hold)
        {
            RequestedSprint = Input.Sprint == InputType.Hold;
        }
    }

    public void UpdateBody(float DeltaTime)
    {
        var CurrentHeight = Motor.Capsule.height;
        var CameraTargetHeight = CurrentHeight * (State.Stance is Stance.Stand or Stance.Sprint
            ? StandCameraTargetHeight
            : CrouchCameraTargetHeight);

        var NormalizedHeight = CurrentHeight / StandHeight;

        var RootTargetScale = new Vector3(1f, NormalizedHeight, 1f);

        Root.localScale = Vector3.Lerp
        (
            Root.localScale,
            RootTargetScale,
            1f - Mathf.Exp(-CrouchHeightResponse * DeltaTime)
        );

        CameraTarget.localPosition = Vector3.Lerp
        (
            CameraTarget.localPosition,
            new Vector3(0f, CameraTargetHeight, 0f),
            1f - Mathf.Exp(-CrouchHeightResponse * DeltaTime)
        );
    }

    public void UpdateVelocity(ref Vector3 CurrentVelocity, float DeltaTime)
    {
        State.Acceleration = Vector3.zero;
        if (Motor.GroundingStatus.IsStableOnGround)
        {
            TimeSinceUngrounded = 0f;
            UngroundedDueToJump = false;

            var StableNormal = StabiliseNormal(GroundNormal, Motor.GroundingStatus.GroundNormal, NormalFilterTau, DeltaTime);

            var GroundedMovement = Motor.GetDirectionTangentToSurface
            (
                direction: RequestedMovement,
                surfaceNormal: StableNormal
            ) * RequestedMovement.magnitude;

            CheckSprint(ref CurrentVelocity);

            CheckSlide(ref CurrentVelocity, ref GroundedMovement, DeltaTime);

            if (State.Stance is Stance.Stand or Stance.Crouch or Stance.Sprint)
            {
                Move(ref CurrentVelocity, DeltaTime);
            }
            else
            {
                HandleSlide(ref CurrentVelocity, ref GroundedMovement, DeltaTime);
            }
        }
        else
        {
            TimeSinceUngrounded += DeltaTime;

            AerialMove(ref CurrentVelocity, DeltaTime);
        }

        if (RequestedJump)
        {
            CheckJump(ref CurrentVelocity, DeltaTime);
        }

        if (RequestedSlam)
        {
            CheckSlam(ref CurrentVelocity, DeltaTime);
        }

    }

    private void CheckSlam(ref Vector3 CurrentVelocity, float DeltaTime)
    {
        if (Motor.GroundingStatus.IsStableOnGround)
        {
            RequestedSlam = false;
            return;
        }

        Ray GroundCheck = new Ray(Motor.TransientPosition, -Motor.CharacterUp);
        var EffectiveSlamForce = SlamForce;
        if (Physics.Raycast(GroundCheck, out RaycastHit Hit, SlamHeightThreshold, Motor.CollidableLayers, QueryTriggerInteraction.Ignore))
        {
            EffectiveSlamForce = Mathf.Lerp(0f, SlamHeightThreshold, Hit.distance);
        }
        CurrentVelocity += -Motor.CharacterUp * SlamForce * DeltaTime;
    }

    void CheckJump(ref Vector3 CurrentVelocity, float DeltaTime)
    {
        var CanCoyoteJump = TimeSinceUngrounded < CoyoteTime && !UngroundedDueToJump;

        if (Motor.GroundingStatus.IsStableOnGround || CanCoyoteJump)
        {
            RequestedJump = false;
            RequestedCrouch = false;
            RequestedCrouchInAir = false;

            Motor.ForceUnground(time: 0.1f);
            UngroundedDueToJump = true;

            var TargetJumpSpeed = State.Stance is Stance.Sprint ? SprintJumpSpeed : JumpSpeed;

            var CurrentVerticalSpeed = Vector3.Dot(CurrentVelocity, Motor.CharacterUp);
            var TargetVerticalSpeed = Mathf.Max(CurrentVerticalSpeed, TargetJumpSpeed);

            CurrentVelocity += Motor.CharacterUp * (TargetVerticalSpeed - CurrentVerticalSpeed);
        }
        else
        {
            TimeSinceJumpRequest += DeltaTime;

            var CanJumpLater = TimeSinceJumpRequest < CoyoteTime;
            RequestedJump = CanJumpLater;
        }
    }

    void Move(ref Vector3 CurrentVelocity, float DeltaTime)
    {
        Vector3 Velocity;
        Vector3 LocalRequestedMovement = transform.InverseTransformDirection(RequestedMovement);
        LocalRequestedMovement.y = 0f;

        float AccelResponse;
        float DecelResponse;
        if (State.Stance is Stance.Sprint)
        {
            AccelResponse = SprintAccelerationResponse;
            DecelResponse = SprintDecelerationResponse;

            Velocity = new Vector3
            (
                x: SprintStrafeSpeed,
                y: 0f,
                z: LocalRequestedMovement.z > 0f ? SprintForwardSpeed : SprintBackwardSpeed
            );
        }
        else if (State.Stance is Stance.Crouch)
        {
            AccelResponse = CrouchAccelerationResponse;
            DecelResponse = CrouchDecelerationResponse;

            Velocity = new Vector3
            (
                x: CrouchStrafeSpeed,
                y: 0f,
                z: LocalRequestedMovement.z > 0f ? CrouchForwardSpeed : CrouchBackwardSpeed
            );
        }
        else
        {
            AccelResponse = WalkAccelerationResponse;
            DecelResponse = WalkDecelerationResponse;

            Velocity = new Vector3
            (
                x: StrafeSpeed,
                y: 0f,
                z: LocalRequestedMovement.z > 0f ? ForwardSpeed : BackwardSpeed
            );
        }

        Vector3 Response = new Vector3(
            x: Mathf.Abs(LocalRequestedMovement.x) > WalkThreshold ? AccelResponse : DecelResponse,
            y: 0f,
            z: Mathf.Abs(LocalRequestedMovement.z) > WalkThreshold ? AccelResponse : DecelResponse
        );

        var StableNormal = StabiliseNormal(GroundNormal, Motor.GroundingStatus.GroundNormal, NormalFilterTau, DeltaTime);

        Vector3 LocalCurrent = transform.InverseTransformDirection(CurrentVelocity);
        Vector3 LocalTarget = new Vector3
        (
            x: Velocity.x * LocalRequestedMovement.x,
            y: 0f,
            z: Velocity.z * LocalRequestedMovement.z
        );

        LocalCurrent.x = Mathf.Lerp
        (
            a: LocalCurrent.x,
            b: LocalTarget.x,
            t: Response.x * DeltaTime
        );

        LocalCurrent.z = Mathf.Lerp
        (
            a: LocalCurrent.z,
            b: LocalTarget.z,
            t: Response.z * DeltaTime
        );

        Vector3 SlopeVelocity = Motor.GetDirectionTangentToSurface
        (
            direction: transform.TransformDirection(LocalCurrent),
            surfaceNormal: StableNormal
        ) * LocalCurrent.magnitude;

        float Alpha = DeltaTime / (VelocityFilterTau + DeltaTime);


        var MoveVelocity = Vector3.Lerp(CurrentVelocity, SlopeVelocity, Alpha);
        State.Acceleration = (MoveVelocity - CurrentVelocity) / DeltaTime;
        CurrentVelocity = MoveVelocity;
    }

    void AerialMove(ref Vector3 CurrentVelocity, float DeltaTime)
    {
        if (RequestedMovement.sqrMagnitude > 0.1f)
        {
            var PlanarMovement = Vector3.ProjectOnPlane
            (
                RequestedMovement,
                Motor.CharacterUp
            ).normalized * RequestedMovement.magnitude;

            var CurrentPlanarVelocity = Vector3.ProjectOnPlane
            (
                CurrentVelocity,
                Motor.CharacterUp
            );

            var MovementForce = PlanarMovement * AirAcceleration * DeltaTime;

            if (UngroundedDueToJump && Mathf.Abs(CurrentVelocity.y) < ApexJumpThreshold)
            {
                MovementForce *= ApexSpeedModifier;
                MovementForce += (ApexAccelerationModifier - 1f) * PlanarMovement * DeltaTime;
            }

            if (CurrentPlanarVelocity.magnitude < AirSpeed)
            {
                var TargetPlanarVelocity = CurrentPlanarVelocity + MovementForce;

                TargetPlanarVelocity = Vector3.ClampMagnitude(TargetPlanarVelocity, AirSpeed);
                MovementForce = TargetPlanarVelocity - CurrentPlanarVelocity;
            }
            else if (Vector3.Dot(CurrentPlanarVelocity, MovementForce) > 0f)
            {
                var ConstrainedMovementForce = Vector3.ProjectOnPlane
                (
                    vector: MovementForce,
                    planeNormal: CurrentPlanarVelocity.normalized
                );

                MovementForce = ConstrainedMovementForce;
            }

            if (Motor.GroundingStatus.FoundAnyGround)
            {
                if (Vector3.Dot(MovementForce, CurrentVelocity + MovementForce) > 0f)
                {
                    Vector3 StableNormal = StabiliseNormal(GroundNormal, Motor.GroundingStatus.GroundNormal, NormalFilterTau, DeltaTime);
                    var ObstructionNormal = Vector3.Cross
                    (
                        Motor.CharacterUp,
                        Vector3.Cross(Motor.CharacterUp, StableNormal)
                    ).normalized;

                    MovementForce = Vector3.ProjectOnPlane
                    (
                        vector: MovementForce,
                        planeNormal: ObstructionNormal
                    );
                }
            }
            var AerialVelocity = CurrentVelocity;
            AerialVelocity += MovementForce;

            State.Acceleration = (AerialVelocity - CurrentVelocity) / DeltaTime;
            CurrentVelocity = AerialVelocity;
        }
        var VerticalSpeed = Vector3.Dot(CurrentVelocity, Motor.CharacterUp);
        var EffectiveGravity = Gravity * (RequestedSustainedJump && VerticalSpeed > 0f ? JumpSustainGravity : 1f);
        CurrentVelocity += Motor.CharacterUp * EffectiveGravity * DeltaTime;

        var DownwardVelocity = Vector3.Project(CurrentVelocity, -Motor.CharacterUp);
        if (DownwardVelocity.magnitude > MaxFallSpeed)
        {
            var ExcessVelocity = DownwardVelocity - DownwardVelocity.normalized * MaxFallSpeed;
            CurrentVelocity -= ExcessVelocity;
        }
    }

    void CheckSprint(ref Vector3 CurrentVelocity)
    {
        float Speed = CurrentVelocity.magnitude;
        if (RequestedSprint && (RequestedMovement.sqrMagnitude < 0.1f || Speed < SprintMinimumSpeed))
        {
            RequestedSprint = false;

            if (State.Stance is Stance.Sprint)
            {
                State.Stance = Stance.Stand;
            }

            return;
        }

        if (State.Stance is Stance.Stand && RequestedSprint)
        {
            State.Stance = Stance.Sprint;
        }
        else if (State.Stance is Stance.Sprint && !RequestedSprint)
        {
            State.Stance = Stance.Stand;
        }
    }

    void CheckSlide(ref Vector3 CurrentVelocity, ref Vector3 GroundedMovement, float DeltaTime)
    {
        bool IsMoving = GroundedMovement.sqrMagnitude > 0.1f;
        bool IsCrouching = State.Stance is Stance.Crouch;
        bool WasStanding = LastState.Stance is Stance.Stand;
        bool WasSprinting = LastState.Stance is Stance.Sprint;
        bool WasInAir = !LastState.IsGrounded;

        if (IsMoving && IsCrouching && (WasStanding || WasSprinting || WasInAir))
        {
            State.Stance = Stance.Slide;
            RequestedSlide = true;

            var StableNormal = StabiliseNormal(GroundNormal, Motor.GroundingStatus.GroundNormal, NormalFilterTau, DeltaTime);

            if (WasInAir)
            {
                CurrentVelocity = Vector3.ProjectOnPlane
                (
                    vector: LastState.Velocity,
                    planeNormal: StableNormal
                );
            }

            if (!LastState.IsGrounded && !RequestedCrouchInAir)
            {
                RequestedSlide = false;
                RequestedCrouchInAir = false;
            }

            if (LastState.Velocity.magnitude > SlideSpeed)
            {
                CurrentVelocity = CurrentVelocity.normalized * LastState.Velocity.magnitude;
            }
        }
    }

    void HandleSlide(ref Vector3 CurrentVelocity, ref Vector3 GroundedMovement, float DeltaTime)
    {
        float Speed = CurrentVelocity.magnitude;

        if (RequestedSlide && State.Stance is Stance.Slide)
        {
            if (GroundedMovement.sqrMagnitude < 0.1f)
            {
                Speed = Mathf.Lerp(Speed, 0f, SlideFriction * DeltaTime);
            }
            else if (Speed > SlideSpeed)
            {
                Speed = Mathf.Lerp(Speed, SlideSpeed, SlideFriction * DeltaTime);
            }
            else if (Speed < SlideSpeed)
            {
                Speed = Mathf.Lerp(Speed, SlideSpeed, SlideAcceleration * DeltaTime);
            }
        }

        Vector3 SlideVelocity = CurrentVelocity.normalized * Speed;

        float Alpha = 1f - Mathf.Exp(-DeltaTime / VelocityFilterTau);
        CurrentVelocity = Vector3.Lerp(CurrentVelocity, SlideVelocity, Alpha);

        var StableNormal = StabiliseNormal(GroundNormal, Motor.GroundingStatus.GroundNormal, NormalFilterTau, DeltaTime);

        {
            Vector3 SlopeDown = Vector3.ProjectOnPlane(-Motor.CharacterUp, StableNormal).normalized;
            float DownhillDot = Vector3.Dot(CurrentVelocity.normalized, SlopeDown);

            float Factor = 1f;

            if (DownhillDot > SlopeAngleThreshold)
            {
                Factor = DownhillFactor;
            }
            else if (DownhillDot < -SlopeAngleThreshold)
            {
                Factor = UphillFactor;
            }

            var Force = Vector3.ProjectOnPlane
            (
                vector: Motor.CharacterUp,
                planeNormal: StableNormal
            ) * SlideGravity * Factor;

            CurrentVelocity += Force * DeltaTime;
        }

        {
            Speed = CurrentVelocity.magnitude;
            var SteerVelocity = CurrentVelocity;

            var TargetVelocity = GroundedMovement * Speed;
            var SteerForce = (TargetVelocity - SteerVelocity) * SlideSteerAcceleration * DeltaTime;
            SteerVelocity += SteerForce;

            SteerVelocity = SteerVelocity.normalized * Speed;

            State.Acceleration = (SteerVelocity - CurrentVelocity) / DeltaTime;
            CurrentVelocity = SteerVelocity;
        }

        if (CurrentVelocity.magnitude < SlideEndSpeed)
        {
            State.Stance = Stance.Crouch;
        }
    }

    public void UpdateRotation(ref Quaternion CurrentRotation, float DeltaTime)
    {
        var Forward = Vector3.ProjectOnPlane(RequestedRotation * Vector3.forward, Motor.CharacterUp).normalized; // Ensures character only rotates in the plane of the ground

        if (Forward != Vector3.zero)
        {
            CurrentRotation = Quaternion.LookRotation(Forward, Motor.CharacterUp);
        }
    }

    public void BeforeCharacterUpdate(float DeltaTime)
    {
        TempState = State;

        if (RequestedCrouch && State.Stance is Stance.Stand or Stance.Sprint)
        {
            State.Stance = Stance.Crouch;

            Motor.SetCapsuleDimensions(
                radius: Motor.Capsule.radius,
                height: CrouchHeight,
                yOffset: CrouchHeight / 2f
            );
        }
    }
    public void PostGroundingUpdate(float DeltaTime)
    {
        if (!Motor.GroundingStatus.IsStableOnGround && State.Stance is Stance.Slide)
        {
            State.Stance = Stance.Crouch;
        }
    }
    public void AfterCharacterUpdate(float DeltaTime)
    {
        var TotalAcceleration = (State.Velocity - LastState.Velocity) / DeltaTime;
        State.Acceleration = Vector3.ClampMagnitude(State.Acceleration, TotalAcceleration.magnitude);

        GroundNormal = Motor.GroundingStatus.GroundNormal;

        if (!RequestedCrouch && State.Stance is Stance.Crouch or Stance.Slide)
        {
            Motor.SetCapsuleDimensions(
                radius: Motor.Capsule.radius,
                height: StandHeight,
                yOffset: StandHeight / 2f
            );

            if (Motor.CharacterOverlap(Motor.TransientPosition, Motor.TransientRotation, UncrouchOverlapResults, Motor.CollidableLayers, QueryTriggerInteraction.Ignore) > 0)
            {
                RequestedCrouch = true;

                Motor.SetCapsuleDimensions(
                    radius: Motor.Capsule.radius,
                    height: CrouchHeight,
                    yOffset: CrouchHeight / 2f
                );
            }
            else
            {
                State.Stance = Stance.Stand;
            }
        }

        State.IsGrounded = Motor.GroundingStatus.IsStableOnGround;
        State.Velocity = Motor.Velocity;

        LastState = TempState;

        StanceText.text = $"State: {State.Stance}";
        GroundedText.text = State.IsGrounded ? "Grounded" : "In Air";
        VelocityText.text = $"Velocity: {Motor.Velocity.magnitude:F2}";
    }

    Vector3 StabiliseNormal(Vector3 LastNormal, Vector3 RawNormal, float NormalFilterTau, float DeltaTime, float AngleThreshold = 1f)
    {
        const float kEpsilon = 1e-6f;
        if (RawNormal.sqrMagnitude < kEpsilon)
        {
            return LastNormal;
        }

        float Alpha = 1f - Mathf.Exp(-DeltaTime / NormalFilterTau);
        Vector3 Filtered = Vector3.Lerp(LastNormal, RawNormal, Alpha).normalized;

        if (Vector3.Angle(Filtered, RawNormal) > AngleThreshold)
        {
            Filtered = RawNormal;
        }

        return Filtered;
    }

    public void OnGroundHit(Collider HitCollider, Vector3 HitNormal, Vector3 HitPoint, ref HitStabilityReport HitStabilityReport) { }
    public void OnMovementHit(Collider HitCollider, Vector3 HitNormal, Vector3 HitPoint, ref HitStabilityReport HitStabilityReport)
    {
        State.Acceleration = Vector3.ProjectOnPlane(State.Acceleration, HitNormal);
    }
    public bool IsColliderValidForCollisions(Collider Coll) => true;
    public void OnDiscreteCollisionDetected(Collider HitCollider) { }
    public void ProcessHitStabilityReport(Collider HitCollider, Vector3 HitNormal, Vector3 HitPoint, Vector3 AtCharacterPosition, Quaternion AtCharacterRotation, ref HitStabilityReport HitStabilityReport) { }

    public Transform GetCameraTarget() => CameraTarget;
    public CharacterState GetState() => State;
    public CharacterState GetLastState() => LastState;

    public void SetPosition(Vector3 Position, bool KillVelocity = true)
    {
        Motor.SetPosition(Position);
        if (KillVelocity)
        {
            Motor.BaseVelocity = Vector3.zero;
        }
    }
}