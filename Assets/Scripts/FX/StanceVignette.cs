using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StanceVignette : MonoBehaviour
{
    [SerializeField] float WalkVignetteIntensity;
    [SerializeField] float SprintVignetteIntensity;
    [SerializeField] float CrouchVignetteIntensity;
    [SerializeField] float SlideVignetteIntensity;
    [SerializeField] float VignetteResponse;
    VolumeProfile Profile;
    Vignette _Vignette;
    public void Initialise(VolumeProfile _Profile)
    {
        Profile = _Profile;

        if (!Profile.TryGet(out _Vignette))
        {
            _Vignette = Profile.Add<Vignette>();
            return;
        }
        _Vignette.active = true;
        _Vignette.intensity.overrideState = true;
        _Vignette.intensity.Override(WalkVignetteIntensity);
    }

    public void UpdateVignette(float DeltaTime, Stance CurrentStance)
    {
        var TargetVignetteIntensity = CurrentStance switch
        {
            Stance.Stand => WalkVignetteIntensity,
            Stance.Sprint => SprintVignetteIntensity,
            Stance.Crouch => CrouchVignetteIntensity,
            Stance.Slide => SlideVignetteIntensity,
            _ => 0f
        };

        _Vignette.intensity.value = Mathf.Lerp(
            a: _Vignette.intensity.value,
            b: TargetVignetteIntensity,
            t: VignetteResponse * DeltaTime
        );
    }
}
