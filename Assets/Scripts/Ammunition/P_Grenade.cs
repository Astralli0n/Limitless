using System.Linq;
using UnityEngine;

public class P_Grenade : MonoBehaviour
{
    [SerializeField] Transform Player;
    [SerializeField] float Range;
    [SerializeField] float ExplosionRange;
    [SerializeField] float Damage;
    [SerializeField] float Delay;
    [SerializeField] Vector3 InitPos;
    bool DealtDMG = false;
    private Transform AttachedParent;
    private Vector3 PositionOffset;

    void LateUpdate() {
        if (AttachedParent != null) {
            transform.position = AttachedParent.position + PositionOffset;
        }

        // Range check
        if (Vector3.Distance(transform.position, InitPos) > Range) {
            Debug.Log("BULLET LEFT RANGE");
            Invoke("Explode", Delay);
        }
    }

    public void SetStats(float _Range, float _ExplosionRange, float DMG, float _Delay, Vector3 Pos, Transform Origin) {
        Range = _Range;
        ExplosionRange = _ExplosionRange;
        Damage = DMG;
        Delay = _Delay;
        InitPos = Pos;
        Player = Origin;
        Debug.Log($"Bullet Initialized: Range={Range}, InitPos={InitPos}, Speed={GetComponent<Rigidbody>().linearVelocity.magnitude}");
    }

    void OnCollisionEnter(Collision Other) {
        if (Other.transform != Player) { 
            AttachedParent = Other.transform;
            PositionOffset = transform.position - AttachedParent.position;
        }

        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<SphereCollider>().enabled = false;
        Invoke("Explode", Delay);
    }

    void Explode() {
        AttachedParent = null;

        transform.localScale = Vector3.one * ExplosionRange;

        if (!DealtDMG) {
            var ObjectsInRange = Physics.OverlapSphere(transform.position, ExplosionRange)
                                        .Where(Obj => Obj.transform != transform)
                                        .Where(Obj => Obj.GetComponent<Health>() != null);

            if (ObjectsInRange.Count() > 0) {
                foreach (var Obj in ObjectsInRange) {
                    Obj.GetComponent<Health>().TakeDamage(Damage);
                }
            }
        }

        DealtDMG = true;
        Destroy(gameObject, 0.5f);
    }
}