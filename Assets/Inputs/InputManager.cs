using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public Vector2 MoveInput;
    public Vector2 ViewInput;
    public bool JumpInput;
    public bool JumpInputPress = false;
    public bool JumpInputRelease = false;
    public bool SprintInput;
    public bool CrouchInput;
    public bool FireInput;
    public bool FireInputPress = false;
    public bool FireInputRelease = false;
    public bool ReloadInputPress;
    public bool PreviousWeaponInputPress = false;
    public bool NextWeaponInputPress = false;

    void Awake() {
        if(Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    void LateUpdate() {
        JumpInputPress = false;
        JumpInputRelease = false;

        FireInputPress = false;
        FireInputRelease = false;

        ReloadInputPress = false;

        PreviousWeaponInputPress = false;
        NextWeaponInputPress = false;
    }

    public void SetMove(InputAction.CallbackContext CTX) {
        MoveInput = CTX.ReadValue<Vector2>();
    }

    public void SetView(InputAction.CallbackContext CTX) {
        ViewInput = CTX.ReadValue<Vector2>();
    }

    public void SetJump(InputAction.CallbackContext CTX) {
        JumpInput = CTX.ReadValue<float>() > 0f;
        
        if (CTX.phase == InputActionPhase.Performed) {
            JumpInputPress = true;
        }

        else if (CTX.phase == InputActionPhase.Canceled) {
            JumpInputRelease = true;
        }
    }

    public void SetFire(InputAction.CallbackContext CTX) {
        FireInput = CTX.ReadValue<float>() > 0f;
        
        if (CTX.phase == InputActionPhase.Performed) {
            FireInputPress = true;
        }

        else if (CTX.phase == InputActionPhase.Canceled) {
            FireInputRelease = true;
        }
    }

    public void SetSprint(InputAction.CallbackContext CTX) {
        SprintInput = CTX.ReadValue<float>() > 0f;
    }

    public void SetCrouch(InputAction.CallbackContext CTX) {
        CrouchInput = CTX.ReadValue<float>() > 0f;
    }

    public void SetReload(InputAction.CallbackContext CTX) {
        if (CTX.phase == InputActionPhase.Performed) {
            ReloadInputPress = true;
        }
    }

    public void SetPrevious(InputAction.CallbackContext CTX) {
        if (CTX.phase == InputActionPhase.Performed) {
            PreviousWeaponInputPress = true;
        }
    }

    public void SetNext(InputAction.CallbackContext CTX) {
        if (CTX.phase == InputActionPhase.Performed) {
            NextWeaponInputPress = true;
        }
    }
}
