﻿using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace com.HTC.WVRLoader
{
    public static class WVRUtility
    {
#if UNITY_EDITOR
        private const bool USE_WVR = false;
#else
        private const bool USE_WVR = true;
#endif

        public const string INVALID_RESULT = "unknown";

        private const uint MAX_RESULT_BUFFER_SIZE = 2560;

        public static string WVR_GetParameterDynamicSize(string encodedParam)
        {
            if (!USE_WVR)
            {
                TextAsset asset = Resources.Load<TextAsset>("SampleSceneComponents");
                if (asset != null)
                {
                    string text = asset.text;
                    Resources.UnloadAsset(asset);
                    return text;
                }
                else
                {
                    return null;
                }
            }

            Debug.LogFormat("Pass command: {0}", encodedParam);

            IntPtr ptrParam = Marshal.StringToHGlobalAnsi(encodedParam);
            string result = string.Empty;

            uint return_str_length = 0;
            Debug.LogFormat("Result str length = {0}", return_str_length);

            if (return_str_length > 0)
            {
                IntPtr ptrResult = Marshal.AllocHGlobal(new IntPtr(return_str_length));

                return_str_length = 0;

                result = Marshal.PtrToStringAnsi(ptrResult);
                Debug.LogFormat("Result str = {0}", result);

                Marshal.FreeHGlobal(ptrResult);
            }

            Marshal.FreeHGlobal(ptrParam);
            return result;
        }

        public static string WVR_GetParameter(string encodedParam)
        {
            if (!USE_WVR)
            {
                return null;
            }

            Debug.LogFormat("Pass command: {0}", encodedParam);

            IntPtr ptrParam = Marshal.StringToHGlobalAnsi(encodedParam);
            IntPtr ptrResult = Marshal.AllocHGlobal(new IntPtr(MAX_RESULT_BUFFER_SIZE));
            string result = string.Empty;

            uint return_str_length = 0;
            Debug.LogFormat("Result str length = {0}", return_str_length);

            if (return_str_length > 0)
            {
                result = Marshal.PtrToStringAnsi(ptrResult);
                Debug.LogFormat("Result str = {0}", result);
            }

            Marshal.FreeHGlobal(ptrResult);
            Marshal.FreeHGlobal(ptrParam);

            return result;
        }

        public static void WVR_SetParameter(string encodedParam)
        {
            if (!USE_WVR)
            {
                return;
            }

            IntPtr ptrParam = Marshal.StringToHGlobalAnsi(encodedParam);
            Marshal.FreeHGlobal(ptrParam);
        }


    }
}
