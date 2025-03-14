using UnityEngine;

public class W_Longbow : T_Ranged
{
    [Header("Specific Stats")]
    [SerializeField] float MaxRange;
    T_Chargeable ChargeComponent;

    void Awake() {
        ChargeComponent = GetComponent<T_Chargeable>();
        Player = transform.parent.GetComponent<PlayerController>();
    }

    protected override void Update() {
        base.Update();

        if(CanFire()) {
            SetFireStats();
            ChargeComponent.StopCharging();
        }
    }

    void SetFireStats() {
        var HoldDuration = ChargeComponent.GetChargeRatio();
        var FireRange = (HoldDuration * (MaxRange - Range)) + Range;

        GameObject Projectile = CheckFire(Player.AimDir);
    }
}
