using UnityEngine;

public class M_Blast : T_OnHit
{
    [SerializeField] GameObject GrenadePrefab;
    [SerializeField] float ExplosionRange;
    [SerializeField] float Damage;

    public override void ResetModifier()
    {
        Debug.Log(GetComponentInParent<Health>().GetHealth());
        if(GetComponentInParent<Health>().GetHealth() <= 0f) {
            Debug.Log("Spawning Grenade");
            var Grenade = Instantiate(GrenadePrefab, transform.position, Quaternion.identity);

            Grenade.GetComponent<P_Grenade>().SetStats(0f, ExplosionRange, Damage, 0.1f, 0f, transform.position, transform);
        }
        
        base.ResetModifier();
    }
}
