using UnityEngine;

public class CameraController : MonoBehaviour
{
    Vector3 CurrentRot;
    Vector3 TargetRot;
    [Header("Recoil Settings")]
    [SerializeField] float Snappiness;
    [SerializeField] float ReturnSpeed;

    void Update() {
        TargetRot = Vector3.Lerp(TargetRot, Vector3.zero, ReturnSpeed * Time.deltaTime);
        CurrentRot = Vector3.Slerp(CurrentRot, TargetRot, Snappiness * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(CurrentRot);
    }

    public void RecoilFire(Vector3 RecoilStats) {
        TargetRot += new Vector3(RecoilStats.x, Random.Range(-RecoilStats.y, RecoilStats.y), Random.Range(-RecoilStats.z, RecoilStats.z));
    }
}
