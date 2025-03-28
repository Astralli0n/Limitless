using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class T_OnHit : Modifier
{
    [Header("On Hit Stats")]
    [SerializeField] string EffectType;
    [SerializeField] float TickDMG;
    [SerializeField] float TickNum;
    [SerializeField] float TickDelay;
    [SerializeField] GameObject ModifierGameObject;
    public bool OnEnemy = true;

    void OnEnable()
    {
        if(!OnEnemy) {
            // Subscribe to the global hit event.
            T_Ranged.OnAnyHit += HandleOnHit;
        }
    }

    void OnDisable()
    {
        if(!OnEnemy) {
            T_Ranged.OnAnyHit -= HandleOnHit;
        }
    }

    // This method will be called whenever a hitscan shot registers a hit.
    void HandleOnHit(Health EnemyHealth)
    {
        if (EnemyHealth == null) return;
        float StartTime = Time.time;
        // Let the Health script apply the status effect.
        EnemyHealth.ApplyStatusEffect(EffectType, TickNum, TickDelay, TickDMG, StartTime, new List<GameObject>() { ModifierGameObject });
    }
}
