using UnityEngine;
using UnityEngine.UI;

public class W_Flamethrower : T_Ranged
{
    [Header("Specific Stats")]
    [SerializeField] float TickDMG;
    [SerializeField] float TickNum;
    [SerializeField] float TickDelay;
    protected override void Awake() {
        base.Awake();
        Player = transform.GetComponentInParent<PlayerController>();
    }

    protected override bool CanFire() {
        if(CurrentAmmo <= 0) { return false; }
        if(InputManager.Instance.FireInput && CurrUnloadTime < 0) {
            return true;
        }

        return false;
    }

    protected override void Update() {
        base.Update();

        if(CanFire()) {
            SetFireStats();
        }
    }

    void SetFireStats() {
        GameObject Projectile = CheckFire(Player.AimDir);
        Projectile.GetComponent<P_Flame>().SetStats(Range, Damage, TickDMG, TickDelay, TickNum, FirePoint.position, Player.transform);
    }

    protected override void CheckReload()
    {
        base.CheckReload(); 
    }
}
