using System;
using UnityEngine;

public class T_Ranged : Weapon
{
    [SerializeField] bool IsProjectileBased;
    [Header("Ranged Stats")]
    [SerializeField] protected float Range;

    [Header("Projectile Stats")]
    [SerializeField] protected float ProjectileSpeed;
    [SerializeField] protected GameObject ProjectilePrefab;

    // Global event for any hit – both projectile and hitscan can invoke it.
    public static event Action<Health> OnAnyHit;

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
        Vector3 ShootForce = AimDir * ProjectileSpeed;
        Quaternion AimRot = Quaternion.LookRotation(AimDir, Vector3.up);    

        GameObject Projectile = Instantiate(ProjectilePrefab, FirePoint.position, AimRot);
        Projectile.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        Projectile.GetComponent<Rigidbody>().AddForce(ShootForce, ForceMode.Impulse);
        return Projectile;
    }

    protected void HitscanFire(Vector3 AimDir) {
        RaycastHit Hit;

        if (Physics.Raycast(FirePoint.position, AimDir, out Hit, Range)) {
            Debug.DrawRay(FirePoint.position, AimDir * Hit.distance, Color.red, 1.0f);
            // If the hit object has a Health component, trigger the hit event.
            if (Hit.collider.TryGetComponent<Health>(out Health EnemyHealth)) {
                OnAnyHit?.Invoke(EnemyHealth);
            }
        } else {
            Debug.DrawRay(FirePoint.position, AimDir * Range, Color.yellow, 1.0f);
        }
    }

    public static void RaiseOnAnyHit(Health EnemyHealth)
    {
        OnAnyHit?.Invoke(EnemyHealth);
    }

    protected override void Update() {
        base.Update();

        if (IsActive) {
            ChargeBar.enabled = false;
        }
    }
}
