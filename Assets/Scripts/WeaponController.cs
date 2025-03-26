using System.Linq;
using UnityEngine;
using TMPro;

public class WeaponController : MonoBehaviour
{
    [SerializeField] Transform WeaponHolder;
    [SerializeField] Weapon[] Weapons;
    [SerializeField] TMP_Text WeaponNameUI;
    int CurrentWeaponIndex;
    int LastWeaponIndex;

    void Awake() {
        Weapons = WeaponHolder.GetComponentsInChildren<Weapon>();
        Weapons[0].IsActive = true;
        WeaponNameUI.text = Weapons[0].gameObject.name;
        CurrentWeaponIndex = 0;
        LastWeaponIndex = 0;

        for (int i = 1; i < Weapons.Count(); i++)
        {
            Weapons[i].IsActive = false;
        }
    }

    void Update() {
        if(InputManager.Instance.PreviousWeaponInputPress) {
            if(CurrentWeaponIndex == 0) {
                CurrentWeaponIndex = Weapons.Count() - 1;
            } else {
                CurrentWeaponIndex -= 1;
            }
        } else if(InputManager.Instance.NextWeaponInputPress) {
            if(CurrentWeaponIndex == Weapons.Count() - 1) {
                CurrentWeaponIndex = 0;
            } else {
                CurrentWeaponIndex += 1;
            }
        }

        if(LastWeaponIndex != CurrentWeaponIndex) {
            foreach (var Weapon in Weapons)
            {
                Weapon.IsActive = false;
            }

            Weapons[CurrentWeaponIndex].IsActive = true;
            WeaponNameUI.text = Weapons[CurrentWeaponIndex].gameObject.name;
            LastWeaponIndex = CurrentWeaponIndex;
        }
    }
}
