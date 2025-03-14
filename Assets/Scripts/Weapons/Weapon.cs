using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Basic Stats")]
    [SerializeField] protected float Damage;
    [SerializeField] protected float ReloadTime;
    [SerializeField] protected float UnloadTime;
    [SerializeField] protected float CurrentAmmo;
    [SerializeField] protected int MaxAmmo;
    [SerializeField] protected Transform FirePoint;
    protected PlayerController Player;
    protected int Ammo;
    protected float CurrReloadTime;
    protected float CurrUnloadTime;
    bool IsReloading;
    
    protected List<int> ReturnAmmoStats() {
        return new List<int> {Ammo, MaxAmmo};
    }

    protected void CheckReload() {
        if(!IsReloading && InputManager.Instance.ReloadInputPress && Ammo < MaxAmmo) {
            Reload();
        }

        if(IsReloading) {
            if(CurrReloadTime < 0) {
                Ammo = MaxAmmo;
                IsReloading = false;
            }
        }
    }

    protected void Reload() {
        CurrReloadTime = ReloadTime;
        IsReloading = true;
    }

    protected virtual void Update() {
        CheckReload();
        Timer();
    }

    protected void Timer() {
        if(IsReloading) {
            CurrReloadTime -= Time.deltaTime;
        }
        CurrUnloadTime -= Time.deltaTime;
    }
 }
