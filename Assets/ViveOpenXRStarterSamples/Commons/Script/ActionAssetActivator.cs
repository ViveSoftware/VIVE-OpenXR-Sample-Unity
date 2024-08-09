// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
#if DEFINE_VIVE_OPENXR
using UnityEngine.InputSystem;
#endif

namespace VIVE.OpenXR.Samples
{
    public class ActionAssetActivator : MonoBehaviour
    {
#if DEFINE_VIVE_OPENXR
        [SerializeField]
        InputActionAsset m_ActionAsset;
        public InputActionAsset actionAsset
        {
            get => m_ActionAsset;
            set => m_ActionAsset = value;
        }

        private void OnEnable()
        {
            if (m_ActionAsset != null)
            {
                m_ActionAsset.Enable();
            }
        }
#endif
    }
}
