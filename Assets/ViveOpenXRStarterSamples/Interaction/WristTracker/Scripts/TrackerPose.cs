// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using UnityEngine.XR;
#if DEFINE_VIVE_OPENXR
using UnityEngine.InputSystem;
#endif

namespace VIVE.OpenXR.StarterSample.WristTracker
{
    public class TrackerPose : MonoBehaviour
    {

        #region Inspector
#if DEFINE_VIVE_OPENXR
        public bool IsLeft = false;

        [SerializeField]
        private InputActionReference m_DevicePose = null;
        public InputActionReference DevicePose { get { return m_DevicePose; } set { m_DevicePose = value; } }
        bool getDeviceTracked(InputActionReference actionReference)
        {

            bool tracked = false;

            if (OpenXRHelper.VALIDATE(actionReference, out string value))
            {
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.InputSystem.XR.PoseState))
#else
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.XR.OpenXR.Input.Pose))
#endif
                {
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                    tracked = actionReference.action.ReadValue<UnityEngine.InputSystem.XR.PoseState>().isTracked;
#else
                    tracked = actionReference.action.ReadValue<UnityEngine.XR.OpenXR.Input.Pose>().isTracked;
#endif
                    
                }
            }
 
            return tracked;

        }
        InputTrackingState getDeviceTrackingState(InputActionReference actionReference)
        {
            InputTrackingState state = InputTrackingState.None;

            if (OpenXRHelper.VALIDATE(actionReference, out string value))
            {
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.InputSystem.XR.PoseState))
#else
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.XR.OpenXR.Input.Pose))
#endif
                {
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                    state = actionReference.action.ReadValue<UnityEngine.InputSystem.XR.PoseState>().trackingState;
#else
                    state = actionReference.action.ReadValue<UnityEngine.XR.OpenXR.Input.Pose>().trackingState;
#endif
                }
            }
  
            return state;
        }
        Vector3 getDevicePosition(InputActionReference actionReference)
        {
            var position = Vector3.zero;

            if (OpenXRHelper.VALIDATE(actionReference, out string value))
            {
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.InputSystem.XR.PoseState))
#else
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.XR.OpenXR.Input.Pose))
#endif
                {
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                    position = actionReference.action.ReadValue<UnityEngine.InputSystem.XR.PoseState>().position;
#else
                    position = actionReference.action.ReadValue<UnityEngine.XR.OpenXR.Input.Pose>().position;
#endif
                }
            }
          
            return position;
        }
        Quaternion getDeviceRotation(InputActionReference actionReference)
        {
            var rotation = Quaternion.identity;

            if (OpenXRHelper.VALIDATE(actionReference, out string value))
            {
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.InputSystem.XR.PoseState))
#else
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.XR.OpenXR.Input.Pose))
#endif
                {
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                    rotation = actionReference.action.ReadValue<UnityEngine.InputSystem.XR.PoseState>().rotation;
#else
                    rotation = actionReference.action.ReadValue<UnityEngine.XR.OpenXR.Input.Pose>().rotation;
#endif
                   
                }
            }
           
            return rotation;
        }

        [SerializeField]
        private InputActionReference m_PrimaryButton = null;
        public InputActionReference PrimaryButton { get { return m_PrimaryButton; } set { m_PrimaryButton = value; } }

        [SerializeField]
        private InputActionReference m_Menu = null;
        public InputActionReference Menu { get { return m_Menu; } set { m_Menu = value; } }
        bool getButton(InputActionReference actionReference)
        {
            bool pressed = false;

            if (OpenXRHelper.VALIDATE(actionReference, out string value))
            {
                if (actionReference.action.activeControl.valueType == typeof(bool))
                    pressed = actionReference.action.ReadValue<bool>();
                if (actionReference.action.activeControl.valueType == typeof(float))
                    pressed = actionReference.action.ReadValue<float>() > 0;
            }
           
            return pressed;
        }
#endif
        #endregion

        int printFrame = 0;
        protected bool printIntervalLog = false;
        private void Update()
        {
            printFrame++;
            printFrame %= 300;
            printIntervalLog = (printFrame == 0);
#if DEFINE_VIVE_OPENXR
            var tracked = getDeviceTracked(m_DevicePose);
            var trackingState = getDeviceTrackingState(m_DevicePose);
            var position = getDevicePosition(m_DevicePose);
            var rotation = getDeviceRotation(m_DevicePose);


            if (tracked)
            {
                transform.localPosition = position;
                transform.localRotation = rotation;
            }
           
#endif
        }
    }
}
