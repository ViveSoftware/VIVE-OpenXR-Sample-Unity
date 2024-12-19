using System;
using UnityEngine;
using VIVE.OpenXR;
using VIVE.OpenXR.Toolkits.PlaneDetection;
using static VIVE.OpenXR.PlaneDetection.VivePlaneDetection;

[Serializable]
public class MRScenePerceptionHelper
{
    PlaneDetector pd;
    XrPlaneDetectionStateEXT planePerceptionState;
    public bool isSceneComponentRunning { get; private set; } = false;
    public bool isScenePerceptionStarted { get; private set; } = false;

    private const string LOG_TAG = "JelbeeMR";


    private static class Log
    {
        public static void i(string tag, string msg, bool inEditor = false)
        {
            Debug.LogFormat("{0} {1}", tag, msg);
        }
        public static void e(string tag, string msg, bool inEditor = false)
        {
            Debug.LogFormat("{0} {1}", tag, msg);
        }
    }

    public void OnEnable()
    {

        if (!PlaneDetectionManager.IsSupported())
        {
            Log.e(LOG_TAG, "ScenePerception Not Supported");
            throw new Exception("Scene Perception is not supported on this device..");
        }

        XrResult result = pd.BeginPlaneDetection();

        if (result == XrResult.XR_SUCCESS)
        {
            isSceneComponentRunning = true;
        }
        else
        {
            Log.e(LOG_TAG, "Start scene failed!");
        }


    }

    public void OnDisable()
    {
        if (!isSceneComponentRunning) return;

        StopScenePerception();

        isSceneComponentRunning = false;
    }

    public void StartScenePerception(Action successHandler, Action failedHandler)
    {
        if (isSceneComponentRunning && !isScenePerceptionStarted)
        {

            XrResult result = pd.BeginPlaneDetection();

            if (result == XrResult.XR_SUCCESS)
            {
                Log.i(LOG_TAG, "Start scene perception for 2d planes success");
                isScenePerceptionStarted = true;
                ScenePerceptionGetState();
                successHandler?.Invoke();
            }
            else
            {
                Log.e(LOG_TAG, "Start scene perception for 2d planes error.");
                failedHandler?.Invoke();
            }
        }
        else
        {
            Log.e(LOG_TAG, $"Start scene perception for 2d planes error: isSceneComponentRunning[{isSceneComponentRunning}] isScenePerceptionStarted[{isScenePerceptionStarted}]"); ;
        }
    }

    public void StopScenePerception()
    {
        if (isSceneComponentRunning && isScenePerceptionStarted)
        {
            PlaneDetectionManager.DestroyPlaneDetector(pd);
            isScenePerceptionStarted = false;

        }
    }

    public void ScenePerceptionGetState()
    {
        XrPlaneDetectionStateEXT latestPerceptionState = XrPlaneDetectionStateEXT.NONE_EXT;
        XrPlaneDetectionStateEXT result = pd.GetPlaneDetectionState();
        if (result == XrPlaneDetectionStateEXT.DONE_EXT)
        {
            planePerceptionState = latestPerceptionState;
        }
    }

    public bool CurrentPerceptionTargetIsCompleted()
    {
        return planePerceptionState == XrPlaneDetectionStateEXT.DONE_EXT;
    }

    public bool CurrentPerceptionTargetIsEmpty()
    {
        return planePerceptionState == XrPlaneDetectionStateEXT.NONE_EXT;
    }
}
