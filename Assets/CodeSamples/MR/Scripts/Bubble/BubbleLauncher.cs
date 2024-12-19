using com.HTC.Common;
using System.Collections;
using UnityEngine;
using VIVE.OpenXR;
using VIVE.OpenXR.Hand;
using VIVE.OpenXR.Toolkits.CustomGesture;

public class BubbleLauncher : Singleton<BubbleLauncher>
{
    [SerializeField] private Collider[] handColliders_Right;
    [SerializeField] private Collider[] handColliders_Left;
    [SerializeField] private Transform[] jalbeeEscapePoint;
    [SerializeField] private Transform launchPoint_Right;
    [SerializeField] private Transform launchPoint_Left;
    [SerializeField] private Transform praticleFOVLimit;
    [SerializeField] private ParticleSystem bubbleParticle;
    [SerializeField] private ParticleSystem bubbleParticle_Left;
    [SerializeField] private float offset = 0f;
    [SerializeField] private bool ignoreHit;

    private Vector3 jointPos = new Vector3();
    private Vector3 targetDir = new Vector3();
    private Vector3 newDir = new Vector3();
    private Transform cam;
    private Transform jelbee;
    private Transform desk;
    private Transform lastEscapePoint;
    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.EmissionModule emissionModule_Left;
    private bool stopEscape = false;
    private float stopTimer = 0;


    protected override void AwakeSingleton()
    {
        bubbleParticle.Stop();
        emissionModule = bubbleParticle.emission;
        emissionModule_Left = bubbleParticle_Left.emission;
        gameObject.SetActive(false);
        if (MRTutorialManager.Instance != null)
        {
            MRTutorialManager.Instance.OnTutorialStartHandler += OnTutorialStart;
            MRTutorialManager.Instance.OnTutorialEndHandler += OnTutorialEnd;
        }
        else
        {
            Debug.Log("MRTutorialManager.Instance is null.");
        }

    }

    private void OnEnable()
    {
        cam = Camera.main?.transform;
        desk = PivotManager.Instance?.GetPivot("Desk");

        if (desk != null && jalbeeEscapePoint != null && jalbeeEscapePoint.Length > 0)
        {
            jalbeeEscapePoint[0].parent.transform.position = desk.position;
        }
        else
        {
            Debug.LogError("Desk or JalbeeEscapePoint is null or not properly initialized.");
        }
    }

    private void OnTutorialStart()
    {
        stopEscape = true;
        StopAllCoroutines();
    }

    private void OnTutorialEnd()
    {
        stopEscape = false;
        ignoreHit = false;
    }

    private void Update()
    {
        if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                OnBubbleTouched();
            }
            if (Input.GetKeyDown(KeyCode.F11))
            {
                StopAllCoroutines();
            }
        }

        praticleFOVLimit.eulerAngles = cam.eulerAngles;
        praticleFOVLimit.position = cam.position;

        {
            LaunchBubble(true);
            LaunchBubble(false);
        }
    }

    private bool LaunchBubble(bool isLeftHand)
    {
#if UNITY_EDITOR
        if (MRFlowManager.Instance.GameState == MRFlowManager.State.portal) return false;
#endif
        bool hadLaunch = false;

        CGEnums.HandFlag _Hand = CGEnums.HandFlag.Right;
        if (isLeftHand == true)
        {
            _Hand = CGEnums.HandFlag.Left;
        }

        if (CustomGestureDefiner.IsCurrentGestureTriiggered("LaunchBubble", _Hand) || CustomGestureDefiner.IsCurrentGestureTriiggered("OK", _Hand))
        {
#if !UNITY_EDITOR
            if (PortalSceneManager.Instance != null){
                PortalSceneManager.Instance.PauseDrawindPortal();
            }
            else
            {
                Debug.Log("PortalSceneManager.Instance is null.");
            }
#endif
            SwitchColliders(false, isLeftHand);

            if (XR_EXT_hand_tracking.Interop.GetJointLocations(isLeftHand, out XrHandJointLocationEXT[] handJointLocaton))
            {
                jointPos = OpenXRHelper.ToUnityVector(handJointLocaton[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_DISTAL_EXT].pose.position);
            }

            targetDir = (jointPos - cam.position).normalized;

            if (isLeftHand)
            {
#if !UNITY_EDITOR
                newDir = Vector3.RotateTowards(launchPoint_Left.forward, targetDir, 1, 0.0f);
                launchPoint_Left.rotation = Quaternion.LookRotation(newDir);
                launchPoint_Left.position = jointPos + targetDir * offset;
#else
                newDir = Camera.main.transform.forward;
                launchPoint_Left.rotation = Quaternion.LookRotation(newDir);
                launchPoint_Left.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.5f));
#endif
                emissionModule_Left.enabled = true;
            }
            else
            {
#if !UNITY_EDITOR
                newDir = Vector3.RotateTowards(launchPoint_Right.forward, targetDir, 1, 0.0f);
                launchPoint_Right.rotation = Quaternion.LookRotation(newDir);
                launchPoint_Right.position = jointPos + targetDir * offset;
#else
                newDir = Camera.main.transform.forward;
                launchPoint_Right.rotation = Quaternion.LookRotation(newDir);
                launchPoint_Right.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.5f));
