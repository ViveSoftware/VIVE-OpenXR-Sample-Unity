using com.HTC.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MRTutorialManager : Singleton<MRTutorialManager>
{
    [Serializable]
    public class TutorialDefine
    {
        public string Tutorial;
        public string HandHintPivot;
        public string VoicePlayer;
        public string Clip;
        public float Duration;
    }

    [SerializeField] private string robotPivotKey = "Desk";
    [SerializeField] private Vector3 pivotOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private RobotAssistantEnums.FacialAnimationIndex[] faces;
    [SerializeField] private HandHintManager handHintMgr;
    [SerializeField] private TutorialDefine[] tutorialDefines;

    private string currentTutorial;
    private Dictionary<string, TutorialDefine> tutorialDefMap = new Dictionary<string, TutorialDefine>();

    public Action OnTutorialStartHandler;
    public Action OnTutorialEndHandler;
    public bool IsTutorialPlaying { get; private set; }

    private Action onTutorialCompletedCallback = delegate { };
    private Vector3 robotPivot;

    void Start()
    {
        tutorialDefMap = new Dictionary<string, TutorialDefine>();
        foreach (TutorialDefine def in tutorialDefines)
        {
            tutorialDefMap[def.Tutorial] = def;
        }

        if (PivotManager.Instance != null)
        {
            Transform pivot = PivotManager.Instance.GetPivot(robotPivotKey);
            robotPivot = pivot.position + pivotOffset;
        }
        else
        {
            Debug.Log("PivotManager.Instance is null");
            return;
        }

    }

    public void ShowTutorial(string tutorialName, bool flyToPivot, Action onTutorialCompleted = null)
    {
        if (IsTutorialPlaying)
        {
            CancelInvoke();
            InvokeTutorialCompleted();
        }

        currentTutorial = tutorialName;
        IsTutorialPlaying = true;

        Debug.Log($"show tutorial: {tutorialName}");

        if (onTutorialCompleted != null)
        {
            onTutorialCompletedCallback += onTutorialCompleted;
        }

        if (flyToPivot)
        {
            MoveRobotToPivot();
        }
        else
        {
            ShowCurrentTutorial();
        }

        OnTutorialStartHandler?.Invoke();
    }

    private void MoveRobotToPivot()
    {
        if (RobotAssistantManager.robotAssistantManagerInstance != null)
        {
            RobotAssistantManager.robotAssistantManagerInstance.ForceStopReaction();
            RobotAssistantManager.robotAssistantManagerInstance.SetRobotPosition(robotPivot, ShowCurrentTutorial);
        }
        else
        {
            Debug.LogError("RobotAssistantManager.instance is null. Cannot move robot to pivot.");
        }
    }

    private void ShowCurrentTutorial()
    {
        if (!string.IsNullOrEmpty(currentTutorial))
        {
            TutorialDefine tutorial = null;

            if (tutorialDefMap.TryGetValue(currentTutorial, out tutorial))
            {
                if (RobotAssistantManager.robotAssistantManagerInstance != null)
                {
                    RobotAssistantManager.robotAssistantManagerInstance.TriggerLeisure();
                }
                else
                {
                    Debug.LogError("RobotAssistantManager.instance is null. Cannot trigger leisure.");
                    return;
                }

                if (BoxPivotManager.Instance != null)
                {
                    Transform handHintPivot = BoxPivotManager.Instance.Get(tutorial.HandHintPivot);
                    handHintMgr.Show(tutorial.Tutorial, handHintPivot);
                    Invoke("InvokeTutorialCompleted", tutorial.Duration);
                }
                else
                {
                    Debug.LogError("BoxPivotManager.Instance is null. Cannot retrieve hand hint pivot.");
                }
            }
            else
            {
                Debug.LogError($"Tutorial: [{currentTutorial}] does not exist!");
            }
        }
        else
        {
            Debug.LogError("Tutorial ID cannot be empty!");
        }
    }

    private void InvokeTutorialCompleted()
    {
        if (RobotAssistantManager.robotAssistantManagerInstance != null)
        {
            RobotAssistantManager.robotAssistantManagerInstance.ForceStopReaction();
        }
        else
        {
            Debug.LogError("RobotAssistantManager.instance is null. Cannot force stop robot reaction.");
        }

        IsTutorialPlaying = false;

        onTutorialCompletedCallback?.Invoke();
        OnTutorialEndHandler?.Invoke();

        currentTutorial = null;
        handHintMgr.Hide();
    }


    public void ForceStopTutorial()
    {
        if (IsTutorialPlaying)
        {
            CancelInvoke();
            InvokeTutorialCompleted();
        }
    }
}
