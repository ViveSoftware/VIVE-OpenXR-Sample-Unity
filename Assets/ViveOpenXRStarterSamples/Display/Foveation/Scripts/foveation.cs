// Copyright HTC Corporation All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace VIVE.OpenXR.StarterSample.Foveation
{
    public class foveation : MonoBehaviour
    {

        private float FOVLarge = 34;
        private float FOVSmall = 6;
        private float FOVMiddle = 14;

#if DEFINE_VIVE_OPENXR
        public static XrFoveationConfigurationHTC config_left, config_right;
        public static XrFoveationConfigurationHTC[] configs = { config_left, config_right };
#endif


        foveation()
        {
#if DEFINE_VIVE_OPENXR
            configs[0].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_HIGH_HTC;
            configs[0].clearFovDegree = FOVSmall;
            configs[0].focalCenterOffset.x = 0.0f;
            configs[0].focalCenterOffset.y = 0.0f;
            configs[1].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_HIGH_HTC;
            configs[1].clearFovDegree = FOVSmall;
            configs[1].focalCenterOffset.x = 0.0f;
            configs[1].focalCenterOffset.y = 0.0f;
#endif
        }
        void Start()
        {
            FoveationIsEnable();
            PeripheralQualityHigh();
        }


        public void LeftClearVisionFOVHigh()
        {
#if DEFINE_VIVE_OPENXR
            configs[0].clearFovDegree = FOVLarge;
            configs[1].clearFovDegree = FOVLarge;
            ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
#endif
        }

        public void LeftClearVisionFOVLow()
        {
#if DEFINE_VIVE_OPENXR
            configs[0].clearFovDegree = FOVSmall;
            configs[1].clearFovDegree = FOVSmall;
            ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
#endif
        }

        public void LeftClearVisionFOVMiddle()
        {
#if DEFINE_VIVE_OPENXR
            configs[0].clearFovDegree = FOVMiddle;
            configs[1].clearFovDegree = FOVMiddle;
            ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
#endif
        }

        public void FoveationIsDisable()
        {
#if DEFINE_VIVE_OPENXR
            ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_DISABLE_HTC, 0, null);
#endif
        }

        public void FoveationIsEnable()
        {
#if DEFINE_VIVE_OPENXR
            ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_FIXED_HTC, 0, null);
#endif
        }

        public void FoveationIsDynamic()
        {
#if DEFINE_VIVE_OPENXR
            UInt64 flags = ViveFoveation.XR_FOVEATION_DYNAMIC_CLEAR_FOV_ENABLED_BIT_HTC |
                ViveFoveation.XR_FOVEATION_DYNAMIC_FOCAL_CENTER_OFFSET_ENABLED_BIT_HTC |
                ViveFoveation.XR_FOVEATION_DYNAMIC_LEVEL_ENABLED_BIT_HTC;
            ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_DYNAMIC_HTC, 0, null, flags);
#endif
        }

        public void PeripheralQualityHigh()
        {
#if DEFINE_VIVE_OPENXR
            configs[0].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_HIGH_HTC;
            configs[1].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_HIGH_HTC;
            ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
#endif
        }

        public void PeripheralQualityLow()
        {
#if DEFINE_VIVE_OPENXR
            configs[0].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_LOW_HTC;
            configs[1].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_LOW_HTC;
            ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
#endif
        }

        public void PeripheralQualityMiddle()
        {
#if DEFINE_VIVE_OPENXR
            configs[0].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_MEDIUM_HTC;
            configs[1].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_MEDIUM_HTC;
            ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
#endif
        }


    }
}