#endif
                emissionModule.enabled = true;
            }

            if (!bubbleParticle.isPlaying)
            {
                bubbleParticle.Play();
            }

            hadLaunch = true;

            return hadLaunch;
        }
#if UNITY_ANDROID
        if (PortalSceneManager.Instance != null)
        {
            PortalSceneManager.Instance.RestartDrawindPortal();
        }
        else
        {
            Debug.Log("PortalSceneManager.Instance is null.");
        }

#endif

        if (isLeftHand)
        {
            emissionModule_Left.enabled = false;
            SwitchColliders(true, true);
        }
        else
        {
            emissionModule.enabled = false;
            SwitchColliders(true, false);
        }
        return hadLaunch;
    }

    public void SwitchColliders(bool value, bool isLeftHand)
    {
        if (isLeftHand)
        {
            foreach (Collider collider in handColliders_Left)
            {
                collider.enabled = value;
            }
        }
        else
        {
            foreach (Collider collider in handColliders_Right)
            {
                collider.enabled = value;
            }
        }
    }

    public void OnBubbleTouched()
    {
        if (stopEscape || !gameObject.activeSelf) return;
        StartCoroutine(JalbeeEscape());
    }

    private IEnumerator JalbeeEscape()
    {
        if (!ignoreHit)
        {
            RobotAssistantManager robotAssistantManager = null;
            if (RobotAssistantManager.robotAssistantManagerInstance != null)
            {
                ignoreHit = true;
                robotAssistantManager = RobotAssistantManager.robotAssistantManagerInstance;
                robotAssistantManager.moveSpeed = 2;

                jelbee = robotAssistantManager.transform;

                Transform targetPoint = jalbeeEscapePoint[Random.Range(0, jalbeeEscapePoint.Length - 1)];

                if (lastEscapePoint != null)
                {
                    while (targetPoint == lastEscapePoint)
                    {
                        targetPoint = jalbeeEscapePoint[Random.Range(0, jalbeeEscapePoint.Length - 1)];
                    }
                }
                lastEscapePoint = targetPoint;

                Vector3 targetPosition = targetPoint.position;

                robotAssistantManager.TriggerReaction(RobotAssistantEnums.ReactionAnimationIndex.Happy);

                yield return new WaitForSeconds(1f);
                robotAssistantManager.ForceStopReaction();
                robotAssistantManager.SetRobotPosition(targetPosition);

                yield return new WaitForSeconds(2f);
                robotAssistantManager.ForceStopReaction();
                robotAssistantManager = RobotAssistantManager.robotAssistantManagerInstance;
                robotAssistantManager.moveSpeed = 1;
                robotAssistantManager.TriggerLeisure();
                ignoreHit = false;
            }
        }
        yield return null;
    }
}