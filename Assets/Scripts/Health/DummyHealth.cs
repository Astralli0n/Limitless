using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DummyHealth : Health {
    [SerializeField] Image HealthBar;
    [SerializeField] GameObject DummyVisuals; // Assign the child that holds the mesh or sprite

    protected override void Update() {
        base.Update();
        if(HealthBar != null)
            HealthBar.fillAmount = CurrentHP / MaxHP;
    }

    IEnumerator RespawnRoutine() {
        yield return new WaitForSeconds(1f);
        Respawn();
    }

    protected override void Die() {
        // Set WasKilled first
        WasKilled = true;
        
        // Clear effects manually rather than calling base.Die()
        List<StatusEffectData> EffectsToRemove = new List<StatusEffectData>(ActiveStatusEffects);
        foreach (var Effect in EffectsToRemove) {
            foreach (Modifier mod in Effect.Modifiers) {
                mod.ResetModifier();
            }
            ActiveStatusEffects.Remove(Effect);
        }
        ActiveStatusEffects.Clear();
        
        // Hide visuals
        if(DummyVisuals != null) {
            DummyVisuals.SetActive(false);
        }
        
        // Start the respawn coroutine
        StartCoroutine(RespawnRoutine());
    }

    void Respawn() {
        Debug.Log("Dummy respawning");
        WasKilled = false;
        DummyVisuals.SetActive(true);
        CurrentHP = MaxHP;
        // Ensure no status effects remain
        ActiveStatusEffects.Clear();
    }
}
