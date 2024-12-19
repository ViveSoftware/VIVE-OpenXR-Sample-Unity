using UnityEngine;
using VIVE.OpenXR;
using VIVE.OpenXR.CompositionLayer;
using VIVE.OpenXR.Passthrough;

namespace com.HTC.Common
{
    public static class PassthroughUtils
    {
        private static VIVE.OpenXR.Passthrough.XrPassthroughHTC activePassthroughID;
        private const string TAG = nameof(PassthroughUtils);
        private static Color originColor;

        private static Camera _MainCamera = null;
        private static Camera MainCamera
        {
            get
            {
                if (_MainCamera == null)
                {
                    _MainCamera = Camera.main;
                }

                return _MainCamera;
            }
        }

        public static bool EnablePassThrough()
        {
            MainCamera.clearFlags = CameraClearFlags.SolidColor;
            originColor = MainCamera.backgroundColor;
            MainCamera.backgroundColor = Color.white * 0;

            return WVR_enablePassthrough(true);
        }

        public static void DisablePassThrough()
        {
            MainCamera.clearFlags = CameraClearFlags.Skybox;
            MainCamera.backgroundColor = originColor;

            WVR_enablePassthrough(false);
        }

        private static bool WVR_enablePassthrough(bool enable)
        {
            Debug.Log($"{TAG}: Set passthrough {enable}");

            if (enable)
            {
                XrResult result = PassthroughAPI.CreatePlanarPassthrough(out activePassthroughID, LayerType.Underlay);
                if (result != XrResult.XR_SUCCESS)
                    Debug.Log($"{TAG}: WVR_ShowPassthroughUnderlay(true) failed: {result.ToString()}");
                return result == XrResult.XR_SUCCESS;
            }
            else
            {
                XrResult result = PassthroughAPI.DestroyPassthrough(activePassthroughID);
                if (result != XrResult.XR_SUCCESS)
                    Debug.Log($"{TAG}: WVR_ShowPassthroughUnderlay(false) failed: {result.ToString()}");
            }

            return false;
        }

    }
}
