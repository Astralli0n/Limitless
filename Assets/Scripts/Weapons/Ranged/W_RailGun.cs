using UnityEngine;
using UnityEngine.UI;

public class W_RailGun : T_Ranged
{
    [Header("Specific Stats")]
    [SerializeField] float MaxDMG;
    [SerializeField] float RangeCutoff;
    protected override void Awake() {
        base.Awake();
        Player = transform.GetComponentInParent<PlayerController>();
    }

    protected override void Update() {
        base.Update();

        if(CanFire()) {
            SetFireStats();
        }
    }

    void SetFireStats() {
        GameObject Projectile = CheckFire(Player.AimDir);
        Projectile.GetComponent<P_RGBullet>().SetStats(Range, RangeCutoff, Damage, MaxDMG, FirePoint.position, Player.transform);
    }

    protected override void CheckReload()
    {
        base.CheckReload(); 
    }
}
