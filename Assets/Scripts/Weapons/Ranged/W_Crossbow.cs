using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(T_Chargeable))]
public class W_Crossbow : T_Ranged
{
    T_Chargeable ChargeComponent;
    bool ChargeShot;

    protected override void Awake() {
        base.Awake();
        ChargeComponent = GetComponent<T_Chargeable>();
        Player = transform.GetComponentInParent<PlayerController>();
        ChargeBar.enabled = true;
    }

    protected override void Update() {
        base.Update();

        if (InputManager.Instance.FireInputPress && CurrentAmmo > 0) {
            ChargeComponent.StartCharging();
        }

        if(!ChargeShot) {
            if (ChargeComponent.IsCharging && CurrentAmmo > 0) {
                ChargeComponent.UpdateCharging();
                ChargeBar.fillAmount = ChargeComponent.GetChargeRatio() / 2f;
            } else {
                ChargeBar.fillAmount = 0f;
            }
        }

        if(CanFire()) {
            if(!ChargeShot) {
                if(ChargeComponent.GetChargeRatio() == 1) {
                    ChargeShot = true;
                }
            } else {
                SetFireStats();
            }

            ChargeComponent.StopCharging();
        }
    }

    void SetFireStats() {
        GameObject Projectile = CheckFire(Player.AimDir);
        Projectile.GetComponent<P_Arrow>().SetStats(Range, Damage, FirePoint.position, Player.transform);

        ChargeShot = false;
    }

    protected override void CheckReload()
    {
        if(ChargeShot) { IsReloading = false; return; }
        base.CheckReload(); 
    }
}
