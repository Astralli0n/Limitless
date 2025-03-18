using UnityEngine;
using UnityEngine.UI;

public class DummyHealth : Health {
    [SerializeField] Image HealthBar;
    protected override void Die() {
        gameObject.SetActive(false); // Disappear when "killed"
        Invoke(nameof(Respawn), 1f); // Respawn after 1 second
    }

    void Respawn() {
        gameObject.SetActive(true);
        CurrentHP = MaxHP;
    }
}
