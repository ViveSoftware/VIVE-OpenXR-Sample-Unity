using UnityEngine;

public class SolorRotate : MonoBehaviour
{
    public GameObject Axis;
    public float _RotationSpeed;
    void Update()
    {
        this.transform.RotateAround(Axis.transform.position, Axis.transform.up, _RotationSpeed / 3);
    }
}
