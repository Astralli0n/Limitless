using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

public class CameraMotionEffects : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] float DivergenceAngle;

    [Space]

    [Header("Motion Settings")]
    [SerializeField] Vector2 WalkFOVMotionRange;
    [SerializeField] Vector2 SprintFOVMotionRange;
    [SerializeField] Vector2 CrouchFOVMotionRange;
    [SerializeField] Vector2 SlideFOVMotionRange;

    [Space]

    [Header("FOV Settings")]
    [SerializeField] Vector2 WalkFOVRange;
    [SerializeField] Vector2 SprintFOVRange;
    [SerializeField] Vector2 CrouchFOVRange;
    [SerializeField] Vector2 SlideFOVRange;
    [SerializeField] float FOVDamping;
    float ReferenceFOVVelocity;
    Camera Cam;

    [Space]

    [Header("Aberration Settings")]
    [SerializeField] Vector2 AberrationMotionRange;
    [SerializeField] Vector2 AberrationAccelerationRange;
    [SerializeField] Vector2 AberrationFluctuationRange;
    [SerializeField][Range(0f, 1f)] float AberrationVariance;
    [SerializeField] float AberrationDamping;
    VolumeProfile Profile;
    ChromaticAberration Aberration;
    float AberrationVelocity;
    float AberrationSineTime;

    [Space]

    [Header("Speed Lines")]
    [SerializeField] Vector2 SprintSpeedLineMotionRange;
    [SerializeField] Vector2 SprintSpawnRateRange;
    [SerializeField] Vector2 SlideSpeedLineMotionRange;
    [SerializeField] Vector2 SlideSpawnRateRange;
    [SerializeField] float SpeedLineDamping;
    VisualEffect SpeedLineEffect;

    public void Initialise(Camera _Camera, VolumeProfile _Profile, VisualEffect Effect)
    {
        Cam = _Camera;
        Cam.fieldOfView = WalkFOVRange.x;

        Profile = _Profile;

        if (!Profile.TryGet(out Aberration))
        {
            Aberration = Profile.Add<ChromaticAberration>();
            return;
        }
        Aberration.active = true;
        Aberration.intensity.overrideState = true;

        SpeedLineEffect = Effect;
    }

    public void UpdateFOV(float DeltaTime, Vector2 Velocity, Vector2 PreviousVelocity, Stance State)
    {
        Vector2 FOVRange = State switch
        {
            Stance.Stand => WalkFOVRange,
            Stance.Sprint => SprintFOVRange,
            Stance.Crouch => CrouchFOVRange,
            Stance.Slide => SlideFOVRange,
            _ => WalkFOVRange
        };

        Vector2 MotionRange = State switch
        {
            Stance.Stand => WalkFOVMotionRange,
            Stance.Sprint => SprintFOVMotionRange,
            Stance.Crouch => CrouchFOVMotionRange,
            Stance.Slide => SlideFOVMotionRange,
            _ => WalkFOVMotionRange
        };

        float Dot = Vector2.Dot(Velocity.normalized, PreviousVelocity.normalized);

        if (Dot > Mathf.Cos(DivergenceAngle * Mathf.Deg2Rad))
        {
            float TargetFOV = Mathf.Lerp(FOVRange.x, FOVRange.y, Mathf.InverseLerp(MotionRange.x, MotionRange.y, Velocity.magnitude));
            Cam.fieldOfView = Mathf.SmoothDamp(Cam.fieldOfView, TargetFOV, ref ReferenceFOVVelocity, FOVDamping, Mathf.Infinity, DeltaTime);
        }
        else
        {
            Cam.fieldOfView = Mathf.SmoothDamp(Cam.fieldOfView, FOVRange.x, ref ReferenceFOVVelocity, FOVDamping, Mathf.Infinity, DeltaTime);
        }
    }

    public void UpdateAberration(float DeltaTime, float Speed, float Acceleration, Stance State)
    {
        float BaseIntensity = Mathf.InverseLerp(
            AberrationMotionRange.x,
            AberrationMotionRange.y,
            Speed
        );

        float FluctuationFrequency = Mathf.Lerp(
            a: AberrationFluctuationRange.x,
            b: AberrationFluctuationRange.y,
            t: Mathf.InverseLerp(AberrationAccelerationRange.x, AberrationAccelerationRange.y, Acceleration)
        );

        AberrationSineTime += DeltaTime * FluctuationFrequency * Mathf.PI * 2f;

        float Fluctuation = Mathf.Sin(AberrationSineTime) * AberrationVariance;

        float TargetAberration = Mathf.Clamp01(BaseIntensity + Fluctuation);

        Aberration.intensity.value = Mathf.SmoothDamp(
            Aberration.intensity.value,
            TargetAberration,
            ref AberrationVelocity,
            AberrationDamping,
            Mathf.Infinity,
            DeltaTime
        );
    }

    public void UpdateSpeedLines(float DeltaTime, float Speed, Stance State)
    {
        Vector2 TargetMotionRange = State switch
        {
            Stance.Sprint => SprintSpeedLineMotionRange,
            Stance.Slide => SlideSpeedLineMotionRange,
            _ => Vector2.zero
        };

        Vector2 TargetSpawnRateRange = State switch
        {
            Stance.Sprint => SprintSpawnRateRange,
            Stance.Slide => SlideSpawnRateRange,
            _ => Vector2.zero
        };

        float TargetMotion = Mathf.InverseLerp(TargetMotionRange.x, TargetMotionRange.y, Speed);
        float TargetSpawnRate = Mathf.Lerp(TargetSpawnRateRange.x, TargetSpawnRateRange.y, TargetMotion);

        float SmoothedSpawnRate = Mathf.SmoothDamp(
            SpeedLineEffect.GetFloat("SpawnRate"),
            TargetSpawnRate,
            ref TargetSpawnRate,
            SpeedLineDamping,
            Mathf.Infinity,
            DeltaTime
        );
        SpeedLineEffect.SetFloat("SpawnRate", SmoothedSpawnRate);
    }
    
}
