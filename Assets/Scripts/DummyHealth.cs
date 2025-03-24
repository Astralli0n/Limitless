using UnityEngine;
using UnityEngine.UI;

public class DummyHealth : Health {
    [SerializeField] Image HealthBar;
    protected override void Die() {
        gameObject.SetActive(false); // Disappear when "killed"
        Invoke(nameof(Respawn), 1f); // Respawn after 1 second
    }

    protected override void Update()
    {
        base.Update();
        HealthBar.fillAmount = CurrentHP / MaxHP;
    }

    void Respawn() {
        gameObject.SetActive(true);
        HealthBar.fillAmount = 1f;
        CurrentHP = MaxHP;
    }
}
