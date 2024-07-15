// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;

namespace VIVE.OpenXR.StarterSample.WristTracker
{
    [RequireComponent(typeof(Text))]
    public class TrackerText : MonoBehaviour
    {

        #region Right Tracker
        [SerializeField]
        private InputActionReference m_TrackedR = null;
        public InputActionReference TrackedR { get => m_TrackedR; set => m_TrackedR = value; }

        [SerializeField]
        private InputActionReference m_TrackingStateR = null;
        public InputActionReference TrackingStateR { get => m_TrackingStateR; set => m_TrackingStateR = value; }

        [SerializeField]
        private InputActionReference m_RightA = null;
        public InputActionReference RightA { get => m_RightA; set => m_RightA = value; }
        #endregion

        #region Left Tracker
        [SerializeField]
        private InputActionReference m_TrackedL = null;
        public InputActionReference TrackedL { get => m_TrackedL; set => m_TrackedL = value; }

        [SerializeField]
        private InputActionReference m_TrackingStateL = null;
        public InputActionReference TrackingStateL { get => m_TrackingStateL; set => m_TrackingStateL = value; }

        [SerializeField]
        private InputActionReference m_LeftX = null;
        public InputActionReference LeftX { get => m_LeftX; set => m_LeftX = value; }

        [SerializeField]
        private InputActionReference m_LeftMenu = null;
        public InputActionReference LeftMenu { get => m_LeftMenu; set => m_LeftMenu = value; }
        #endregion

        private Text m_Text = null;

        private void Start()
        {
            m_Text = GetComponent<Text>();
        }
        private void Update()
        {
            if (m_Text == null) { return; }

            // Left tracker text
            m_Text.text = "Left Tracker ";

            { // Tracked
                if (GetButton(m_TrackedL, out bool value, out string msg))
                {
                    m_Text.text += "tracked: " + value + ", ";
                }
            }
            { // trackingState
                if (GetInteger(m_TrackingStateL, out InputTrackingState value, out string msg))
                {
                    m_Text.text += "state: " + value + ", ";
                }
            }

            { // Left X
                if (GetButton(m_LeftX, out bool value, out string msg))
                {
                    if (value)
                    {
                        Debug.Log("Update() Left X is pressed.");
                        m_Text.text += "Left X";
                    }
                }
            }
            { // Left Menu
                if (GetButton(m_LeftMenu, out bool value, out string msg))
                {
                    if (value)
                    {
                        Debug.Log("Update() Left Menu is pressed.");
                        m_Text.text += "Left Menu";
                    }
                }
            }

            // Right tracker text
            m_Text.text += "\nRight Tracker ";

            { // Tracked
                if (GetButton(m_TrackedR, out bool value, out string msg))
                {
                    m_Text.text += "tracked: " + value + ", ";
                }
            }
            { // trackingState
                if (GetInteger(m_TrackingStateR, out InputTrackingState value, out string msg))
                {
                    m_Text.text += "state: " + value + ", ";
                }
            }

            { // Right A
                if (GetButton(m_RightA, out bool value, out string msg))
                {
                    if (value)
                    {
                        Debug.Log("Update() Right A is pressed.");
                        m_Text.text += "Right A";
                    }
                }
            }
        }

        public enum ActionRefError 
        {
            NONE = 0,
            REFERENCE_NULL = 1,
            ACTION_NULL = 2,
            DISABLED = 3,
            ACTIVECONTROL_NULL = 4,
            NO_CONTROLS_COUNT = 5,
        }
        public string Name( ActionRefError error)
        {
            if (error == ActionRefError.REFERENCE_NULL) { return "Null reference."; }
            if (error == ActionRefError.ACTION_NULL) { return "Null reference action."; }
            if (error == ActionRefError.DISABLED) { return "Reference action disabled."; }
            if (error == ActionRefError.ACTIVECONTROL_NULL) { return "No active control of the reference action."; }
            if (error == ActionRefError.NO_CONTROLS_COUNT) { return "No action control count."; }

            return "";
        }
        private ActionRefError VALIDATE(InputActionReference actionReference)
        {
            if (actionReference == null) { return ActionRefError.REFERENCE_NULL; }
            if (actionReference.action == null) { return ActionRefError.ACTION_NULL; }
            if (!actionReference.action.enabled) { return ActionRefError.DISABLED; }
            if (actionReference.action.activeControl == null) { return ActionRefError.ACTIVECONTROL_NULL; }
            else if (actionReference.action.controls.Count <= 0) { return ActionRefError.NO_CONTROLS_COUNT; }

            return ActionRefError.NONE;
        }
        public bool GetButton(InputActionReference actionReference, out bool value, out string msg)
        {
            var result = VALIDATE(actionReference);

            value = false;
            msg = Name(result);

            if (result == ActionRefError.NONE)
            {
                if (actionReference.action.activeControl.valueType == typeof(float))
                    value = actionReference.action.ReadValue<float>() > 0;
                if (actionReference.action.activeControl.valueType == typeof(bool))
                    value = actionReference.action.ReadValue<bool>();

                return true;
            }

            return false;
        }
        public bool GetInteger(InputActionReference actionReference, out InputTrackingState value, out string msg)
        {
            var result = VALIDATE(actionReference);

            value = 0;
            msg = Name(result);

            if (result == ActionRefError.NONE)
            {
                if (actionReference.action.activeControl.valueType == typeof(int))
                {
                    int diff = 0;
                    int i = actionReference.action.ReadValue<int>();

                    diff = i & ((int)InputTrackingState.Position);
                    if (diff != 0) { value |= InputTrackingState.Position; }

                    diff = i & ((int)InputTrackingState.Rotation);
                    if (diff != 0) { value |= InputTrackingState.Rotation; }

                    diff = i & ((int)InputTrackingState.Velocity);
                    if (diff != 0) { value |= InputTrackingState.Velocity; }

                    diff = i & ((int)InputTrackingState.AngularVelocity);
                    if (diff != 0) { value |= InputTrackingState.AngularVelocity; }

                    diff = i & ((int)InputTrackingState.Acceleration);
                    if (diff != 0) { value |= InputTrackingState.Acceleration; }

                    diff = i & ((int)InputTrackingState.AngularAcceleration);
                    if (diff != 0) { value |= InputTrackingState.AngularAcceleration; }
                }

                return true;
            }

            return false;
        }
    }
}
