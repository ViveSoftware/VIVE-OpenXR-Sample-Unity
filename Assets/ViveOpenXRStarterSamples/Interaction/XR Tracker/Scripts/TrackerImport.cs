// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using System.Text;
#if DEFINE_VIVE_OPENXR
using UnityEngine.InputSystem;
#endif

namespace VIVE.OpenXR.StarterSample.XRTracker 
{
    public class TrackerImport : MonoBehaviour
    {
#if DEFINE_VIVE_OPENXR
        [SerializeField]
        private string m_TrackerName = "";
        public string TrackerName { get { return m_TrackerName; } set { m_TrackerName = value; } }

        [SerializeField]
        private InputActionReference m_IsTracked = null;
        public InputActionReference IsTracked { get { return m_IsTracked; } set { m_IsTracked = value; } }

        [SerializeField]
        private InputActionReference m_TrackingState = null;
        public InputActionReference TrackingState { get { return m_TrackingState; } set { m_TrackingState = value; } }

        [SerializeField]
        private InputActionReference m_Position = null;
        public InputActionReference Position { get { return m_Position; } set { m_Position = value; } }

        [SerializeField]
        private InputActionReference m_Rotation = null;
        public InputActionReference Rotation { get { return m_Rotation; } set { m_Rotation = value; } }

        [SerializeField]
        private InputActionReference m_Menu = null;
        public InputActionReference Menu { get { return m_Menu; } set { m_Menu = value; } }

        [SerializeField]
        private InputActionReference m_GripPress = null;
        public InputActionReference GripPress { get { return m_GripPress; } set { m_GripPress = value; } }

        [SerializeField]
        private InputActionReference m_TriggerPress = null;
        public InputActionReference TriggerPress { get { return m_TriggerPress; } set { m_TriggerPress = value; } }

        [SerializeField]
        private InputActionReference m_TrackpadPress = null;
        public InputActionReference TrackpadPress { get { return m_TrackpadPress; } set { m_TrackpadPress = value; } }

        [SerializeField]
        private InputActionReference m_TrackpadTouch = null;
        public InputActionReference TrackpadTouch { get { return m_TrackpadTouch; } set { m_TrackpadTouch = value; } }
#endif

        private Text m_Text = null;
        private void Start()
        {
            m_Text = GetComponent<Text>();
        }



