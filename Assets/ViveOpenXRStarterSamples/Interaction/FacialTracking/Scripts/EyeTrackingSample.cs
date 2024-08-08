// Copyright HTC Corporation All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using UnityEngine.XR.OpenXR;
#if DEFINE_VIVE_OPENXR
using VIVE.OpenXR.FacialTracking;
#endif

namespace VIVE.OpenXR.StarterSample.FacialTracking
{
    public class EyeTrackingSample : MonoBehaviour
    {
        public enum XrEyeShapeHTC
        {
            XR_EYE_SHAPE_NONE_HTC = -1,
            XR_EYE_EXPRESSION_LEFT_BLINK_HTC = 0,
            XR_EYE_EXPRESSION_LEFT_WIDE_HTC = 1,
            XR_EYE_EXPRESSION_RIGHT_BLINK_HTC = 2,
            XR_EYE_EXPRESSION_RIGHT_WIDE_HTC = 3,
            XR_EYE_EXPRESSION_LEFT_SQUEEZE_HTC = 4,
            XR_EYE_EXPRESSION_RIGHT_SQUEEZE_HTC = 5,
            XR_EYE_EXPRESSION_LEFT_DOWN_HTC = 6,
            XR_EYE_EXPRESSION_RIGHT_DOWN_HTC = 7,
            XR_EYE_EXPRESSION_LEFT_OUT_HTC = 8,
            XR_EYE_EXPRESSION_RIGHT_IN_HTC = 9,
            XR_EYE_EXPRESSION_LEFT_IN_HTC = 10,
            XR_EYE_EXPRESSION_RIGHT_OUT_HTC = 11,
            XR_EYE_EXPRESSION_LEFT_UP_HTC = 12,
            XR_EYE_EXPRESSION_RIGHT_UP_HTC = 13,
            XR_EYE_EXPRESSION_MAX_ENUM_HTC = 14,
        }

        private static Dictionary<XrEyeShapeHTC, SkinnedMeshRendererShape> ShapeMap;
        public SkinnedMeshRenderer HeadskinnedMeshRenderer;
        private static float[] blendshapes = new float[60];
        // Start is called before the first frame update

        public GameObject leftEye;
        public GameObject rightEye;
        private GameObject[] EyeAnchors;

        void Start()
        {
            ShapeMap = new Dictionary<XrEyeShapeHTC, SkinnedMeshRendererShape>();
            ShapeMap.Add(XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_BLINK_HTC, SkinnedMeshRendererShape.Eye_Left_Blink);
            ShapeMap.Add(XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_WIDE_HTC, SkinnedMeshRendererShape.Eye_Left_Wide);
            ShapeMap.Add(XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_BLINK_HTC, SkinnedMeshRendererShape.Eye_Right_Blink);
            ShapeMap.Add(XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_WIDE_HTC, SkinnedMeshRendererShape.Eye_Right_Wide);
            ShapeMap.Add(XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_SQUEEZE_HTC, SkinnedMeshRendererShape.Eye_Left_Squeeze);
            ShapeMap.Add(XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_SQUEEZE_HTC, SkinnedMeshRendererShape.Eye_Right_Squeeze);
            ShapeMap.Add(XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_DOWN_HTC, SkinnedMeshRendererShape.Eye_Left_Down);
            ShapeMap.Add(XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_DOWN_HTC, SkinnedMeshRendererShape.Eye_Right_Down);
            ShapeMap.Add(XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_OUT_HTC, SkinnedMeshRendererShape.Eye_Left_Left);
            ShapeMap.Add(XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_IN_HTC, SkinnedMeshRendererShape.Eye_Right_Left);
            ShapeMap.Add(XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_IN_HTC, SkinnedMeshRendererShape.Eye_Left_Right);
            ShapeMap.Add(XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_OUT_HTC, SkinnedMeshRendererShape.Eye_Right_Right);
            ShapeMap.Add(XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_UP_HTC, SkinnedMeshRendererShape.Eye_Left_Up);
            ShapeMap.Add(XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_UP_HTC, SkinnedMeshRendererShape.Eye_Right_Up);

            EyeAnchors = new GameObject[2];
            EyeAnchors[0] = new GameObject();
            EyeAnchors[0].name = "EyeAnchor_" + 0;
            EyeAnchors[0].transform.SetParent(gameObject.transform);
            EyeAnchors[0].transform.localPosition = leftEye.transform.localPosition;
            EyeAnchors[0].transform.localRotation = leftEye.transform.localRotation;
            EyeAnchors[0].transform.localScale = leftEye.transform.localScale;
            EyeAnchors[1] = new GameObject();
            EyeAnchors[1].name = "EyeAnchor_" + 1;
            EyeAnchors[1].transform.SetParent(gameObject.transform);
            EyeAnchors[1].transform.localPosition = rightEye.transform.localPosition;
            EyeAnchors[1].transform.localRotation = rightEye.transform.localRotation;
            EyeAnchors[1].transform.localScale = rightEye.transform.localScale;
        }

        // Update is called once per frame
        void Update()
        {
#if DEFINE_VIVE_OPENXR
            var feature = OpenXRSettings.Instance.GetFeature<ViveFacialTracking>();
            feature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC, out blendshapes);

            for (XrEyeShapeHTC i = XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_BLINK_HTC; i < XrEyeShapeHTC.XR_EYE_EXPRESSION_MAX_ENUM_HTC; i++)
            {
                HeadskinnedMeshRenderer.SetBlendShapeWeight((int)ShapeMap[i], blendshapes[(int)i] * 100f);
            }

            Vector3 GazeDirectionCombinedLocal = Vector3.zero;
            if (blendshapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_IN_HTC] > blendshapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_OUT_HTC])
            {
                GazeDirectionCombinedLocal.x = -blendshapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_IN_HTC];
            }
            else
            {
                GazeDirectionCombinedLocal.x = blendshapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_OUT_HTC];
            }
            if (blendshapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_UP_HTC] > blendshapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_WIDE_HTC])
            {
                GazeDirectionCombinedLocal.y = blendshapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_UP_HTC];
            }
            else
            {
                GazeDirectionCombinedLocal.y = -blendshapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_WIDE_HTC];
            }
            GazeDirectionCombinedLocal.z = (float)-1.0;
            Vector3 target = EyeAnchors[0].transform.TransformPoint(GazeDirectionCombinedLocal);
            leftEye.transform.LookAt(target);

            if (blendshapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_IN_HTC] > blendshapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_UP_HTC])
            {
                GazeDirectionCombinedLocal.x = blendshapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_IN_HTC];
            }
            else
            {
                GazeDirectionCombinedLocal.x = -blendshapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_UP_HTC];
            }
            if (blendshapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_UP_HTC] > blendshapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_DOWN_HTC])
            {
                GazeDirectionCombinedLocal.y = -blendshapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_UP_HTC];
            }
            else
            {
                GazeDirectionCombinedLocal.y = blendshapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_DOWN_HTC];
            }
            GazeDirectionCombinedLocal.z = (float)-1.0;
            target = EyeAnchors[1].transform.TransformPoint(GazeDirectionCombinedLocal);
            rightEye.transform.LookAt(target);
#endif
        }
    }
}
