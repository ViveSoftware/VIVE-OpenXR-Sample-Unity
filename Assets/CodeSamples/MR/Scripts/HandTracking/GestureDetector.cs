using com.HTC.Common;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace com.HTC.Gesture
{
    public class GestureDetector : MonoBehaviour
    {
        [SerializeField]
        private bool detectOnStart = false;

        [SerializeField]
        private int curStep;

        [SerializeField]
        private IGestureDetectMethod[] detectedMethods;

        public UnityEvent StartRecognizingHandler;

        public UnityEvent RecognizingHandler;

        public UnityEventFloat RecognizingStepHandler;

        public UnityEventInt StartWaitingNextStepHandler;

        public UnityEvent WaitingNextStepHandler;

        public UnityEventInt StartRecognizeStepHandler;

        public UnityEvent CompleteStepHandler;

        public UnityEvent StopRecognizingHandler;

        public UnityEvent CancelHandler;

        public UnityEvent CompleteHandler;

        public UnityEvent ReleaseHandler;

        private float curDetectTime;

        private float curDetectLostTime;

        private float curWaitNextStepTime;

        private Action curStateUpdateHandler = delegate { };

        private IGestureDetectMethod curDetectMethod
        {
            get
            {
                return detectedMethods[curStep];
            }
        }

        private IGestureDetectMethod nextDetectMethod
        {
            get
            {
                return detectedMethods[curStep + 1];
            }
        }

        private bool isDetecting = false;

        private void Start()
        {
            if (detectOnStart)
            {
                StartDetect();
            }
        }

        public void StartDetect()
        {
            if (!isDetecting)
            {
                isDetecting = true;
                curStep = 0;
                curStateUpdateHandler = WaitStart;
            }
            else
            {
                Debug.LogWarning("Gesture Detector is already detecting.");
            }
        }

        private void WaitStart()
        {
            if (curDetectMethod.IsDetected())
            {
                curDetectTime = curDetectMethod.Duration;
                curDetectLostTime = curDetectMethod.DetectingToleration;
                StartRecognizingHandler.Invoke();
                StartRecognizeStepHandler.Invoke(curStep);
                curStateUpdateHandler = RecognizingStep;
            }
        }

        private void RecognizingStep()
        {
            curDetectTime -= Time.deltaTime;

            if (curDetectTime <= 0)
            {
                CompleteStepHandler.Invoke();
                if (curStep == detectedMethods.Length - 1)
                {
                    curStateUpdateHandler = WaitStart;
                    Complete();
                }
                else
                {
                    curWaitNextStepTime = nextDetectMethod.StartToleration;
                    curStateUpdateHandler = WaitNextStep;
                    StartWaitingNextStepHandler.Invoke(curStep + 1);
                }
            }
            else
            {
                if (curDetectMethod.IsDetected())
                {
                    curDetectLostTime = curDetectMethod.DetectingToleration;
                    RecognizingStepHandler.Invoke(1f - curDetectTime / curDetectMethod.Duration);
                }
                else
                {
                    curDetectLostTime -= Time.deltaTime;
                    if (curDetectLostTime <= 0)
                    {
                        curStateUpdateHandler = WaitStart;
                        Cancel();
                    }
                }
            }
        }

        private void WaitNextStep()
        {
            if (nextDetectMethod.IsDetected())
            {
                curStep = curStep + 1;
                curDetectTime = curDetectMethod.Duration;
                curStateUpdateHandler = RecognizingStep;
                StartRecognizeStepHandler.Invoke(curStep);
            }
            else
            {
                WaitingNextStepHandler.Invoke();

                if (curDetectMethod.IsDetected())
                {
                    curDetectLostTime = curDetectMethod.DetectingToleration;
                    curWaitNextStepTime = nextDetectMethod.StartToleration;
                }
                else
                {
                    curDetectLostTime -= Time.deltaTime;
                    if (curDetectLostTime > 0) return;

                    curWaitNextStepTime -= Time.deltaTime;
                    if (curWaitNextStepTime > 0) return;

                    curStateUpdateHandler = WaitStart;
                    Cancel();
                }
            }
        }

        public void StopDetect()
        {
            if (isDetecting)
            {
                isDetecting = false;
                if (curStateUpdateHandler != WaitStart)
                {
                    Cancel();
                }
                curStateUpdateHandler = delegate { };
            }
        }

        private void Update()
        {
            curDetectMethod.UpdateTime(Time.deltaTime);
            curStateUpdateHandler.Invoke();
        }

        private void Complete()
        {
            curStep = 0;
            CompleteHandler.Invoke();
            StopRecognizingHandler.Invoke();
        }

        private void Cancel()
        {
            curStep = 0;
            CancelHandler.Invoke();
            StopRecognizingHandler.Invoke();
        }
    }
}
