using UnityEngine;
using UnityEngine.UI;

public class W_GrenadeLauncher : T_Ranged
{
    [Header("Specific Stats")]
    [SerializeField] float ExplosionRange;
    [SerializeField] float ExplosionDelay;
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
        Projectile.GetComponent<P_Grenade>().SetStats(Range, ExplosionRange, Damage, ExplosionDelay, FirePoint.position, Player.transform);
    }

    protected override void CheckReload()
    {
        base.CheckReload(); 
    }
}
