using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Basic Stats")]
    [SerializeField] protected float Damage;
    [SerializeField] protected float ReloadTime;
    [SerializeField] protected float UnloadTime;
    [SerializeField] protected int MaxAmmo;
    [SerializeField] protected Transform FirePoint;
    protected PlayerController Player;
    [SerializeField] protected int CurrentAmmo;
    protected float CurrReloadTime;
    protected float CurrUnloadTime;
    bool IsReloading;

    protected virtual void Awake() {
        Debug.Log($"Weapon Awake: CurrentAmmo set to {CurrentAmmo}");
        CurrentAmmo = MaxAmmo;
    }
    
    protected List<int> ReturnAmmoStats() {
        return new List<int> {CurrentAmmo, MaxAmmo};
    }

    protected void CheckReload() {
        if(!IsReloading && InputManager.Instance.ReloadInputPress && CurrentAmmo < MaxAmmo) {
            Reload();
        }

        if(IsReloading) {
            if(CurrReloadTime < 0) {
                CurrentAmmo = MaxAmmo;
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
