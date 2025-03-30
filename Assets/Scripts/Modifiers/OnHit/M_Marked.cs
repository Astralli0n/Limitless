using UnityEngine;

public class M_Marked : T_OnHit
{
    [SerializeField] float DamageMultiplier;

    protected override void HandleOnHit(Health EnemyHealth, GameObject _Player, float Damage, bool IsTick)
    {
        if(Player == _Player && EnemyHealth.HasStatusEffect(EffectType)) {
            EnemyHealth.TakeDamage(Damage * (DamageMultiplier - 1f));
        }
        base.HandleOnHit(EnemyHealth, Player, Damage, IsTick);
    }

    public override void ResetModifier() {
        Debug.Log("Resetting Marked Modifier");
        base.ResetModifier();
    }
}
