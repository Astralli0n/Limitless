using UnityEngine;

public struct CameraInput
{
    public Vector2 LookInput;
}

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] Vector2 MouseSensitivity;
    [SerializeField] float MouseResponse;
    [SerializeField] float MinLookAngle;
    [SerializeField] float MaxLookAngle;
    Vector3 EulerAngles;
    public void Initialise(Transform Target)
    {
        transform.position = Target.position;
        transform.rotation = Target.rotation;
        transform.eulerAngles = EulerAngles = Target.eulerAngles;
    }

    public void UpdatePosition(Transform Target) 
    {
        transform.position = Target.position;
    }

    public void UpdateRotation(CameraInput Input) 
    {
        EulerAngles += new Vector3(Input.LookInput.y * -MouseSensitivity.y, Input.LookInput.x * MouseSensitivity.x);
        EulerAngles.x = Mathf.Clamp(EulerAngles.x, MinLookAngle, MaxLookAngle);
        transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, EulerAngles, MouseResponse);
    }
}
