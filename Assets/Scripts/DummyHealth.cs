using UnityEngine;

public class DummyHealth : Health {
    protected override void Die() {
        gameObject.SetActive(false); // Disappear when "killed"
        Invoke(nameof(Respawn), 1f); // Respawn after 1 second
    }

    void Respawn() {
        gameObject.SetActive(true);
        CurrentHP = MaxHP;
    }
}
