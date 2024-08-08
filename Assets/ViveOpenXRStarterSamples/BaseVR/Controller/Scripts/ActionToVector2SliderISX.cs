// Copyright HTC Corporation All Rights Reserved.

using UnityEngine.UI;
using UnityEngine;
using UnityEngine.XR;

#if DEFINE_VIVE_OPENXR
using UnityEngine.InputSystem;
#endif

namespace VIVE.OpenXR.StarterSample.ControllerSample
{
    public class ActionToVector2SliderISX : MonoBehaviour
    {
#if DEFINE_VIVE_OPENXR
        [SerializeField]
        private InputActionReference m_ActionReference;
        public InputActionReference actionReference { get => m_ActionReference; set => m_ActionReference = value; }
#endif
        [SerializeField]
        public Slider xAxisSlider = null;

        [SerializeField]
        public Slider yAxisSlider = null;


        private void OnEnable()
        {
            if (xAxisSlider == null)
                Debug.LogWarning("ActionToSlider Monobehaviour started without any associated X-axis slider.  This input won't be reported.", this);

            if (yAxisSlider == null)
                Debug.LogWarning("ActionToSlider Monobehaviour started without any associated Y-axis slider.  This input won't be reported.", this);

        }
        void Update()
        {

#if DEFINE_VIVE_OPENXR
            if (actionReference != null && actionReference.action != null && xAxisSlider != null && yAxisSlider != null)
            {
                if (actionReference.action.enabled)
                {
                    SetVisible(gameObject, true);
                }

                Vector2 value = actionReference.action.ReadValue<Vector2>();
                xAxisSlider.value = value.x;
                yAxisSlider.value = value.y;
            }
            else
            {
                SetVisible(gameObject, false);
            }
#endif
        }

        void SetVisible(GameObject go, bool visible)
        {
            Graphic graphic = go.GetComponent<Graphic>();
            if (graphic != null)
                graphic.enabled = visible;

            Graphic[] graphics = go.GetComponentsInChildren<Graphic>();
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].enabled = visible;
            }
        }
    }
}
