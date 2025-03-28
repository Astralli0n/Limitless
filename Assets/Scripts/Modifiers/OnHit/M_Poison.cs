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

    public override void ResetModifier()
    {
        transform.parent.GetComponent<Rigidbody>().mass = 1f;
        base.ResetModifier();
    }
}
