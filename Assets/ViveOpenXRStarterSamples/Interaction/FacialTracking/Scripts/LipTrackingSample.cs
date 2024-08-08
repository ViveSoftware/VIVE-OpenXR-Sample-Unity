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
    public class LipTrackingSample : MonoBehaviour
    {
        public enum XrLipShapeHTC
        {
            XR_LIP_SHAPE_NONE_HTC = -1,
            XR_LIP_SHAPE_JAW_RIGHT_HTC = 0,
            XR_LIP_SHAPE_JAW_LEFT_HTC = 1,
            XR_LIP_SHAPE_JAW_FORWARD_HTC = 2,
            XR_LIP_SHAPE_JAW_OPEN_HTC = 3,
            XR_LIP_SHAPE_MOUTH_APE_SHAPE_HTC = 4,
            XR_LIP_SHAPE_MOUTH_UPPER_RIGHT_HTC = 5,
            XR_LIP_SHAPE_MOUTH_UPPER_LEFT_HTC = 6,
            XR_LIP_SHAPE_MOUTH_LOWER_RIGHT_HTC = 7,
            XR_LIP_SHAPE_MOUTH_LOWER_LEFT_HTC = 8,
            XR_LIP_SHAPE_MOUTH_UPPER_OVERTURN_HTC = 9,
            XR_LIP_SHAPE_MOUTH_LOWER_OVERTURN_HTC = 10,
            XR_LIP_SHAPE_MOUTH_POUT_HTC = 11,
            XR_LIP_SHAPE_MOUTH_SMILE_RIGHT_HTC = 12,
            XR_LIP_SHAPE_MOUTH_SMILE_LEFT_HTC = 13,
            XR_LIP_SHAPE_MOUTH_SAD_RIGHT_HTC = 14,
            XR_LIP_SHAPE_MOUTH_SAD_LEFT_HTC = 15,
            XR_LIP_SHAPE_CHEEK_PUFF_RIGHT_HTC = 16,
            XR_LIP_SHAPE_CHEEK_PUFF_LEFT_HTC = 17,
            XR_LIP_SHAPE_CHEEK_SUCK_HTC = 18,
            XR_LIP_SHAPE_MOUTH_UPPER_UPRIGHT_HTC = 19,
            XR_LIP_SHAPE_MOUTH_UPPER_UPLEFT_HTC = 20,
            XR_LIP_SHAPE_MOUTH_LOWER_DOWNRIGHT_HTC = 21,
            XR_LIP_SHAPE_MOUTH_LOWER_DOWNLEFT_HTC = 22,
            XR_LIP_SHAPE_MOUTH_UPPER_INSIDE_HTC = 23,
            XR_LIP_SHAPE_MOUTH_LOWER_INSIDE_HTC = 24,
            XR_LIP_SHAPE_MOUTH_LOWER_OVERLAY_HTC = 25,
            XR_LIP_SHAPE_TONGUE_LONGSTEP1_HTC = 26,
            XR_LIP_SHAPE_TONGUE_LEFT_HTC = 27,
            XR_LIP_SHAPE_TONGUE_RIGHT_HTC = 28,
            XR_LIP_SHAPE_TONGUE_UP_HTC = 29,
            XR_LIP_SHAPE_TONGUE_DOWN_HTC = 30,
            XR_LIP_SHAPE_TONGUE_ROLL_HTC = 31,
            XR_LIP_SHAPE_TONGUE_LONGSTEP2_HTC = 32,
            XR_LIP_SHAPE_TONGUE_UPRIGHT_MORPH_HTC = 33,
            XR_LIP_SHAPE_TONGUE_UPLEFT_MORPH_HTC = 34,
            XR_LIP_SHAPE_TONGUE_DOWNRIGHT_MORPH_HTC = 35,
            XR_LIP_SHAPE_TONGUE_DOWNLEFT_MORPH_HTC = 36,
            XR_LIP_SHAPE_MAX_ENUM_HTC = 37
        }
        public SkinnedMeshRenderer HeadskinnedMeshRenderer;
        private static float[] blendshapes = new float[60];
        //Map OpenXR lip shape to avatar lip blend shape
        private static Dictionary<XrLipShapeHTC, SkinnedMeshRendererShape> ShapeMap;
        // Start is called before the first frame update



        void Start()
        {
            ShapeMap = new Dictionary<XrLipShapeHTC, SkinnedMeshRendererShape>();
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_JAW_RIGHT_HTC, SkinnedMeshRendererShape.Jaw_Right);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_JAW_LEFT_HTC, SkinnedMeshRendererShape.Jaw_Left);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_JAW_FORWARD_HTC, SkinnedMeshRendererShape.Jaw_Forward);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_JAW_OPEN_HTC, SkinnedMeshRendererShape.Jaw_Open);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_APE_SHAPE_HTC, SkinnedMeshRendererShape.Mouth_Ape_Shape);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_UPPER_RIGHT_HTC, SkinnedMeshRendererShape.Mouth_Upper_Right);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_UPPER_LEFT_HTC, SkinnedMeshRendererShape.Mouth_Upper_Left);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_LOWER_RIGHT_HTC, SkinnedMeshRendererShape.Mouth_Lower_Right);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_LOWER_LEFT_HTC, SkinnedMeshRendererShape.Mouth_Lower_Left);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_UPPER_OVERTURN_HTC, SkinnedMeshRendererShape.Mouth_Upper_Overturn);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_LOWER_OVERTURN_HTC, SkinnedMeshRendererShape.Mouth_Lower_Overturn);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_POUT_HTC, SkinnedMeshRendererShape.Mouth_Pout);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_SMILE_RIGHT_HTC, SkinnedMeshRendererShape.Mouth_Smile_Right);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_SMILE_LEFT_HTC, SkinnedMeshRendererShape.Mouth_Smile_Left);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_SAD_RIGHT_HTC, SkinnedMeshRendererShape.Mouth_Sad_Right);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_SAD_LEFT_HTC, SkinnedMeshRendererShape.Mouth_Sad_Left);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_CHEEK_PUFF_RIGHT_HTC, SkinnedMeshRendererShape.Cheek_Puff_Right);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_CHEEK_PUFF_LEFT_HTC, SkinnedMeshRendererShape.Cheek_Puff_Left);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_CHEEK_SUCK_HTC, SkinnedMeshRendererShape.Cheek_Suck);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_UPPER_UPRIGHT_HTC, SkinnedMeshRendererShape.Mouth_Upper_UpRight);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_UPPER_UPLEFT_HTC, SkinnedMeshRendererShape.Mouth_Upper_UpLeft);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_LOWER_DOWNRIGHT_HTC, SkinnedMeshRendererShape.Mouth_Lower_DownRight);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_LOWER_DOWNLEFT_HTC, SkinnedMeshRendererShape.Mouth_Lower_DownLeft);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_UPPER_INSIDE_HTC, SkinnedMeshRendererShape.Mouth_Upper_Inside);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_LOWER_INSIDE_HTC, SkinnedMeshRendererShape.Mouth_Lower_Inside);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_LOWER_OVERLAY_HTC, SkinnedMeshRendererShape.Mouth_Lower_Overlay);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_TONGUE_LONGSTEP1_HTC, SkinnedMeshRendererShape.Tongue_LongStep1);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_TONGUE_LEFT_HTC, SkinnedMeshRendererShape.Tongue_Left);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_TONGUE_RIGHT_HTC, SkinnedMeshRendererShape.Tongue_Right);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_TONGUE_UP_HTC, SkinnedMeshRendererShape.Tongue_Up);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_TONGUE_DOWN_HTC, SkinnedMeshRendererShape.Tongue_Down);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_TONGUE_ROLL_HTC, SkinnedMeshRendererShape.Tongue_Roll);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_TONGUE_LONGSTEP2_HTC, SkinnedMeshRendererShape.Tongue_LongStep2);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_TONGUE_UPRIGHT_MORPH_HTC, SkinnedMeshRendererShape.Tongue_UpRight_Morph);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_TONGUE_UPLEFT_MORPH_HTC, SkinnedMeshRendererShape.Tongue_UpLeft_Morph);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_TONGUE_DOWNRIGHT_MORPH_HTC, SkinnedMeshRendererShape.Tongue_DownRight_Morph);
            ShapeMap.Add(XrLipShapeHTC.XR_LIP_SHAPE_TONGUE_DOWNLEFT_MORPH_HTC, SkinnedMeshRendererShape.Tongue_DownLeft_Morph);

        }

        // Update is called once per frame
        void Update()
        {
#if DEFINE_VIVE_OPENXR
            var feature = OpenXRSettings.Instance.GetFeature<ViveFacialTracking>();
            feature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, out blendshapes);


            for (XrLipShapeHTC i = XrLipShapeHTC.XR_LIP_SHAPE_JAW_RIGHT_HTC; i < XrLipShapeHTC.XR_LIP_SHAPE_MAX_ENUM_HTC; i++)
            {
                HeadskinnedMeshRenderer.SetBlendShapeWeight((int)ShapeMap[i], blendshapes[(int)i] * 100f);
            }
#endif

        }
    }
}
