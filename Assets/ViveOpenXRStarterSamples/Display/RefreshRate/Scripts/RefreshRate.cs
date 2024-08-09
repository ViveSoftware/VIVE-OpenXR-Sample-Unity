// Copyright HTC Corporation All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace VIVE.OpenXR.StarterSample.RefreshRate
{
    public class RefreshRate : MonoBehaviour
    {
        public float originalRefreshRate;
        public float requestRefreshRate = 90.0f;
        public float currentRefreshRate;

        public Text original;
        public Text request;
        public Text current;

        // Start is called before the first frame update
        void Start()
        {
            originalRefreshRate = GetRefreshRate();
            original.text = "Original RefreshRate:" + originalRefreshRate.ToString();
#if DEFINE_VIVE_OPENXR
            XR_FB_display_refresh_rate.RequestDisplayRefreshRate(requestRefreshRate);
            request.text = "Request RefreshRate:" + requestRefreshRate.ToString();
#endif
        }

        // Update is called once per frame
        void Update()
        {
            currentRefreshRate = GetRefreshRate();
            current.text = "Current RefreshRate:" + currentRefreshRate.ToString();
        }

        public void RequestRefreshRate(int num)
        {
#if DEFINE_VIVE_OPENXR
            XR_FB_display_refresh_rate.RequestDisplayRefreshRate(num);
            request.text = "Request RefreshRate:" + num.ToString();
#endif
        }


        private float GetRefreshRate()
        {
#if DEFINE_VIVE_OPENXR
            if (XR_FB_display_refresh_rate.GetDisplayRefreshRate(out float rate) == XrResult.XR_SUCCESS) { return rate; }
#endif
            return 0.0f;
        }
    }
}
