using System.Collections.Generic;
using UnityEngine;

public class M_Recycle : T_OnHit
{
    [SerializeField] float AmmoIncreasePercentage;
    [SerializeField] float AmmoCap;
    
    // Static dictionary to store each weapon's base max ammo.
    private static Dictionary<Weapon, int> baseMaxAmmoDict = new Dictionary<Weapon, int>();

    public override void ResetModifier()
    {
        // Only perform recycling if the enemy (or dummy) is "dead".
        if (GetComponentInParent<Health>().GetHealth() <= 0f) {
            // Get all weapons attached to the Player.
            var weaponsList = Player.GetComponentsInChildren<Weapon>();

            foreach (var weapon in weaponsList) {
                // Get the current ammo stats (index 1 is MaxAmmo).
                var ammoStats = weapon.ReturnAmmoStats(); 

                int baseAmmo;
                // If this weapon hasn't been recycled before, record its current max ammo.
                if (!baseMaxAmmoDict.TryGetValue(weapon, out baseAmmo)) {
                    baseAmmo = ammoStats[1];
                    baseMaxAmmoDict[weapon] = baseAmmo;
                }
                
                // Calculate new current ammo based on the ammo increase percentage.
                int ammoIncrease = Mathf.RoundToInt(baseAmmo * AmmoIncreasePercentage);
                int newAmmo = Mathf.Min(baseAmmo + ammoIncrease, Mathf.RoundToInt(AmmoCap * baseAmmo));

                // Instead of increasing max ammo, use the stored base ammo.
                weapon.SetAmmo((int)Mathf.Min(newAmmo, AmmoCap * baseAmmo), baseAmmo);
            }
        }
        
        base.ResetModifier(); // This destroys the modifier prefab.
    }
}
