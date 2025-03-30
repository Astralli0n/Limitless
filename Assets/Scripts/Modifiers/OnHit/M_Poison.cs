using UnityEngine;

public class M_Poison : T_OnHit
{
    [SerializeField] float SlowdownPercentage = 5f;

    void Update()
    {
        if(OnEnemy) {
            transform.parent.GetComponent<Rigidbody>().mass = 1 + SlowdownPercentage;
        }
    }

    protected override void HandleOnHit(Health EnemyHealth, GameObject Player, float Damage, bool IsTick)
    {
        Debug.Log(IsTick);
        if(!IsTick) {
            base.HandleOnHit(EnemyHealth, Player, Damage, IsTick);
        }
    }
    public override void ResetModifier()
    {
        transform.parent.GetComponent<Rigidbody>().mass = 1f;
        base.ResetModifier();
    }
}
