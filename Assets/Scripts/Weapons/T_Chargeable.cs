using UnityEngine;
using UnityEngine.UI;

public class T_Chargeable : MonoBehaviour
{
    [Header("Charging Settings")]
    public bool UsingCharge = true;
    public float MaxChargeTime = 1f;
    protected float CurrentChargeTime = 0f;
    public bool IsCharging { get; private set; }

    public void StartCharging() {
        if(UsingCharge) {
            IsCharging = true;
            CurrentChargeTime = 0f;
        }
    }

    public void UpdateCharging() {
        if(IsCharging) {
            CurrentChargeTime = Mathf.Min(CurrentChargeTime + Time.deltaTime, MaxChargeTime);
        }
    }

    public float StopCharging() {
        IsCharging = false;
        float ChargeRatio = GetChargeRatio();
        CurrentChargeTime = 0f;
        return ChargeRatio;
    }

    public float GetChargeRatio() {
        return CurrentChargeTime / MaxChargeTime;
    }
}
