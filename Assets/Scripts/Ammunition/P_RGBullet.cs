using UnityEngine;

public class P_RGBullet : MonoBehaviour
{
    [SerializeField] Transform Player;
    [SerializeField] float Range;
    [SerializeField] float RangeCutoff;
    [SerializeField] float MinDMG;
    [SerializeField] float MaxDMG;
    [SerializeField] Vector3 InitPos;
    bool DealtDMG = false;

    void LateUpdate() {
        if(Vector3.Distance(transform.position, InitPos) > Range) {
            Debug.Log("BULLET LEFT RANGE");
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
        Debug.Log($"Bullet Initialized: Range={Range}, InitPos={InitPos}, Speed={GetComponent<Rigidbody>().linearVelocity.magnitude}");
    }

    void OnCollisionEnter(Collision Other)
    {
        var OtherHealth = Other.transform.GetComponent<Health>();
        if(DealtDMG || Other.transform == Player) { return; }

        if(OtherHealth != null) {
            float DistMult = Mathf.Min(RangeCutoff, Vector3.Distance(InitPos, transform.position)) / RangeCutoff;
            float DMG = DistMult * (MaxDMG - MinDMG) + MinDMG;

            OtherHealth.TakeDamage(DMG);
            DealtDMG = true;
        }
        Destroy(gameObject);
    }
}