        void Update()
        {
#if DEFINE_VIVE_OPENXR
            if (m_Text == null) { return; }

            m_Text.text = m_TrackerName;

            m_Text.text += " isTracked: ";
            {
                if (GetButton(m_IsTracked, out bool value, out string msg))
                {
                    m_Text.text += value;
                }
                else
                {
                    m_Text.text += msg;
                }
            }
            m_Text.text += "\n";
            m_Text.text += "trackingState: ";
            {
                if (GetInteger(m_TrackingState, out InputTrackingState value, out string msg))
                {
                    m_Text.text += value;
                }
                else
                {
                    m_Text.text += msg;
                }
            }
            m_Text.text += "\n";
            m_Text.text += "position (";
            {
                if (GetVector3(m_Position, out Vector3 value, out string msg))
                {
                    m_Text.text += value.x.ToString() + ", " + value.y.ToString() + ", " + value.z.ToString();
                }
                else
                {
                    m_Text.text += msg;
                }
            }
            m_Text.text += ")\n";
            m_Text.text += "rotation (";
            {
                if (GetQuaternion(m_Rotation, out Quaternion value, out string msg))
                {
                    m_Text.text += value.x.ToString() + ", " + value.y.ToString() + ", " + value.z.ToString() + ", " + value.w.ToString();
                }
                else
                {
                    m_Text.text += msg;
                }
            }

            m_Text.text += ")";
            m_Text.text += "\nmenu: ";
            {
                if (GetButton(m_Menu, out bool value, out string msg))
                {
                    m_Text.text += value;
                }
                else
                {
                    m_Text.text += msg;
                }
            }
            m_Text.text += "\ngrip: ";
            {
                if (GetButton(m_GripPress, out bool value, out string msg))
                {
                    m_Text.text += value;
                }
                else
                {
                    m_Text.text += msg;
                }
            }
            m_Text.text += "\ntrigger press: ";
            {
                if (GetButton(m_TriggerPress, out bool value, out string msg))
                {
                    m_Text.text += value;

                    if (PerformHaptic(m_TriggerPress, out msg))
                    {
                        m_Text.text += ", Vibrate";
                    }
                    else
                    {
                        m_Text.text += ", Failed: " + msg;
                    }
                }
                else
                {
                    m_Text.text += msg;
                }
            }
            m_Text.text += "\ntrackpad press: ";
            {
                if (GetButton(m_TrackpadPress, out bool value, out string msg))
                {
                    m_Text.text += value;
                }
                else
                {
                    m_Text.text += msg;
                }
            }
            m_Text.text += "\ntrackpad touch: ";
            {
                if (GetButton(m_TrackpadTouch, out bool value, out string msg))
                {
                    m_Text.text += value;
                }
                else
                {
                    m_Text.text += msg;
                }
            }
#endif
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
        public static string Name(ActionRefError error)
        {
            if (error == ActionRefError.REFERENCE_NULL) { return "Null reference"; }
            if (error == ActionRefError.ACTION_NULL) { return "Null reference action"; }
            if (error == ActionRefError.DISABLED) { return "Reference action disabled"; }
            if (error == ActionRefError.ACTIVECONTROL_NULL) { return "No active control of the reference action"; }
            if (error == ActionRefError.NO_CONTROLS_COUNT) { return "No action control count"; }

            return "";
        }
#if DEFINE_VIVE_OPENXR
        private static ActionRefError VALIDATE(InputActionReference actionReference)
        {
            if (actionReference == null) { return ActionRefError.REFERENCE_NULL; }
            if (actionReference.action == null) { return ActionRefError.ACTION_NULL; }
            if (!actionReference.action.enabled) { return ActionRefError.DISABLED; }
            if (actionReference.action.activeControl == null) { return ActionRefError.ACTIVECONTROL_NULL; }
            else if (actionReference.action.controls.Count <= 0) { return ActionRefError.NO_CONTROLS_COUNT; }

            return ActionRefError.NONE;
        }

        public static bool GetButton(InputActionReference actionReference, out bool value, out string msg)
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

        public static bool GetInteger(InputActionReference actionReference, out InputTrackingState value, out string msg)
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
        public static bool GetVector3(InputActionReference actionReference, out Vector3 value, out string msg)
        {
            var result = VALIDATE(actionReference);

            value = Vector3.zero;
            msg = Name(result);

            if (result == ActionRefError.NONE)
            {
                if (actionReference.action.activeControl.valueType == typeof(Vector3))
                    value = actionReference.action.ReadValue<Vector3>();

                return true;
            }

            return false;
        }
        public static bool GetQuaternion(InputActionReference actionReference, out Quaternion value, out string msg)
        {
            var result = VALIDATE(actionReference);

            value = Quaternion.identity;
            msg = Name(result);

            if (result == ActionRefError.NONE)
            {
                if (actionReference.action.activeControl.valueType == typeof(Quaternion))
                    value = actionReference.action.ReadValue<Quaternion>();

                Vector3 direction = value * Vector3.forward;
                return true;
            }

            return false;
        }
        public bool PerformHaptic(InputActionReference actionReference, out string msg)
        {
            var result = VALIDATE(actionReference);

            msg = Name(result);

            if (result == ActionRefError.NONE)
            {
                float amplitude = 1.0f;
                float duration = 0.1f;
                var command = UnityEngine.InputSystem.XR.Haptics.SendHapticImpulseCommand.Create(0, amplitude, duration);
                actionReference.action.activeControl.device.ExecuteCommand(ref command);

                return true;
            }

            return false;
        }
#endif
    }
}

