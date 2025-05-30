using UnityEngine;

public class CameraLean : MonoBehaviour
{
    [SerializeField] float AttackDamping;
    [SerializeField] float DecayDamping;
    [SerializeField] float WalkStrength;
    [SerializeField] float SprintStrength;
    [SerializeField] float CrouchStrength;
    [SerializeField] float SlideStrength;
    [SerializeField] float StrengthResponse;
    Vector3 DampedAcceleration;
    Vector3 DampedAccelerationVelocity;
    float SmoothedStrength;
    public void Initialise()
    {
        SmoothedStrength = WalkStrength;
    }

    public void UpdateLean(float DeltaTime, Vector3 Acceleration, Stance PlayerStance, Vector3 Up)
    {
        var PlanarAcceleration = Vector3.ProjectOnPlane(Acceleration, Up);
        var Damping = PlanarAcceleration.sqrMagnitude > DampedAcceleration.sqrMagnitude ? AttackDamping : DecayDamping;

        DampedAcceleration = Vector3.SmoothDamp
        (
            current: DampedAcceleration,
            target: PlanarAcceleration,
            currentVelocity: ref DampedAccelerationVelocity,
            smoothTime: Damping,
            maxSpeed: Mathf.Infinity,
            deltaTime: DeltaTime
        );

        var LeanAxis = Vector3.Cross(DampedAcceleration.normalized, Up).normalized;

        var TargetStrength = PlayerStance switch
        {
            Stance.Stand => WalkStrength,
            Stance.Sprint => SprintStrength,
            Stance.Crouch => CrouchStrength,
            Stance.Slide => SlideStrength,
            _ => 0f
        };

        SmoothedStrength = Mathf.Lerp(SmoothedStrength, TargetStrength, 1f - Mathf.Exp(-StrengthResponse * DeltaTime));

        transform.localRotation = Quaternion.identity;
        transform.rotation = Quaternion.AngleAxis(-DampedAcceleration.magnitude * SmoothedStrength, LeanAxis) * transform.rotation;
    }
}
