using UnityEngine;

public class P_Bullet : MonoBehaviour
{
    [SerializeField] Transform Player;
    [SerializeField] float Range;
    [SerializeField] float RangeCutoff;
    [SerializeField] float MinDMG;
    [SerializeField] float MaxDMG;
    [SerializeField] Vector3 InitPos;
    bool DealtDMG = false;

    void LateUpdate() {
        if(Vector3.Distance(transform.position, InitPos) >= Range) {
            Destroy(gameObject);
        }
    }

    public void SetStats(float _Range, float Cutoff, float Min, float Max, Vector3 Pos, Transform Origin) {
        Range = _Range;
        RangeCutoff = Cutoff;
        MinDMG = Min;
        MaxDMG = Max;
        InitPos = Pos;
        Player = Origin;
    }

    void OnTriggerEnter(Collider Other)
    {
        var OtherHealth = Other.transform.GetComponent<Health>();
        if(DealtDMG || Other.transform == Player) { return; }

        if(OtherHealth != null) {
            float DistMult = Mathf.Min(RangeCutoff, Vector3.Distance(InitPos, transform.position)) / RangeCutoff;
            float DMG = DistMult * (MaxDMG - MinDMG) + MinDMG;

            Weapon.InvokeOnAnyHit(OtherHealth, Player.gameObject, DMG, false);
            OtherHealth.TakeDamage(DMG);
            DealtDMG = true;
        } else {
            Destroy(gameObject);
        }
    }
}
