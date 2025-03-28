using System.Collections.Generic;
using UnityEngine;
public class StatusEffectData {
    public string Type;
    public float TickNum;     // Total ticks (effect duration)
    public float Delay;       // Time between ticks
    public float DMG;         // Damage per tick
    public float StartTime;   // When the effect was applied
    public float TicksElapsed; // How many ticks have been processed
    public List<Modifier> Modifiers; // Any extra modifiers for this effect

    public StatusEffectData(string type, float tickNum, float delay, float dmg, float startTime, List<Modifier> modifiers) {
        Type = type;
        TickNum = tickNum;
        Delay = delay;
        DMG = dmg;
        StartTime = startTime;
        TicksElapsed = 1f;
        Modifiers = modifiers;
    }
}

public class Health : MonoBehaviour, IDamageable {
    [SerializeField] protected float CurrentHP;
    [SerializeField] protected float MaxHP;

    // Store all active status effects in a list
    [SerializeField] protected List<StatusEffectData> ActiveStatusEffects;
    protected bool WasKilled = false;

    public event System.Action<float> OnHealthChanged; // Notify UI

    void Awake() {
        CurrentHP = MaxHP;
        ActiveStatusEffects = new List<StatusEffectData>();
    }

    // Now, ApplyStatusEffect adds a new status effect instance without replacing any existing one.
    public void ApplyStatusEffect(string Type, float TickNum, float Delay, float DMG, float StartTime, List<GameObject> ModifierTypes) {
        // Remove any existing status effect of the same type:
        List<StatusEffectData> effectsToRemove = new List<StatusEffectData>();
        foreach (var effect in ActiveStatusEffects) {
            if (effect.Type == Type) {
                // Reset (and thereby delete) all modifier GameObjects for this effect.
                foreach (Modifier mod in effect.Modifiers) {
                    mod.ResetModifier(); // This should destroy the modifier GameObject.
                }
                effectsToRemove.Add(effect);
            }
        }
        // Remove the old effects from the list.
        foreach (var effect in effectsToRemove) {
            ActiveStatusEffects.Remove(effect);
        }
        
        // Create new modifiers list:
        List<Modifier> modifiers = new List<Modifier>();
        if (ModifierTypes != null) {
            foreach (var modType in ModifierTypes) {
                if (modType == null) { continue; }
                // Instantiate the prefab and get its Modifier component.
                var newMod = Instantiate(modType, transform).GetComponent<Modifier>();
                modifiers.Add(newMod);
            }
        }
        
        // Add the new status effect.
        ActiveStatusEffects.Add(new StatusEffectData(Type, TickNum, Delay, DMG, StartTime, modifiers));
    }

    protected void HandleTicks() {
        if (ActiveStatusEffects.Count == 0) return;

        // We'll use a temporary list for effects to remove.
        List<StatusEffectData> EffectsToRemove = new List<StatusEffectData>();

        foreach (var effect in ActiveStatusEffects) {
            // Check if it's time for the next tick.
            if (Time.time >= effect.StartTime + effect.TicksElapsed * effect.Delay) {
                // Apply damage for this tick.
                TakeDamage(effect.DMG);
                effect.TicksElapsed += 1f;

                // If we've processed more ticks than intended, mark the effect for removal.
                if (effect.TicksElapsed > effect.TickNum) {
                    EffectsToRemove.Add(effect);
                }
            }
        }

        // Remove expired effects.
        foreach (var effect in EffectsToRemove) {
            // Also remove any modifier components that were added.
            foreach (Modifier mod in effect.Modifiers) {
                mod.ResetModifier();
            }
            ActiveStatusEffects.Remove(effect);
        }
    }

    protected virtual void Update() {
        HandleTicks();
    }

    public virtual void TakeDamage(float Damage) {
        CurrentHP -= Damage;
        OnHealthChanged?.Invoke(CurrentHP / MaxHP); // Notify UI

        if (CurrentHP <= 0 && !WasKilled) {
            Die();
        }
    }

    protected virtual void Die() {
        Debug.Log("ELIMINATED");
        WasKilled = true;
        
        // Create a temporary copy of the active effects to remove.
        List<StatusEffectData> effectsToRemove = new List<StatusEffectData>(ActiveStatusEffects);
        
        foreach (var effect in effectsToRemove) {
            foreach (Modifier mod in effect.Modifiers) {
                mod.ResetModifier();
            }
            ActiveStatusEffects.Remove(effect);
        }
        
        CurrentHP = MaxHP;
        WasKilled = false;
    }


    public float GetHealth() {
        return CurrentHP;
    }

    public float GetMaxHealth() {
        return MaxHP;
    }
}
