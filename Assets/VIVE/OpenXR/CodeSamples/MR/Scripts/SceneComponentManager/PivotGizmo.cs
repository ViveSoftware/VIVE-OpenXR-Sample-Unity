using UnityEngine;
using UnityEngine.UI;

public class PivotGizmo : MonoBehaviour
{
    [SerializeField] private Text label;

    public void SetLabel(string labelText)
    {
        label.text = labelText;
    }
}
