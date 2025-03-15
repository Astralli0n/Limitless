using UnityEngine;

public class Health : MonoBehaviour, IDamageable {
    [SerializeField] protected float CurrentHP;
    [SerializeField] protected float MaxHP;

    void Awake() {
        CurrentHP = MaxHP;
    }

    public virtual void TakeDamage(float damage) {
        CurrentHP -= damage;
        if (CurrentHP <= 0) {
            Die();
        }
    }

    protected virtual void Die() {
        Debug.Log("ELIMINATED");
        CurrentHP = MaxHP; // Reset if needed
    }

    public float GetHealth() {
        return CurrentHP;
    }

    public float GetMaxHealth() {
        return MaxHP;
    }
}
