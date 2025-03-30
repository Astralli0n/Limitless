using System.Collections.Generic;
using UnityEngine;

public class T_OnHit : Modifier
{
    [Header("On Hit Stats")]
    [SerializeField] protected string EffectType;
    [SerializeField] protected float TickDMG;
    [SerializeField] protected float TickNum;
    [SerializeField] protected float TickDelay;
    [SerializeField] protected GameObject ModifierGameObject;
    public bool OnEnemy = true;

    void Awake() {
        if(!OnEnemy) {
            Player = GetComponentInParent<PlayerController>().gameObject;
        }
    }

    void OnEnable()
    {
        if(!OnEnemy) {
            // Subscribe to the global hit event.
            Weapon.OnAnyHit += HandleOnHit;
        }
    }

    void OnDisable()
    {
        if(!OnEnemy) {
            Weapon.OnAnyHit -= HandleOnHit;
        }
    }

    // This method will be called whenever a hitscan shot registers a hit.
    protected virtual void HandleOnHit(Health EnemyHealth, GameObject Player, float Damage, bool IsTick)
    {
        if (EnemyHealth == null || OnEnemy) return;
        float StartTime = Time.time;
        // Let the Health script apply the status effect.
        EnemyHealth.ApplyStatusEffect(EffectType, TickNum, TickDelay, TickDMG, StartTime, new List<GameObject>() { ModifierGameObject }, Player);
    }
            
}
