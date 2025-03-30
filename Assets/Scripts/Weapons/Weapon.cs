using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    [Header("Basic Stats")]
    public bool IsActive;
    [SerializeField] protected float Damage;
    [SerializeField] protected float ReloadTime;
    [SerializeField] protected float UnloadTime;
    [SerializeField] protected int MaxAmmo;
    [SerializeField] protected Transform FirePoint;
    protected PlayerController Player;

    [Header("Ammo")]
    [SerializeField] protected GameObject AmmoSegment;
    [SerializeField] protected int CurrentAmmo;
    protected float CurrReloadTime;
    protected float CurrUnloadTime;
    protected Image ChargeBar;
    protected Transform AmmoParent;
    protected List<Image> AmmoBar;
    protected bool IsReloading;
    public static event Action<Health, GameObject, float, bool> OnAnyHit;

    protected virtual void Awake() {
        ChargeBar = GetComponentInParent<PlayerController>().transform.GetComponentsInChildren<Image>(true)
                    .FirstOrDefault(Img => Img.gameObject.name == "ChargeBar");

        AmmoParent = ChargeBar.transform.parent.Find("AmmoBar");

        CurrentAmmo = MaxAmmo;
        StartCoroutine(UpdateAmmoBarRoutine());
    }

    public static void InvokeOnAnyHit(Health EnemyHealth, GameObject Player, float DMG, bool IsTick)
    {
        OnAnyHit?.Invoke(EnemyHealth, Player, DMG, IsTick);
    }
    
    public List<int> ReturnAmmoStats() {
        return new List<int> {CurrentAmmo, MaxAmmo};
    }

    public void SetAmmo(int Current, int Max) {
        CurrentAmmo = Current;
        MaxAmmo = Max;

        StopAllCoroutines();

        StartCoroutine(UpdateAmmoBarRoutine());
    }

    protected virtual void CheckReload() {
        if(!IsReloading && InputManager.Instance.ReloadInputPress && CurrentAmmo < MaxAmmo && IsActive) {
            Reload();
        }

        if(IsReloading) {
            if(IsActive) {
                AmmoParent.GetComponent<Image>().enabled = true;
                AmmoParent.GetComponent<Image>().fillAmount = (ReloadTime - CurrReloadTime) / ReloadTime / 2f;
            }
            
            if(CurrReloadTime < 0) {
                if(IsActive) { AmmoParent.GetComponent<Image>().fillAmount = 0f; }
                CurrentAmmo = MaxAmmo;
                IsReloading = false;
            }
        } else if(IsActive){
            AmmoParent.GetComponent<Image>().enabled = false;
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

    protected void CreateAmmoBar()
    {
        AmmoBar = new List<Image>();
        
        foreach (Transform child in AmmoParent)
        {
            Destroy(child.gameObject);
        }

        float InitAngle = 90f;
        float EndAngle = -90f;
        
        for (int i = 0; i < CurrentAmmo; i++)
        {
            float Angle = InitAngle + (EndAngle - InitAngle) * (i - 0.5f) / (Mathf.Max(CurrentAmmo, MaxAmmo) - 1);
            
            GameObject Segment = Instantiate(AmmoSegment, AmmoParent);
            Segment.name = $"AmmoSegment_{i}";
            
            Segment.GetComponent<RectTransform>().localPosition = Vector3.zero;
            Segment.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, Angle - 90);
            Segment.GetComponent<Image>().fillAmount = 0.5f / Mathf.Max(CurrentAmmo, MaxAmmo);
            
            AmmoBar.Add(Segment.GetComponent<Image>());
        }
        
    }

    private IEnumerator UpdateAmmoBarRoutine()
    {
        while (true) // Run indefinitely
        {
            if(IsActive) {
                CreateAmmoBar();
                yield return new WaitForSeconds(0.2f); // Wait for 0.2 seconds
            } else {
                yield return null;
            }
        }
    }
 }
