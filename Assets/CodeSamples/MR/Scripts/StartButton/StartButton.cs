using com.HTC.Common;
using DG.Tweening;
using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Vive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class StartButton : Singleton<StartButton>
    , IColliderEventHoverEnterHandler
    , IColliderEventHoverExitHandler
{
    [SerializeField] private GameObject fullButton;
    [SerializeField] private PlayableDirector dragTimeline;
    [SerializeField] private Transform downPivot;
    [SerializeField] private Transform upPivot;
    [SerializeField] private AudioSource clickAudio;
    private bool isHover = false;
    private bool buttonPressed = false;
    private bool gameStart = false;
    private List<Transform> rolePos = new List<Transform>();
    private float buttonDistance = 0;

    //update 20241219
    private void Start()
    {
        // Check if PivotManager.Instance is null
        if (PivotManager.Instance == null)
        {
            Debug.LogError("PivotManager.Instance is null. Make sure it is properly initialized.");
            return;
        }

        // Retrieve the Desk Pivot and check if it is null
        Transform deskPivot = PivotManager.Instance.GetPivot("Desk");
        if (deskPivot == null)
        {
            Debug.LogError("Desk Pivot is null. Make sure a pivot named 'Desk' exists in the PivotManager.");
            return;
        }

        // Safely set the fullButton's transform based on deskPivot
        if (fullButton != null)
        {
            fullButton.transform.position = deskPivot.position;
            fullButton.transform.localEulerAngles = deskPivot.forward; // 'deskPivot.transform.forward' adjusted to 'deskPivot.forward' for clarity
            fullButton.transform.DOScale(new Vector3(1f, 1f, 1f), 1f).SetEase(Ease.OutBack);
        }
        else
        {
            Debug.LogError("FullButton is null. Please assign it in the Inspector.");
        }

        // Check upPivot and downPivot before calculating buttonDistance
        if (upPivot != null && downPivot != null)
        {
            buttonDistance = Vector3.Distance(upPivot.position, downPivot.position);
        }
        else
        {
            Debug.LogError("One or both pivots (upPivot, downPivot) are null. Make sure they are properly assigned.");
        }
    }



    private void OnEnable()
    {
        dragTimeline.stopped += OnPlayableDirectorStopped;
    }

    public void OnColliderEventHoverEnter(ColliderHoverEventData eventData)
    {
        if (!buttonPressed && eventData.TryGetEventCaster(out ViveColliderEventCaster caster))
        {
            if (!isHover)
            {
                isHover = true;
                dragTimeline.time = 0;
                dragTimeline.Evaluate();
                rolePos.Clear();
                rolePos.Add(caster.transform);
            }
            else
            {
                if (!rolePos.Contains(caster.transform))
                {
                    rolePos.Add(caster.transform);
                }
            }

        }

    }

    public void OnColliderEventHoverExit(ColliderHoverEventData eventData)
    {
        if (!buttonPressed)
        {
            if (eventData.TryGetEventCaster(out ViveColliderEventCaster caster) && rolePos.Contains(caster.transform))
            {
                rolePos.Remove(caster.transform);
                if (rolePos.Count == 0)
                {
                    isHover = false;
                }
            }

            if (dragTimeline.time < dragTimeline.duration * 0.5f * 0.3f + 0.5f)
            {
                StartCoroutine(ButtonPressed());
            }
            else
            {
                dragTimeline.time = 0;
                dragTimeline.Evaluate();
            }
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Return) && !buttonPressed)
        {
            StartCoroutine(ButtonPressed());
        }
#endif
        if (isHover && !buttonPressed)
        {
            float value = Vector3.Distance(CommonFormula.PointFromPointToLine(upPivot.position, downPivot.position, rolePos[0].position), downPivot.position);
            for (int i = 1; i < rolePos.Count; i++)
            {
                float newValue = Vector3.Distance(CommonFormula.PointFromPointToLine(upPivot.position, downPivot.position, rolePos[i].position), downPivot.position);
                if (value > newValue)
                {
                    value = newValue;
                }
            }

            float dis = Mathf.InverseLerp(0, buttonDistance, value);
            if (dis >= 0 && dis <= 1)
            {
                if (dis < 0.3f)
                {
                    StartCoroutine(ButtonPressed());
                }
                else
                {
                    dragTimeline.time = dragTimeline.duration * dis + 0.5f;
                    dragTimeline.Evaluate();
                }
            }
        }
    }

    IEnumerator ButtonPressed()
    {
        buttonPressed = true;
        dragTimeline.Play();
        isHover = false;
        clickAudio.Play();
        yield return new WaitForSeconds(0.5f);
        fullButton.transform.DOScale(Vector3.zero, 0.5f);
    }

    void OnPlayableDirectorStopped(PlayableDirector director)
    {
        if (dragTimeline == director)
        {
            gameStart = true;
        }
    }

    private void OnDisable()
    {
        dragTimeline.stopped -= OnPlayableDirectorStopped;
    }

    public bool GameStart()
    {
        return gameStart;
    }

    public void Restart()
    {
        gameStart = false;
        dragTimeline.time = 0;
        dragTimeline.Evaluate();
        rolePos.Clear();
    }
}
