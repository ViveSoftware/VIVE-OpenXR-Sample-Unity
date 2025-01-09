// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;

using VIVE.OpenXR.Passthrough;


namespace VIVE.OpenXR.CompositionLayer.Samples.Passthrough
{
    public class PassthroughSetting : MonoBehaviour
    {
        private OpenXR.Passthrough.XrPassthroughHTC activePassthroughID = 0;
        private LayerType currentActiveLayerType = LayerType.Underlay;

        private void Start()
        {
            StartPassthrough();
        }

        private void Update()
        {

        }

        public void SetPassthroughToOverlay()
        {
            if (activePassthroughID != 0)
            {
                PassthroughAPI.SetPassthroughLayerType(activePassthroughID, LayerType.Overlay);
                currentActiveLayerType = LayerType.Overlay;
            }
        }

        public void SetPassthroughToUnderlay()
        {
            if (activePassthroughID != 0)
            {
                PassthroughAPI.SetPassthroughLayerType(activePassthroughID, LayerType.Underlay);
                currentActiveLayerType = LayerType.Underlay;
            }
        }

        void StartPassthrough()
        {
            PassthroughAPI.CreatePlanarPassthrough(out activePassthroughID, currentActiveLayerType, OnDestroyPassthroughFeatureSession);
        }

        void OnDestroyPassthroughFeatureSession(OpenXR.Passthrough.XrPassthroughHTC passthroughID)
        {
            PassthroughAPI.DestroyPassthrough(passthroughID);
            activePassthroughID = 0;
        }
    }
}
