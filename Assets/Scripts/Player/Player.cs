using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    [SerializeField] PlayerCharacter Character;
    [SerializeField] PlayerCamera Camera;

    [SerializeField] CameraSpring CamSpring;
    [SerializeField] CameraLean CamLean;
    [SerializeField] CameraMotionFX CamMotionFX;
    [SerializeField] Volume _Volume;
    [SerializeField] StanceVignette _StanceVignette;

    PlayerControls Controls;

    void Awake()
    {
        Character = GetComponentInChildren<PlayerCharacter>();
        Camera = GetComponentInChildren<PlayerCamera>();   
    }

    void Start()
    {
        Controls = new PlayerControls();
        Controls.Enable();

        Character.Initialise();
        Camera.Initialise(Character.GetCameraTarget());

        CamSpring.Initialise();
        CamLean.Initialise();
        CamMotionFX.Initialise(Camera.GetComponentInChildren<Camera>(), _Volume.profile);

        _StanceVignette.Initialise(_Volume.profile);

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        var DeltaTime = Time.deltaTime;
        var Input = Controls.Player;
        var CameraInput = new CameraInput{ LookInput = Input.Look.ReadValue<Vector2>() };
        Camera.UpdateRotation(CameraInput);

        var _CharacterInput = new CharacterInput
        {
            Rotation = Quaternion.Euler(Camera.transform.eulerAngles),
            Move = Input.Move.ReadValue<Vector2>(),
            Jump = Input.Jump.triggered,
            JumpSustain = Input.Jump.IsPressed(),
            Crouch = (Character.CrouchInputType == InputType.Toggle && Input.Crouch.triggered) ? InputType.Toggle :
                     (Character.CrouchInputType == InputType.Hold && Input.Crouch.IsPressed()) ? InputType.Hold : InputType.None,
                     
            Sprint = (Character.SprintInputType == InputType.Toggle && Input.Sprint.triggered) ? InputType.Toggle :
                     (Character.SprintInputType == InputType.Hold && Input.Sprint.IsPressed()) ? InputType.Hold : InputType.None,
        };
        Character.UpdateInput(_CharacterInput);
        Character.UpdateBody(DeltaTime);

        #if UNITY_EDITOR
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            var Ray = new Ray(Camera.transform.position, Camera.transform.forward);
            if(Physics.Raycast(Ray, out var Hit))
            {
                Teleport(Hit.point);
            }
        }
        #endif
    }

    void LateUpdate()
    {
        var DeltaTime = Time.deltaTime;
        var CameraTarget = Character.GetCameraTarget();
        var State = Character.GetState();

        Camera.UpdatePosition(CameraTarget);

        CamSpring.UpdateSpring(DeltaTime, CameraTarget.up);
        CamLean.UpdateLean(DeltaTime, State.Acceleration, State.Stance, CameraTarget.up);

        CamMotionFX.UpdateFOV(DeltaTime, State.Velocity.magnitude, State.Stance);
        CamMotionFX.UpdateAberration(DeltaTime, State.Velocity.magnitude, State.Stance);

        _StanceVignette.UpdateVignette(DeltaTime, State.Stance);
    }

    public void Teleport(Vector3 Position)
    {
        Character.SetPosition(Position);
    }

    void OnDestroy()
    {
        Controls.Dispose();
    }
}
