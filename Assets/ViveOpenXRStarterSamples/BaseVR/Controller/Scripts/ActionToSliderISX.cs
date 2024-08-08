// Copyright HTC Corporation All Rights Reserved.

using UnityEngine.UI;
using UnityEngine;
using UnityEngine.XR;
#if DEFINE_VIVE_OPENXR
using UnityEngine.InputSystem;
#endif

namespace VIVE.OpenXR.StarterSample.ControllerSample
{
    public class ActionToSliderISX : MonoBehaviour
    {
#if DEFINE_VIVE_OPENXR
        [SerializeField]
        private InputActionReference m_ActionReference;
        public InputActionReference actionReference { get => m_ActionReference; set => m_ActionReference = value; }
#endif
        [SerializeField]
        Slider slider = null;

        Graphic graphic = null;
        Graphic[] graphics = new Graphic[]{ };

        private void OnEnable()
        {
            if (slider == null)
                Debug.LogWarning("ActionToSlider Monobehaviour started without any associated slider. This input will not be reported.", this);

            graphic = gameObject.GetComponent<Graphic>();
            graphics = gameObject.GetComponentsInChildren<Graphic>();
        }

        void Update()
        {
#if DEFINE_VIVE_OPENXR
            if (actionReference != null && actionReference.action != null && slider != null)
            {
                if (actionReference.action.enabled)
                {
                    SetVisible(true);
                }

                float value = actionReference.action.ReadValue<float>();
                slider.value = value;
            }
            else
            {
                SetVisible(false);
            }
#endif
        }

        void SetVisible(bool visible)
        {
            if (graphic != null)
                graphic.enabled = visible;

            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].enabled = visible;
            }
        }
    }
}