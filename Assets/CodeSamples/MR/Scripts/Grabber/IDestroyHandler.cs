using UnityEngine;

public abstract class IDestroyHandler : MonoBehaviour
{
    public abstract void RunDestroy(bool isInstant = false);
    public abstract void StopDestroy();
}
