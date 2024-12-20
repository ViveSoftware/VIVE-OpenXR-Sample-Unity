using DG.Tweening;
using UnityEngine;
using static MRFlowManager;

public class UniversePortalBall : PortalBallBehaviour
{
    [SerializeField] private GameObject solarObject;
    [SerializeField] private ParticleSystem fogParticle;
    [SerializeField] private Transform jelbeePivot;

    [SerializeField] private string groupKey = "Space";
    [SerializeField] private string destroyCmdKey = "Planet";
    [SerializeField] private string pivotKey = "Desk";

    private bool universePortalStart = false;
    private float time = 0;
    private float planetGameTime = 0;

    public int completeInSolarCount = 0;

    protected override void OnStartPortal()
    {

        if (PivotManager.Instance != null)
        {
            Transform pivot = PivotManager.Instance.GetPivot(pivotKey);
            if (pivot != null)
            {
                fogParticle.transform.SetPositionAndRotation(pivot.position, Quaternion.identity);
                jelbeePivot.transform.SetPositionAndRotation(pivot.position + jelbeePivot.transform.localPosition, Quaternion.identity);
                solarObject.transform.SetPositionAndRotation(transform.position + transform.forward * 0.05f, Quaternion.identity);

                solarObject.transform.DOScale(Vector3.one, 3).OnComplete(() =>
                {
                    if (GrabberManager.Instance != null)
                        GrabberManager.Instance.TurnOn();
                });

                solarObject.transform.DOMove(pivot.position + Vector3.up * 0.25f, 3).SetEase(Ease.InOutCubic);
            }
            else
            {
                Debug.Log($"Pivot with key '{pivotKey}' is null.");
            }
        }
        else
        {
            Debug.Log("PivotManager.Instance is null. Cannot retrieve pivot.");
            return;
        }

        if (RobotAssistantManager.robotAssistantManagerInstance != null)
        {
            RobotAssistantManager.robotAssistantManagerInstance.SetRobotPosition(jelbeePivot.transform.position);
        }

        if (GrabberManager.Instance != null)
        {
            GrabberManager.Instance.EnableGroup(groupKey);
        }

        fogParticle.Play(true);
        time = 0;
        planetGameTime = 30;
        universePortalStart = true;
    }

    private void Update()
    {
        if (universePortalStart)
        {
            if (time <= planetGameTime && planetGameTime > 0)
            {
                time += Time.deltaTime;
            }
            else if (time > planetGameTime && planetGameTime > 0)
            {
                universePortalStart = false;
                time = 0;

                if (MRFlowManager.Instance != null)
                {
                    MRFlowManager.Instance.GameState = State.end;
                }

                StopExperience();
            }
        }
    }

    public void StopExperience()
    {
        fogParticle.Stop(true);
        ClosePortalRing();

        if (solarObject != null)
        {
            solarObject.transform.DOScale(Vector3.zero, 3).OnComplete(() =>
            {
                DestroyPortal();
                if (MRFlowManager.Instance != null)
                    MRFlowManager.Instance.GameState = State.end;
            });
        }

        if (GrabberManager.Instance != null)
        {
            GrabberManager.Instance.TurnOff();
        }
    }

    public void SetInParent(int a, GameObject obj)
    {
        Transform child = solarObject.transform.GetChild(a - 1);
        child.GetComponent<SolorRotate>().enabled = true;
        obj.transform.SetParent(child);
        obj.transform.DOLocalMove(Vector3.zero, 3);
        obj.GetComponent<PlanetsHandler>().EnterSolarSystem();

        completeInSolarCount++;
        if (completeInSolarCount < 8)
        {
            time = 0;
            planetGameTime = 30;
        }
    }

    public void SetOutParent(int a, GameObject obj)
    {
        obj.transform.DOKill();
        if (obj.transform.parent == solarObject.transform.GetChild(a - 1))
            completeInSolarCount--;
    }
}
