using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    [Header("Basic Stats")]
    [SerializeField] protected float Damage;
    [SerializeField] protected float ReloadTime;
    [SerializeField] protected float UnloadTime;
    [SerializeField] protected int MaxAmmo;
    [SerializeField] protected Transform FirePoint;
    protected PlayerController Player;

    [Header("Ammo")]
    [SerializeField] protected GameObject AmmoSegment;
    [SerializeField] protected float AmmoSpacing;
    [SerializeField] protected int CurrentAmmo;
    protected float CurrReloadTime;
    protected float CurrUnloadTime;
    protected Image ChargeBar;
    protected Transform AmmoParent;
    protected List<Image> AmmoBar;
    bool IsReloading;

    protected virtual void Awake() {
        Debug.Log($"Weapon Awake: CurrentAmmo set to {CurrentAmmo}");
        ChargeBar = GetComponentInParent<PlayerController>().transform.GetComponentsInChildren<Image>(true)
                    .FirstOrDefault(Img => Img.gameObject.name == "ChargeBar");

        AmmoParent = ChargeBar.transform.parent.Find("AmmoBar");

        CurrentAmmo = MaxAmmo;
        StartCoroutine(UpdateAmmoBarRoutine());
    }
    
    protected List<int> ReturnAmmoStats() {
        return new List<int> {CurrentAmmo, MaxAmmo};
    }

    protected void CheckReload() {
        if(!IsReloading && InputManager.Instance.ReloadInputPress && CurrentAmmo < MaxAmmo) {
            Reload();
        }

        if(IsReloading) {
            AmmoParent.GetComponent<Image>().enabled = true;
            AmmoParent.GetComponent<Image>().fillAmount = (ReloadTime - CurrReloadTime) / ReloadTime / 2f;
            if(CurrReloadTime < 0) {
                AmmoParent.GetComponent<Image>().fillAmount = 0f;
                CurrentAmmo = MaxAmmo;
                IsReloading = false;
            }
        } else {
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
            float Angle = InitAngle + (EndAngle - InitAngle) * (i - 0.5f) / (MaxAmmo - 1);
            
            GameObject Segment = Instantiate(AmmoSegment, AmmoParent);
            Segment.name = $"AmmoSegment_{i}";
            
            Segment.GetComponent<RectTransform>().localPosition = Vector3.zero;
            Segment.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, Angle - 90);
            Segment.GetComponent<Image>().fillAmount = 0.5f / MaxAmmo;
            
            AmmoBar.Add(Segment.GetComponent<Image>());
        }
        
    }

    private IEnumerator UpdateAmmoBarRoutine()
    {
        while (true) // Run indefinitely
        {
            CreateAmmoBar();
            yield return new WaitForSeconds(0.2f); // Wait for 0.2 seconds
        }
    }
 }
