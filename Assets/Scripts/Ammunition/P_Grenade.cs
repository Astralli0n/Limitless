using System.Linq;
using UnityEngine;

public class P_Grenade : MonoBehaviour
{
    [SerializeField] Transform Player;
    [SerializeField] float Range;
    [SerializeField] float ExplosionRange;
    [SerializeField] float KnockbackForce;
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
        if (Vector3.Distance(transform.position, InitPos) >= Range) {
            Invoke("Explode", Delay);
        }
    }

    public void SetStats(float _Range, float _ExplosionRange, float DMG, float _Delay, float Force, Vector3 Pos, Transform Origin) {
        Range = _Range;
        ExplosionRange = _ExplosionRange;
        Damage = DMG;
        Delay = _Delay;
        KnockbackForce = Force;
        InitPos = Pos;
        Player = Origin;
    }

    void OnCollisionEnter(Collision Other)
    {
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
        Destroy(gameObject, 0.2f);
        if (!DealtDMG) {
            var ObjectsInRange = Physics.OverlapSphere(transform.position, ExplosionRange)
                                        .Where(Obj => Obj.transform != transform)
                                        .Where(Obj => Obj.GetComponent<Health>() != null);

            if (ObjectsInRange.Count() > 0) {
                foreach (var Obj in ObjectsInRange) {
                    Obj.GetComponent<Health>().TakeDamage(Damage);
                    T_Ranged.RaiseOnAnyHit(Obj.GetComponent<Health>());

                    var ObjRB = Obj.GetComponent<Rigidbody>();
                    if(ObjRB != null) {
                        float Dist = Vector3.Distance(Obj.transform.position, transform.position);  

                        Vector3 Dir = (Obj.transform.position - transform.position).normalized;

                        var ObjController = Obj.GetComponent<PlayerController>();

                        if(ObjController == null) {
                            // Apply force: scaled by the knockback force and the force multiplier
                            ObjRB.AddExplosionForce(KnockbackForce, transform.position, ExplosionRange, 1f, ForceMode.Impulse);
                        } else {
                            Debug.Log(KnockbackForce * Dir);
                            ObjController.AddFrameForce(KnockbackForce * Dir, false);
                        }
                    }
                }
            }
        }

        DealtDMG = true;
    }
}