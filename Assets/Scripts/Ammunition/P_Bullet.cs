using UnityEngine;

public class P_Bullet : MonoBehaviour
{
    [SerializeField] Transform Player;
    [SerializeField] float Range;
    [SerializeField] float DMG;
    [SerializeField] Vector3 InitPos;
    bool DealtDMG = false;

    void LateUpdate() {
        if(Vector3.Distance(transform.position, InitPos) > Range) {
            Debug.Log("BULLET LEFT RANGE");
            Destroy(gameObject);
        }
    }

    public void SetStats(float _Range, float _DMG, Vector3 Pos, Transform Origin) {
        Range = _Range;
        DMG = _DMG;
        InitPos = Pos;
        Player = Origin;
        Debug.Log($"Bullet Initialized: Range={Range}, InitPos={InitPos}, Speed={GetComponent<Rigidbody>().linearVelocity.magnitude}");
    }

    void OnCollisionEnter(Collision Other)
    {
        var OtherHealth = Other.transform.GetComponent<Health>();
        if(DealtDMG || Other.transform == transform) { return; }
        if(OtherHealth != null) {
            OtherHealth.TakeDamage(DMG);
            DealtDMG = true;
        }
        Destroy(gameObject);
    }
}
