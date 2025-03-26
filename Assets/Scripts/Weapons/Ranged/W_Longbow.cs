using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(T_Chargeable))]
public class W_Longbow : T_Ranged
{
    [Header("Specific Stats")]
    [SerializeField] float MaxRange;
    [SerializeField] float MaxDMG;
    T_Chargeable ChargeComponent;

    protected override void Awake() {
        base.Awake();
        ChargeComponent = GetComponent<T_Chargeable>();
        Player = transform.GetComponentInParent<PlayerController>();
    }

    protected override void Update() {
        if(!IsActive) {
            return;
        } else {
            ChargeBar.enabled = true;
        }
        
        base.Update();

        if (InputManager.Instance.FireInputPress && CurrentAmmo > 0 && IsActive) {
            ChargeComponent.StartCharging();
        }

        if (ChargeComponent.IsCharging && CurrentAmmo > 0) {
            ChargeComponent.UpdateCharging();
            ChargeBar.fillAmount = ChargeComponent.GetChargeRatio() / 2f;
        } else {
            ChargeBar.fillAmount = 0f;
        }

        if(CanFire()) {
            SetFireStats();
            ChargeComponent.StopCharging();
        }
    }

    void SetFireStats() {
        var HoldDuration = ChargeComponent.GetChargeRatio();
        var FireRange = (HoldDuration * (MaxRange - Range)) + Range;
        var FireDMG = (HoldDuration * (MaxDMG - Damage)) + Damage;

        GameObject Projectile = CheckFire(Player.AimDir);
        Projectile.GetComponent<P_Arrow>().SetStats(FireRange, FireDMG, FirePoint.position, Player.transform);
    }
}
