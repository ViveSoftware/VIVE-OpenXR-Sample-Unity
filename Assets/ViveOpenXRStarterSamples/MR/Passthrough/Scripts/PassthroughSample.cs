// Copyright HTC Corporation All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if DEFINE_VIVE_OPENXR
using VIVE.OpenXR.CompositionLayer.Passthrough;
using VIVE.OpenXR.CompositionLayer;
# endif

namespace VIVE.OpenXR.StarterSample.Passthrough {
    public class PassthroughSample : MonoBehaviour
    {
#if DEFINE_VIVE_OPENXR
        private LayerType currentActiveLayerType = LayerType.Underlay;
        private ProjectedPassthroughSpaceType currentActiveSpaceType = ProjectedPassthroughSpaceType.Worldlock;
#endif
        private int activePassthroughID = 0;
        public Mesh passthroughMesh = null;
        public Transform passthroughMeshTransform = null;
        public GameObject hmd = null;

        public Toggle m_Toggle_Planar;
        public Toggle m_Toggle_Projected;
        public Text m_Text;
        // Start is called before the first frame update
        void Start()
        {
            if (hmd == null) hmd = Camera.main.gameObject;
            m_Toggle_Planar = GetComponent<Toggle>();
            m_Toggle_Projected = GetComponent<Toggle>();

        }

        // Update is called once per frame
        void Update()
        {
#if DEFINE_VIVE_OPENXR
            if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.B)) //Set Passthrough as Overlay
            {
                SetPassthroughToOverlay();
            }
            if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.A)) //Set Passthrough as Underlay
            {
                SetPassthroughToUnderlay();
            }
#endif

        }
        public void ToggleValueChanged_Planar(bool changeValue)
        {

            if (changeValue == true)
            {
                StartPlanarPassthrough();
            }
            else if (changeValue == false)
            {
                btnDisable();
            }
        }
        public void ToggleValueChanged_Projected(bool changeValue)
        {

            if (changeValue == true)
            {
                StartProjectedPassthrough();
            }
            else if (changeValue == false)
            {
                btnDisable();
            }
        }

        public void StartProjectedPassthrough()
        {
#if DEFINE_VIVE_OPENXR
            activePassthroughID = CompositionLayerPassthroughAPI.CreateProjectedPassthrough(currentActiveLayerType, OnDestroyPassthroughFeatureSession);
            SetPassthroughMesh(activePassthroughID);
#endif
        }

        void SetPassthroughMesh(int passthroughID)
        {
#if DEFINE_VIVE_OPENXR
            CompositionLayerPassthroughAPI.SetProjectedPassthroughMesh(passthroughID, passthroughMesh.vertices, passthroughMesh.triangles);

            switch (currentActiveSpaceType)
            {
                case ProjectedPassthroughSpaceType.Headlock: //Apply HMD offset
                    Vector3 relativePosition = hmd.transform.InverseTransformPoint(passthroughMeshTransform.transform.position);
                    Quaternion relativeRotation = Quaternion.Inverse(hmd.transform.rotation).normalized * passthroughMeshTransform.transform.rotation.normalized;
                    CompositionLayerPassthroughAPI.SetProjectedPassthroughMeshTransform(passthroughID, currentActiveSpaceType, relativePosition, relativeRotation, new Vector3(1, 1, 1), false);
                    break;
                case ProjectedPassthroughSpaceType.Worldlock:
                default:
                    CompositionLayerPassthroughAPI.SetProjectedPassthroughMeshTransform(passthroughID, currentActiveSpaceType, passthroughMeshTransform.transform.position, passthroughMeshTransform.transform.rotation, new Vector3(1, 1, 1));
                    break;
            }
#endif

        }
        public void SetPassthroughToOverlay()
        {
#if DEFINE_VIVE_OPENXR
            CompositionLayerPassthroughAPI.SetPassthroughLayerType(activePassthroughID, LayerType.Overlay);
            currentActiveLayerType = LayerType.Overlay;
#endif
        }

        public void SetPassthroughToUnderlay()
        {
#if DEFINE_VIVE_OPENXR
            CompositionLayerPassthroughAPI.SetPassthroughLayerType(activePassthroughID, LayerType.Underlay);
            currentActiveLayerType = LayerType.Underlay;
#endif

        }


        public void StartPlanarPassthrough()
        {
#if DEFINE_VIVE_OPENXR
            activePassthroughID = CompositionLayerPassthroughAPI.CreatePlanarPassthrough(currentActiveLayerType, OnDestroyPassthroughFeatureSession);
#endif
        }

        public void btnDisable()
        {
#if DEFINE_VIVE_OPENXR
            if (activePassthroughID != 0)
            {
                CompositionLayerPassthroughAPI.DestroyPassthrough(activePassthroughID);
                activePassthroughID = 0;
            }
#endif
        }

        public void OnDestroyPassthroughFeatureSession(int passthroughID)
        {
#if DEFINE_VIVE_OPENXR
            CompositionLayerPassthroughAPI.DestroyPassthrough(passthroughID);
            activePassthroughID = 0;
#endif
        }
    }
}
