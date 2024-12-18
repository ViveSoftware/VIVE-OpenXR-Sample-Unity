using UnityEngine;

public class LightAligner : MonoBehaviour
{
    private void OnEnable()
    {
        Transform pivot = PivotManager.Instance.GetLightRoot();
        if (pivot != null)
        {
            transform.SetPositionAndRotation(pivot.position, pivot.rotation);
        }
    }
}
