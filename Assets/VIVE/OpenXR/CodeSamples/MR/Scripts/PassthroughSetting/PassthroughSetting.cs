using UnityEngine;
using VIVE.OpenXR.CompositionLayer;
using VIVE.OpenXR.Passthrough;

namespace com.HTC.Common
{
    public class PassthroughSetting : MonoBehaviour
    {
        private VIVE.OpenXR.Passthrough.XrPassthroughHTC activePassthroughID = 0;
        private LayerType currentActiveLayerType = LayerType.Underlay;

        private void OnEnable()
        {
#if !UNITY_EDITOR
             PassthroughAPI.SetPassthroughLayerType(activePassthroughID, LayerType.Underlay);
             currentActiveLayerType = LayerType.Underlay;
            PassthroughAPI.CreatePlanarPassthrough(out activePassthroughID, currentActiveLayerType, OnDestroyPassthroughFeatureSession);
#endif
        }

        private void OnDisable()
        {
#if !UNITY_EDITOR
            PassthroughAPI.DestroyPassthrough(activePassthroughID);
            activePassthroughID = 0;
            //PassthroughUtils.DisablePassThrough();
#endif
        }

        private void OnApplicationPause(bool isPaused)
        {
#if !UNITY_EDITOR
            if (isPaused)
            {
                PassthroughAPI.DestroyPassthrough(activePassthroughID);
                activePassthroughID = 0;
            }
            else
            {
                PassthroughAPI.SetPassthroughLayerType(activePassthroughID, LayerType.Underlay);
                currentActiveLayerType = LayerType.Underlay;
                PassthroughAPI.CreatePlanarPassthrough(out activePassthroughID, currentActiveLayerType, OnDestroyPassthroughFeatureSession);
            }
#endif
        }
        void OnDestroyPassthroughFeatureSession(VIVE.OpenXR.Passthrough.XrPassthroughHTC passthroughID)
        {
            PassthroughAPI.DestroyPassthrough(passthroughID);
            activePassthroughID = 0;
        }

    }
}