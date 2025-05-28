using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraMotionFX : MonoBehaviour
{
    [SerializeField] Vector2 WalkFOVRange;
    [SerializeField] Vector2 WalkAberrationRange;
    [SerializeField] Vector2 WalkSpeedRange;

    [Space]

    [SerializeField] Vector2 SprintFOVRange;
    [SerializeField] Vector2 SprintAberrationRange;
    [SerializeField] Vector2 SprintSpeedRange;

    [Space]

    [SerializeField] Vector2 CrouchFOVRange;
    [SerializeField] Vector2 CrouchAberrationRange;
    [SerializeField] Vector2 CrouchSpeedRange;

    [Space]

    [SerializeField] Vector2 SlideFOVRange;
    [SerializeField] Vector2 SlideAberrationRange;
    [SerializeField] Vector2 SlideSpeedRange;

    [Space]

    [SerializeField] float FOVDamping;
    [SerializeField] float AberrationDamping;
    Camera _Camera;
    VolumeProfile Profile;
    ChromaticAberration Aberration;
    float ReferenceFOV;
    float ReferenceAberration;

    public void Initialise(Camera Camera, VolumeProfile _Profile)
    {
        _Camera = Camera;

        Profile = _Profile;

        if (!Profile.TryGet(out Aberration))
        {
            Aberration = Profile.Add<ChromaticAberration>();
            return;
        }

        Aberration.active = true;
        Aberration.intensity.overrideState = true;
        Aberration.intensity.Override(WalkAberrationRange.x);

        _Camera.fieldOfView = WalkFOVRange.x;
    }

    public void UpdateFOV(float DeltaTime, float Speed, Stance CurrentStance)
    {
        var TargetRange = CurrentStance switch
        {
            Stance.Stand => WalkFOVRange,
            Stance.Sprint => SprintFOVRange,
            Stance.Crouch => CrouchFOVRange,
            Stance.Slide => SlideFOVRange,
            _ => Vector2.zero
        };

        var TargetSpeedRange = CurrentStance switch
        {
            Stance.Stand => WalkSpeedRange,
            Stance.Sprint => SprintSpeedRange,
            Stance.Crouch => CrouchSpeedRange,
            Stance.Slide => SlideSpeedRange,
            _ => Vector2.zero
        };

        var TargetFOV = Mathf.Lerp(
            a: TargetRange.x,
            b: TargetRange.y,
            t: Mathf.InverseLerp(
                a: TargetSpeedRange.x,
                b: TargetSpeedRange.y,
                value: Speed
            )
        );

        _Camera.fieldOfView = Mathf.SmoothDamp(
            current: _Camera.fieldOfView,
            target: TargetFOV,
            currentVelocity: ref ReferenceFOV,
            smoothTime: FOVDamping,
            maxSpeed: Mathf.Infinity,
            deltaTime: DeltaTime
        );
    }

    public void UpdateAberration(float DeltaTime, float Speed, Stance CurrentStance)
    {
        var TargetRange = CurrentStance switch
        {
            Stance.Stand => WalkAberrationRange,
            Stance.Sprint => SprintAberrationRange,
            Stance.Crouch => CrouchAberrationRange,
            Stance.Slide => SlideAberrationRange,
            _ => Vector2.zero
        };

        var TargetSpeedRange = CurrentStance switch
        {
            Stance.Stand => WalkSpeedRange,
            Stance.Sprint => SprintSpeedRange,
            Stance.Crouch => CrouchSpeedRange,
            Stance.Slide => SlideSpeedRange,
            _ => Vector2.zero
        };

        var TargetAberration = Mathf.Lerp(
            a: TargetRange.x,
            b: TargetRange.y,
            t: Mathf.InverseLerp(
                a: TargetSpeedRange.x,
                b: TargetSpeedRange.y,
                value: Speed
            )
        );

        Aberration.intensity.value = Mathf.SmoothDamp(
            current: Aberration.intensity.value,
            target: TargetAberration,
            currentVelocity: ref ReferenceAberration,
            smoothTime: AberrationDamping,
            maxSpeed: Mathf.Infinity,
            deltaTime: DeltaTime
        );
    }
}
