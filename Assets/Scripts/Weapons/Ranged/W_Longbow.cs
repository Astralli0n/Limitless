using UnityEngine;

[RequireComponent(typeof(T_Chargeable))]
public class W_Longbow : T_Ranged
{
    [Header("Specific Stats")]
    [SerializeField] float MaxRange;
    T_Chargeable ChargeComponent;

    protected override void Awake() {
        base.Awake();
        ChargeComponent = GetComponent<T_Chargeable>();
        Player = transform.parent.parent.GetComponent<PlayerController>();
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
        Projectile.GetComponent<P_Bullet>().SetStats(FireRange, Damage, FirePoint.position, transform.parent.parent);
    }
}
