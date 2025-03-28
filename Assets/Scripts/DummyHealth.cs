using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DummyHealth : Health {
    [SerializeField] Image HealthBar;
    [SerializeField] GameObject DummyVisuals; // Assign the child that holds the mesh or sprite

    protected override void Die() {
        // Skip calling base.Die() if it resets health
        base.Die();
        // Instead of disabling the entire GameObject, disable only the visuals
        if(DummyVisuals != null) {
            DummyVisuals.SetActive(false);
        }
        
        // Optionally disable colliders/interactions here as well.
        // Start the respawn coroutine
        StartCoroutine(RespawnRoutine());
    }

    protected override void Update() {
        base.Update();
        if(HealthBar != null)
            HealthBar.fillAmount = CurrentHP / MaxHP;
    }

    IEnumerator RespawnRoutine() {
        yield return new WaitForSeconds(1f);
        Respawn();
    }

    void Respawn() {
        Debug.Log("Dummy respawning");
        WasKilled = false;
        DummyVisuals.SetActive(true);
        CurrentHP = MaxHP;
    }
}
