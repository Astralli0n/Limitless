using UnityEngine;

public class Modifier : MonoBehaviour
{
    public int GearCost;
    public enum GearType {
        Offense,
        Control,
        Defence
    }

    public virtual void ResetModifier() {
        Destroy(gameObject);
    }
}
