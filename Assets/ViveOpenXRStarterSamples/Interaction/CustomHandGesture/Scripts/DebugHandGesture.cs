// Copyright HTC Corporation All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if DEFINE_VIVE_OPENXR
using VIVE.OpenXR.Hand;
using VIVE.OpenXR.Toolkits.CustomGesture;
#endif


namespace VIVE.OpenXR.StarterSample.CustomHandGesture
{
    public class DebugHandGesture : MonoBehaviour
    {
        public Text[] LFingers;
        public Text[] RFingers;
        public Text CurrentGestureL;
        public Text CurrentGestureR;
        public Text CurrentGestureDual;
#if DEFINE_VIVE_OPENXR
        CustomGestureManager HGM;
        CustomGestureDefiner GD;
#endif
        void Start()
        {
#if DEFINE_VIVE_OPENXR
            GD = GetComponent<CustomGestureDefiner>();
            HGM = GetComponent<CustomGestureManager>();
#endif
        }

        void Update()
        {
#if DEFINE_VIVE_OPENXR
            UpdateFingerStatus(CGEnums.HandFlag.Left); //get real left hand finger status
            UpdateFingerStatus(CGEnums.HandFlag.Right); //get real right hand finger status
            ShowCurrentGesture();
#endif
        }

        void ShowCurrentGesture()
        {
            CurrentGestureL.text = "LGesture: " + "No Gesture";
            CurrentGestureR.text = "RGesture: " + "No Gesture";
            CurrentGestureDual.text = "DualGesture: " + "No Gesture";

            if (!IsGestureReady())
                return;
#if DEFINE_VIVE_OPENXR
            foreach (CustomGesture _Gestures in GD.DefinedGestures)
            {
                switch (_Gestures.TargetHand)
                {
                    case CGEnums.HandFlag.Either:
                        //check left hand gesture
                        if (CustomGestureDefiner.IsCurrentGestureTriiggered(_Gestures.GestureName, CGEnums.HandFlag.Left) && CheckHandValid(CGEnums.HandFlag.Left))
                            CurrentGestureL.text = "LGesture: " + _Gestures.GestureName;
                        //check right hand gesture
                        if (CustomGestureDefiner.IsCurrentGestureTriiggered(_Gestures.GestureName, CGEnums.HandFlag.Right) && CheckHandValid(CGEnums.HandFlag.Right))
                            CurrentGestureR.text = "RGesture: " + _Gestures.GestureName;
                        //Debug.Log("DebugHandGesture ShowCurrentGesture()  " + _Gestures.GestureName);
                        break;
                    case CGEnums.HandFlag.Dual:
                        if (CustomGestureDefiner.IsCurrentGestureTriiggered(_Gestures.GestureName, CGEnums.HandFlag.Dual) && CheckHandValid(CGEnums.HandFlag.Left) && CheckHandValid(CGEnums.HandFlag.Right))
                        {
                            CurrentGestureDual.text = "DualGesture: " + _Gestures.GestureName;
                        }
                        break;
                    default:
                        CurrentGestureL.text = "LGesture: " + "No Gesture";
                        CurrentGestureR.text = "RGesture: " + "No Gesture";
                        CurrentGestureDual.text = "DualGesture: " + "No Gesture";
                        break;
                }
            }
#endif

        }
#if DEFINE_VIVE_OPENXR
        void UpdateFingerStatus(CGEnums.HandFlag _Hand)
        {
            Text[] _Fingers = (_Hand == CGEnums.HandFlag.Left) ? LFingers : RFingers;
            _Fingers[0].text = HGM.GetFingerStatus(_Hand, CGEnums.FingerFlag.Thumb).ToString();
            _Fingers[1].text = HGM.GetFingerStatus(_Hand, CGEnums.FingerFlag.Index).ToString();
            _Fingers[2].text = HGM.GetFingerStatus(_Hand, CGEnums.FingerFlag.Middle).ToString();
            _Fingers[3].text = HGM.GetFingerStatus(_Hand, CGEnums.FingerFlag.Ring).ToString();
            _Fingers[4].text = HGM.GetFingerStatus(_Hand, CGEnums.FingerFlag.Pinky).ToString();
        }


        bool CheckHandValid(CGEnums.HandFlag _Hand)
        {
            HandJoint[] _Joints = CustomGestureManager.GetHandJointLocations(_Hand);


            if (!(_Joints[(int)XrHandJointEXT.XR_HAND_JOINT_PALM_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_WRIST_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_TIP_EXT].isValid &&
                _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_PROXIMAL_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_DISTAL_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_METACARPAL_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_PROXIMAL_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_INTERMEDIATE_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_DISTAL_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_MIDDLE_METACARPAL_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_MIDDLE_PROXIMAL_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_MIDDLE_INTERMEDIATE_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_MIDDLE_DISTAL_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_MIDDLE_TIP_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_RING_METACARPAL_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_RING_PROXIMAL_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_RING_INTERMEDIATE_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_RING_DISTAL_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_RING_TIP_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_LITTLE_METACARPAL_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_LITTLE_PROXIMAL_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_LITTLE_INTERMEDIATE_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_LITTLE_DISTAL_EXT].isValid &&
                 _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_LITTLE_TIP_EXT].isValid))
            {
                //Debug.Log("DebugHandGesture CheckHandValid() not valid hand: "+ _Hand);
                return false;
            }
            //Debug.Log("DebugHandGesture CheckHandValid() valid hand: " + _Hand);
            return true;
        }
#endif

        bool IsGestureReady()
        {
#if DEFINE_VIVE_OPENXR
            HandJoint[] _JointsL = CustomGestureManager.GetHandJointLocations(CGEnums.HandFlag.Left);
            HandJoint[] _JointsR = CustomGestureManager.GetHandJointLocations(CGEnums.HandFlag.Right);

            if (_JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_PALM_EXT].position.x == 0 &&
                _JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_PALM_EXT].position.y == 0 &&
                _JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_PALM_EXT].position.z == 0 &&
                _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_PALM_EXT].position.x == 0 &&
                _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_PALM_EXT].position.y == 0 &&
                _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_PALM_EXT].position.z == 0)
            {
                //Debug.Log("IsGestureReady left palm before: " + _JointsL[0].position.x + ", " + _JointsL[0].position.y + ", " + _JointsL[0].position.z);
                //Debug.Log("IsGestureReady right palm before: " + _JointsR[0].position.x + ", " + _JointsR[0].position.y + ", " + _JointsR[0].position.z);
                //Debug.Log("IsGestureReady left wrist before: " + _JointsL[1].position.x + ", " + _JointsL[1].position.y + ", " + _JointsL[1].position.z);
                //Debug.Log("IsGestureReady right wrist before: " + _JointsR[1].position.x + ", " + _JointsR[1].position.y + ", " + _JointsR[1].position.z);
                //Debug.Log("DebugHandGesture IsGestureReady() not ready");
                return false;
            }
#endif
            return true;

        }


    }
}

