using UnityEngine;

public class P_Flame : MonoBehaviour
{
    [SerializeField] Transform Player;
    [SerializeField] float Range;
    [SerializeField] float DMG;
    [SerializeField] float TickDMG;
    [SerializeField] float TickNum;
    [SerializeField] float TickDelay;
    [SerializeField] Vector3 InitPos;
    bool DealtDMG = false;

    void LateUpdate() {
        if(Vector3.Distance(transform.position, InitPos) > Range) {
            Debug.Log("BULLET LEFT RANGE");
            Destroy(gameObject);
        }
    }

    public void SetStats(float _Range, float _DMG, float _TickDMG, float Delay, float Num, Vector3 Pos, Transform Origin) {
        Range = _Range;
        DMG = _DMG;
        InitPos = Pos;
        Player = Origin;

        TickDMG = _TickDMG;
        TickDelay = Delay;
        TickNum = Num;

        Debug.Log($"Bullet initialised w/ {TickDMG}");
    }

    void OnTriggerEnter(Collider Other)
    {
        var OtherHealth = Other.transform.GetComponent<Health>();
        if(DealtDMG || Other.transform == Player) { return; }
        if(OtherHealth != null) {
            OtherHealth.TakeDamage(DMG);
            OtherHealth.ApplyStatusEffect("FIRE", TickNum, TickDelay, TickDMG, Time.time);
            DealtDMG = true;
        }
        Destroy(gameObject);
    }
}
