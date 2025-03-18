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

    protected void CreateAmmoBar()
    {
        AmmoBar = new List<Image>();
        
        // Clear any existing ammo segments first
        foreach (Transform child in AmmoParent)
        {
            Destroy(child.gameObject);
        }

        // For a perfect semicircle (180 degrees)
        float startAngle = 90f;     // Right side (0 degrees)
        float endAngle = -90f;     // Left side (180 degrees)
        
        for (int i = 0; i < CurrentAmmo; i++)
        {
            // Calculate angle in degrees, evenly distributed
            float angleInDegrees = startAngle + (endAngle - startAngle) * i / (MaxAmmo - 1);
            
            // Convert to radians for position calculation
            float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
            
            // Create the segment
            GameObject segment = Instantiate(AmmoSegment, AmmoParent);
            segment.name = $"AmmoSegment_{i}";
            
            // Set position
            RectTransform rectTransform = segment.GetComponent<RectTransform>();
            
            // Rotate to face outward
            rectTransform.localRotation = Quaternion.Euler(0, 0, angleInDegrees - 90);
            segment.GetComponent<Image>().fillAmount = 0.5f / MaxAmmo;
            
            // Add to list
            AmmoBar.Add(segment.GetComponent<Image>());
        }
        
        // Log confirmation
        Debug.Log($"Created ammo bar with {AmmoBar.Count} segments");
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
