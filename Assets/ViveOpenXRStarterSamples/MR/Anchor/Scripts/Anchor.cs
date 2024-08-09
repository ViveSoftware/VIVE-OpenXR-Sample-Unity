// Copyright HTC Corporation All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
#if DEFINE_VIVE_OPENXR
using VIVE.OpenXR.Toolkits.Anchor;
#endif

namespace VIVE.OpenXR.StarterSample.Anchor
{
    public class Anchor : MonoBehaviour
    {

        public Transform rig;
        public Transform anchorPoseD;
        public Transform anchorPose1;
        public Transform obj;
#if DEFINE_VIVE_OPENXR
        public AnchorManager.Anchor anchor1;
#endif
        public Text status;

        public Pose GetRelatedPoseToRig(Transform t)
        {
            return new Pose(rig.InverseTransformPoint(t.position), Quaternion.Inverse(rig.rotation) * t.rotation);
        }
#if DEFINE_VIVE_OPENXR
        AnchorManager.Anchor CreateAnchor(Pose relatedPose, string name)
        {
            if (!AnchorManager.IsSupported())
            {
                Debug.LogError("AnchorManager is not supported.");
                status.text = "AnchorManager is not supported.";
                return null;
            }
            var anchor = AnchorManager.CreateAnchor(relatedPose, name + " (" + Time.frameCount + ")");
            if (anchor == null)
            {
                status.text = "Create " + name + " failed";
                Debug.LogError("Create " + name + " failed");
                // Even error, still got.  Use fake data.
                return anchor;
            }
            else
            {
                string msg = "Create Anchor n=" + anchor.Name + " space=" + anchor.GetXrSpace() + " at p=" + relatedPose.position + " & r=" + relatedPose.rotation.eulerAngles;
                status.text = msg;
                Debug.Log(msg);
                return anchor;
            }
        }

        public void OnCreateAnchor1()
        {
            if (anchor1 != null)
            {
                anchor1.Dispose();
                anchor1 = null;
            }
            anchor1 = CreateAnchor(GetRelatedPoseToRig(anchorPose1), "anchor1");
        }

        public void MoveObjToAnchor(AnchorManager.Anchor anchor)
        {
            if (!AnchorManager.IsSupported())
                return;

            if (anchor == null)
            {
                status.text = "anchor is null";
                return;
            }

            if (AnchorManager.GetTrackingSpacePose(anchor, out Pose pose))
            {
                // Convert tracking space pose to world space pose
                obj.position = rig.TransformPoint(pose.position);
                obj.rotation = rig.rotation * pose.rotation;

                status.text = "Obj move to " + anchor.GetSpatialAnchorName();
            }
            else
            {
                status.text = "Fail to get anchor's pose";
            }
        }

        public void OnFollowAnchor1()
        {
            MoveObjToAnchor(anchor1);
        }

#endif

        public void OnResetObj()
        {
            obj.position = anchorPoseD.position;
            obj.rotation = anchorPoseD.rotation;

            status.text = "Obj move to default pose";
        }



    }
}
