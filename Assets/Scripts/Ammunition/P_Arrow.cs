using UnityEngine;

public class P_Arrow : MonoBehaviour
{
    [SerializeField] Transform Player;
    [SerializeField] float Range;
    [SerializeField] float DMG;
    [SerializeField] Vector3 InitPos;
    bool DealtDMG = false;

    void LateUpdate() {
        if(Vector3.Distance(transform.position, InitPos) >= Range) {
            Destroy(gameObject);
        }
    }

    public void SetStats(float _Range, float _DMG, Vector3 Pos, Transform Origin) {
        Range = _Range;
        DMG = _DMG;
        InitPos = Pos;
        Player = Origin;
    }

    void OnTriggerEnter(Collider Other)
    {
        var OtherHealth = Other.transform.GetComponent<Health>();
        if(DealtDMG || Other.transform == Player) { return; }
        if(OtherHealth != null) {
            Weapon.InvokeOnAnyHit(OtherHealth, Player.gameObject, DMG, false);
            OtherHealth.TakeDamage(DMG);
            DealtDMG = true;
        }
        Destroy(gameObject);
    }
}
