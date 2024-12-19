using UnityEngine;

public class PivotRegister : MonoBehaviour
{
    [SerializeField] private string pivotName;
    void Start()
    {
        if (BoxPivotManager.Instance != null)
        {

            BoxPivotManager.Instance.Register(pivotName, transform);

        }
        else
        {
            Debug.Log("BoxPivotManager.Instance is null");
            return;
        }
    }
}
