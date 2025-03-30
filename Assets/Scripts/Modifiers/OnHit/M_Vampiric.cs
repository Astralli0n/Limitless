using UnityEngine;

public class M_Vampiric : T_OnHit
{
    [SerializeField] float LifeStealPercentage;
    Health PlayerHP;

    void Start()
    {
        PlayerHP = Player.GetComponent<Health>();
    }

    protected override void HandleOnHit(Health EnemyHealth, GameObject Attacker, float Damage, bool IsTick) {
        if (EnemyHealth == null) return;
        if (Attacker == Player) {
            PlayerHP.TakeDamage(-Damage * LifeStealPercentage);
        }
    }

}
