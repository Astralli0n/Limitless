using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable {
    [SerializeField] protected float CurrentHP;
    [SerializeField] protected float MaxHP;
    [SerializeField] protected Dictionary<string, List<float>> StatusEffects; // {TYPE, TICK NUM, DELAY, TICK DAMAGE, START TICK TIME, TICKS REMAINING}

    void Awake() {
        CurrentHP = MaxHP;
        StatusEffects = new Dictionary<string, List<float>>();
    }

    public void ApplyStatusEffect(string Type, float TickNum, float Delay, float DMG, float StartTime) {
        if(StatusEffects.ContainsKey(Type)) { 
            StatusEffects.Remove(Type);
        }

        StatusEffects.Add(Type, new List<float>() {TickNum, Delay, DMG, StartTime, 1f});
    }

    protected void HandleTicks() {
        if(StatusEffects.Count == 0) { return; }

        var EffectsToRemove = new List<string>();

        foreach (var Status in StatusEffects)
        {
            if(Time.time > Status.Value[3] + Status.Value[4] * Status.Value[1]) {
                TakeDamage(Status.Value[2]);

                Status.Value[4] += 1f;
                if(Status.Value[4] == Status.Value[0] + 1) {
                    EffectsToRemove.Add(Status.Key);
                }
            }
        }

        if(EffectsToRemove.Count == 0) { return; }

        foreach (var Status in EffectsToRemove)
        {
            StatusEffects.Remove(Status);
        }
    }

    protected virtual void Update() {
        HandleTicks();
    }

    public virtual void TakeDamage(float Damage) {
        CurrentHP -= Damage;
        if (CurrentHP <= 0) {
            Die();
        }
    }

    protected virtual void Die() {
        Debug.Log("ELIMINATED");
        StatusEffects.Clear();
        CurrentHP = MaxHP; // Reset if needed
    }

    public float GetHealth() {
        return CurrentHP;
    }

    public float GetMaxHealth() {
        return MaxHP;
    }
}
