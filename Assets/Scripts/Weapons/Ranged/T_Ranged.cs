using UnityEngine;

public class T_Ranged : Weapon
{
    [SerializeField] bool IsProjectileBased;
    [Header("Ranged Stats")]
    [SerializeField] protected float Range;
    [SerializeField] protected Vector3 Spread;
    CameraController CamController;

    [Header("Projectile Stats")]
    [SerializeField] protected float ProjectileSpeed;
    [SerializeField] protected GameObject ProjectilePrefab;

    // Global event for any hit – both projectile and hitscan can invoke it.

    protected override void Awake() {
        base.Awake();
        CamController = GetComponentInParent<PlayerController>().GetComponentInChildren<CameraController>();
    }

    protected virtual bool CanFire() {
        if (CurrentAmmo <= 0) { return false; }
        if (InputManager.Instance.FireInputRelease && CurrUnloadTime < 0 && IsActive) {
            return true;
        }
        return false;
    }

    protected GameObject CheckFire(Vector3 AimDir) {
        GameObject Projectile = null;
        if (IsProjectileBased) {
            Projectile = ProjectileFire(AimDir);
        } else {
            HitscanFire(AimDir);
        }

        CurrentAmmo -= 1;
        CurrUnloadTime = UnloadTime;
        CurrReloadTime = ReloadTime;

        return Projectile;
    }

    protected GameObject ProjectileFire(Vector3 AimDir) {
        CamController.RecoilFire(Spread);
        Vector3 ShootForce = AimDir.normalized * ProjectileSpeed;
        Quaternion AimRot = Quaternion.LookRotation(AimDir, Vector3.up);


        GameObject Projectile = Instantiate(ProjectilePrefab, FirePoint.position, AimRot);
        Projectile.GetComponent<Rigidbody>().linearVelocity = GetComponentInParent<Rigidbody>().linearVelocity;
        Projectile.GetComponent<Rigidbody>().AddForce(ShootForce, ForceMode.Impulse);
        return Projectile;
    }

    protected void HitscanFire(Vector3 AimDir) {
        RaycastHit Hit;

        CamController.RecoilFire(Spread);

        if (Physics.Raycast(FirePoint.position, AimDir, out Hit, Range)) {
            Debug.DrawRay(FirePoint.position, AimDir * Hit.distance, Color.red, 1.0f);
            // If the hit object has a Health component, trigger the hit event.
            if (Hit.collider.TryGetComponent<Health>(out Health EnemyHealth)) {
                InvokeOnAnyHit(EnemyHealth, gameObject, Damage, false);
            }
        } else {
            Debug.DrawRay(FirePoint.position, AimDir * Range, Color.yellow, 1.0f);
        }
    }

    protected override void Update() {
        base.Update();

        if (IsActive) {
            ChargeBar.enabled = false;
        }
    }
}
