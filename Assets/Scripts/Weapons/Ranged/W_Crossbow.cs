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
    }

    protected override void Update() {
        base.Update();

        if(!IsActive) {
            return;
        } else {
            ChargeBar.enabled = true;

            if(ChargeShot) { ChargeBar.fillAmount = 0.5f;}
        }
        
        if (InputManager.Instance.FireInputPress && CurrentAmmo > 0 && IsActive) {
            ChargeComponent.StartCharging();
        }

        if (!ChargeShot) {
            if (ChargeComponent.IsCharging && CurrentAmmo > 0) {
                ChargeComponent.UpdateCharging();
                ChargeBar.fillAmount = ChargeComponent.GetChargeRatio() / 2f;
            } else {
                ChargeBar.fillAmount = 0f;
            }
        }
        
        // Process firing logic before base update
        if (CanFire()) {
            if (!ChargeShot) {
                if (ChargeComponent.GetChargeRatio() == 1) {
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
