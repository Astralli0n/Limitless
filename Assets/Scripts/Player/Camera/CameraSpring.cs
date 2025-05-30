using Unity.VisualScripting;
using UnityEngine;

public class CameraSpring : MonoBehaviour
{
    [Min(0.01f)] [SerializeField] float HalfLife;
    [SerializeField] float Frequency;
    [SerializeField] float AngularDisplacement;
    [SerializeField] float LinearDisplacement;

    Vector3 SpringPosition;
    Vector3 SpringVelocity;

    public void Initialise()
    {
        SpringPosition = transform.position;
        SpringVelocity = Vector3.zero;
    }

    public void UpdateSpring(float DeltaTime, Vector3 Up)
    {
        transform.localPosition = Vector3.zero;

        Spring(ref SpringPosition, ref SpringVelocity, transform.position, HalfLife, Frequency, DeltaTime);

        var RelativeSpringPosition = SpringPosition - transform.position;
        var SpringHeight = Vector3.Dot(RelativeSpringPosition, Up);

        transform.localEulerAngles = new Vector3(-SpringHeight * AngularDisplacement, 0f, 0f);
        transform.localPosition += RelativeSpringPosition.normalized * LinearDisplacement;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, SpringPosition);
        Gizmos.DrawWireSphere(SpringPosition, 0.1f);
    }

    static void Spring(ref Vector3 Current, ref Vector3 Velocity, Vector3 Target, float HalfLife, float Frequency, float TimeStep)
    {
        if ((Target - Current).sqrMagnitude < 0.001f)
        {
            Velocity = Vector3.zero;
            return;
        }
        var DampingRatio = -Mathf.Log(0.5f) / (Frequency * HalfLife);
        var F = 1.0f + 2.0f * DampingRatio * Frequency * TimeStep;
        var OO = Frequency * Frequency;
        var HOO = OO * TimeStep;
        var HHOO = TimeStep * HOO;
        var DetInv = 1.0f / (F + HHOO);
        var DetX = F * Current + TimeStep * Velocity + HHOO * Target;
        var DetV = Velocity + HOO * (Target - Current);
        Current = DetInv * DetX;
        Velocity = DetInv * DetV;
    }
}
