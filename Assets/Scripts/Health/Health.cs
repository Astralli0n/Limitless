using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StatusEffectData {
    public string Type;
    public float TickNum;
    public float Delay;
    public float Damage;
    public float StartTime;
    public float TicksElapsed;
    public List<Modifier> Modifiers;
    public GameObject Player;

    public StatusEffectData(string _Type, float Num, float _Delay, float DMG, float Time, List<Modifier> Mods, GameObject _Player) {
        Type = _Type;
        TickNum = Num;
        Delay = _Delay;
        Damage = DMG;
        StartTime = Time;
        TicksElapsed = 1f;
        Modifiers = Mods;
        Player = _Player;
    }
}

public class Health : MonoBehaviour, IDamageable {
    [SerializeField] protected float CurrentHP;
    [SerializeField] protected float MaxHP;
    [SerializeField] protected float OverHealDecay;
    [SerializeField] protected List<StatusEffectData> ActiveStatusEffects;
    protected bool WasKilled = false;

    public event System.Action<float> OnHealthChanged;

    void Awake() {
        CurrentHP = MaxHP;
        ActiveStatusEffects = new List<StatusEffectData>();
    }

    public void ApplyStatusEffect(string Type, float TickNum, float Delay, float DMG, float StartTime, List<GameObject> ModifierTypes, GameObject Player) {
        // Check for existing effect of the same type
        StatusEffectData existingEffect = ActiveStatusEffects.FirstOrDefault(e => e.Type == Type);

        if (existingEffect != null)
        {
            // Refresh the existing effect
            existingEffect.StartTime = Time.time;
            existingEffect.TicksElapsed = 1f;
            // Optionally update other properties if needed (e.g., Damage, TickNum)
            existingEffect.Damage = DMG;
            existingEffect.TickNum = TickNum;
            existingEffect.Delay = Delay;

            // Reset modifiers (optional, if you want new modifiers each time)
            foreach (Modifier Mod in existingEffect.Modifiers)
            {
                Mod.ResetModifier();
            }
            existingEffect.Modifiers.Clear();

            // Add new modifiers
            if (ModifierTypes != null)
            {
                foreach (var ModType in ModifierTypes)
                {
                    if (ModType == null) continue;

                    var NewMod = Instantiate(ModType, transform).GetComponent<Modifier>();
                    existingEffect.Modifiers.Add(NewMod);
                    NewMod.Player = Player;
                }
            }
        }
        else
        {
            // No existing effect, proceed to add a new one
            List<Modifier> Modifiers = new List<Modifier>();
            if (ModifierTypes != null)
            {
                foreach (var ModType in ModifierTypes)
                {
                    if (ModType == null) continue;

                    var NewMod = Instantiate(ModType, transform).GetComponent<Modifier>();
                    Modifiers.Add(NewMod);
                    NewMod.Player = Player;
                }
            }

            ActiveStatusEffects.Add(new StatusEffectData(Type, TickNum, Delay, DMG, StartTime, Modifiers, Player));
        }
    }

    public bool HasStatusEffect(string type)
    {
        return ActiveStatusEffects.Any(e => e.Type == type);
    }


    protected void HandleTicks() {
        if (ActiveStatusEffects.Count == 0) return;

        List<StatusEffectData> EffectsToRemove = new List<StatusEffectData>();

        foreach (var Effect in ActiveStatusEffects) {
            Debug.Log($"Processing effect: {Effect.Type}, TicksElapsed: {Effect.TicksElapsed}, TickNum: {Effect.TickNum}");

            if (Time.time >= Effect.StartTime + Effect.TicksElapsed * Effect.Delay)
            {
                TakeDamage(Effect.Damage);
                Debug.Log($"Applying damage for effect: {Effect.Type}, Damage: {Effect.Damage}");

                // This invocation (with isTick true) will be sent to all subscribed modifiers.
                Weapon.InvokeOnAnyHit(GetComponent<Health>(), Effect.Player, Effect.Damage, true);
                Effect.TicksElapsed += 1f;  // Increment ticks

                if (Effect.TicksElapsed > Effect.TickNum)
                {
                    Debug.Log($"Removing effect: {Effect.Type} after {Effect.TicksElapsed} ticks.");
                    EffectsToRemove.Add(Effect);
                }
            }
        }

        foreach (var Effect in EffectsToRemove) {
            foreach (Modifier Mod in Effect.Modifiers) {
                Mod.ResetModifier();
            }
            ActiveStatusEffects.Remove(Effect);
        }
    }

    protected virtual void Update() {
        HandleTicks();

        if(CurrentHP > MaxHP) {
            CurrentHP -= OverHealDecay * Time.deltaTime;
            if (CurrentHP < MaxHP) {
                CurrentHP = MaxHP;
            }
        }
    }

    public virtual void TakeDamage(float Damage) {
        CurrentHP -= Damage;
        OnHealthChanged?.Invoke(CurrentHP / MaxHP);

        if (CurrentHP <= 0 && !WasKilled) {
            Die();
        }
    }

    protected virtual void Die() {
        Debug.Log("ELIMINATED");
        WasKilled = true;
        
        // Cancel any pending invokes
        CancelInvoke();

        // Clear all active status Effects immediately.
        List<StatusEffectData> EffectsToRemove = new List<StatusEffectData>(ActiveStatusEffects);
        foreach (var Effect in EffectsToRemove) {
            foreach (Modifier mod in Effect.Modifiers) {
                mod.ResetModifier();
            }
            ActiveStatusEffects.Remove(Effect);
        }
        
        // Make sure ActiveStatusEffects is empty (defensive programming)
        ActiveStatusEffects.Clear();
        
        // Reset health
        CurrentHP = MaxHP;
        // Keep WasKilled as true until something else explicitly resets it
        // WasKilled = false; <- Remove this line
    }



    public float GetHealth() {
        return CurrentHP;
    }

    public float GetMaxHealth() {
        return MaxHP;
    }
}
