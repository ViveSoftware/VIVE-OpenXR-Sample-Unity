using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using VIVE.OpenXR;
using VIVE.OpenXR.CompositionLayer;
using VIVE.OpenXR.Passthrough;

using XrFoveationModeHTC = VIVE.OpenXR.Foveation.XrFoveationModeHTC;
using XrFoveationLevelHTC = VIVE.OpenXR.Foveation.XrFoveationLevelHTC;
using XrFoveationConfigurationHTC = VIVE.OpenXR.Foveation.XrFoveationConfigurationHTC;

public class PerformanceController : MonoBehaviour
{
    public Button passthroughEnableButton;
    public Button passthroughDisableButton;

    public Button[] imageQualityButtons;
    public Button[] imageRateButtons;
    public Button[] targetFrameRateButtons;
    public Button[] sharpeningModeButtons;
    public Button[] sharpeningLevelButtons;
    public Button[] foveatedModeButtons;
    public Button[] foveatedLevelButtons;

    public Button resetButton;
    public Button startButton;

    private Button selectedFoveatedModeButton;

    // Passthrough Configurations
    private VIVE.OpenXR.Passthrough.XrPassthroughHTC activePassthroughID = 0;
    private LayerType currentActiveLayerType = LayerType.Underlay;

    private static XrPassthroughConfigurationImageRateHTC[] imageRateConfigs = new XrPassthroughConfigurationImageRateHTC[2];
    private static XrPassthroughConfigurationImageQualityHTC imageQualityConfig = new XrPassthroughConfigurationImageQualityHTC();

    // Sharpening
    private static XrSharpeningModeHTC sharpeningMode = XrSharpeningModeHTC.FAST;
    private static float sharpeningLevel = 1.0f;

    // Foveation
    private float FOVLarge = 57;
    private float FOVSmall = 19;
    private float FOVMiddle = 38;

    public static XrFoveationConfigurationHTC[] foveationConfigs = new XrFoveationConfigurationHTC[2];

    public GameObject canvasObj;
    private Canvas canvasObject;

    private void Awake()
    {
        // Initialize Foveation Configurations
        for (int i = 0; i < foveationConfigs.Length; i++)
        {
            foveationConfigs[i] = new XrFoveationConfigurationHTC
            {
                level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_HIGH_HTC,
                clearFovDegree = FOVLarge,
                focalCenterOffset = new XrVector2f { x = 0.0f, y = 0.0f }
            };
        }
    }

    private void Start()
    {
        InitializeCanvas();
        InitializeButtons();
        InitializePassthrough();

        // Default State
        OnPassthroughEnable(true);
        SetRefreshRate(90);
    }

    // Initialize Canvas
    private void InitializeCanvas()
    {
        if (canvasObj != null)
        {
            canvasObject = canvasObj.GetComponent<Canvas>();
            ShowCanvas(true);
        }
        else
        {
            Debug.LogError("CanvasObj is null. Cannot initialize Canvas.");
        }
    }

    // Initialize Button Listeners
    private void InitializeButtons()
    {
        AddListeners(imageQualityButtons, OnImageQualitySelected);
        AddListeners(imageRateButtons, OnImageRateSelected);
        AddListeners(targetFrameRateButtons, OnTargetFrameRateSelected);
        AddListeners(sharpeningModeButtons, OnSharpeningModeSelected);
        AddListeners(sharpeningLevelButtons, OnSharpeningLevelSelected);
        AddListeners(foveatedModeButtons, OnFoveatedModeSelected);
        AddListeners(foveatedLevelButtons, OnFoveatedLevelSelected);

        resetButton.onClick.AddListener(ResetAll);
    }

    // Add Listeners to Buttons
    private void AddListeners(Button[] buttons, Action<Button> action)
    {
        foreach (var button in buttons)
        {
            button.onClick.AddListener(() => action(button));
        }
    }

    // Initialize Passthrough Settings
    private void InitializePassthrough()
    {
        UInt32 count = 0;
        float[] refreshRates = new float[2];
        XrResult result = XR_FB_display_refresh_rate.EnumerateDisplayRefreshRates(0, out count, out refreshRates[0]);

        if (result == XrResult.XR_SUCCESS)
        {
            Debug.Log($"Found {count} refresh rates.");
            Array.Resize(ref refreshRates, (int)count);
            result = XR_FB_display_refresh_rate.EnumerateDisplayRefreshRates(count, out count, out refreshRates[0]);

            if (result == XrResult.XR_SUCCESS)
            {
                for (int i = 0; i < count && i < targetFrameRateButtons.Length; i++)
                {
                    Text text = targetFrameRateButtons[i].GetComponentInChildren<Text>();
                    if (text != null)
                    {
                        text.text = refreshRates[i].ToString();
                    }
                }
            }
        }
    }

    // Passthrough Enable/Disable
    private void OnPassthroughEnable(bool isEnabled)
    {
        passthroughEnableButton.interactable = !isEnabled;
        passthroughDisableButton.interactable = isEnabled;

        SetImageButtonsInteractable(isEnabled);

        if (!isEnabled)
        {
            ResetButtons(imageQualityButtons);
            ResetButtons(imageRateButtons);
        }
    }

    // Handle Button Selections
    private void OnImageQualitySelected(Button selectedButton) => ToggleButtonSelection(imageQualityButtons, selectedButton);
    private void OnImageRateSelected(Button selectedButton) => ToggleButtonSelection(imageRateButtons, selectedButton);
    private void OnTargetFrameRateSelected(Button selectedButton) => ToggleButtonSelection(targetFrameRateButtons, selectedButton);
    //private void OnSharpeningModeSelected(Button selectedButton) => ToggleButtonSelection(sharpeningModeButtons, selectedButton);
    private void OnSharpeningLevelSelected(Button selectedButton) => ToggleButtonSelection(sharpeningLevelButtons, selectedButton);

    // Handle sharpening mode button selection
    private void OnSharpeningModeSelected(Button selectedButton)
    {
        ToggleButtonSelection(sharpeningModeButtons, selectedButton);

        // Enable sharpening level buttons only if a mode other than "off" is selected
        bool isSharpeningModeSelected = false;
        foreach (var button in sharpeningModeButtons)
        {
            if (button == selectedButton && button.name != "Sharpening_off_btn")
            {
                isSharpeningModeSelected = true;
                break;
            }
        }

        SetSharpeningLevelInteractable(isSharpeningModeSelected);

        if (!isSharpeningModeSelected)
        {
            ResetButtons(sharpeningLevelButtons);
        }
    }

    private void OnFoveatedModeSelected(Button clickedButton)
    {
        if (selectedFoveatedModeButton != null)
        {
            selectedFoveatedModeButton.interactable = true;
        }

        selectedFoveatedModeButton = clickedButton;
        selectedFoveatedModeButton.interactable = false;

        SetFoveatedRenderingLevelInteractable(clickedButton == foveatedModeButtons[0]);
    }

    public void FoveationIsDisable()
    {
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_DISABLE_HTC, 0, null);
    }

    public void FoveationIsEnable()
    {
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_FIXED_HTC, 0, null);
    }

    public void Foveated_Head()
    {
        FoveationIsDynamic(true, true, false);
    }

    public void Foveated_EyeTracking()
    {
        FoveationIsDynamic(false, true, true);
    }

    public void Foveated_EyeTracing()
    {
        FoveationIsDynamic(true, true, true);
    }



    public void FoveationIsDynamic(bool bit01, bool bit02, bool bit04)
    {
        UInt64 flags = (bit01 ? ViveFoveation.XR_FOVEATION_DYNAMIC_LEVEL_ENABLED_BIT_HTC : 0x00) |
            (bit02 ? ViveFoveation.XR_FOVEATION_DYNAMIC_CLEAR_FOV_ENABLED_BIT_HTC : 0x00) |
            (bit04 ? ViveFoveation.XR_FOVEATION_DYNAMIC_FOCAL_CENTER_OFFSET_ENABLED_BIT_HTC : 0x00);
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_DYNAMIC_HTC, 0, null, flags);
    }

    public void FoveatedHigh()
    {
        foveationConfigs[0].clearFovDegree = FOVLarge;
        foveationConfigs[0].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_HIGH_HTC;
        foveationConfigs[1].clearFovDegree = FOVLarge;
        foveationConfigs[1].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_HIGH_HTC;

        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, foveationConfigs);
    }

    public void FoveatedLow()
    {
        foveationConfigs[0].clearFovDegree = FOVSmall;
        foveationConfigs[0].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_LOW_HTC;
        foveationConfigs[1].clearFovDegree = FOVSmall;
        foveationConfigs[1].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_LOW_HTC;

        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, foveationConfigs);
    }

    public void FoveatedMiddle()
    {
        foveationConfigs[0].clearFovDegree = FOVMiddle;
        foveationConfigs[0].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_MEDIUM_HTC;
        foveationConfigs[1].clearFovDegree = FOVMiddle;
        foveationConfigs[1].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_MEDIUM_HTC;

        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, foveationConfigs);
    }
    private void OnFoveatedLevelSelected(Button selectedButton) => ToggleButtonSelection(foveatedLevelButtons, selectedButton);

    static public void SetImageRate(int dst)
    {
        VivePassthroughImageRateChanged.Listen(OnImageRateChanged);

        XrPassthroughConfigurationImageRateHTC config = new XrPassthroughConfigurationImageRateHTC();
        config.type = XrStructureType.XR_TYPE_PASSTHROUGH_CONFIGURATION_IMAGE_RATE_HTC;
        config.next = IntPtr.Zero;
        config.srcImageRate = 90;
        config.dstImageRate = dst;

        int size = Marshal.SizeOf(typeof(XrPassthroughConfigurationImageRateHTC));
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(config, ptr, false);
        if (XR_HTC_passthrough_configuration.SetPassthroughConfigurationHTC(ptr) == XrResult.XR_SUCCESS)
        {
            Debug.Log("Passthrough_manager set srcImageRate = " + config.srcImageRate);
            Debug.Log("Passthrough_manager set dstImageRate = " + config.dstImageRate);
        }
        Marshal.FreeHGlobal(ptr);
    }

    static private void OnImageQualityChanged(float fromQuality, float toQuality)
    {
        Debug.Log("Passthrough_manager from scale = " + fromQuality);
        Debug.Log("Passthrough_manager to scale = " + toQuality);
    }

    static private void OnImageRateChanged(float fromSrcImageRate, float fromDestImageRate, float toSrcImageRate, float toDestImageRate)
    {
        Debug.Log("Passthrough_manager fromSrcImageRate = " + fromSrcImageRate);
        Debug.Log("Passthrough_manager fromDestImageRate = " + fromDestImageRate);
        Debug.Log("Passthrough_manager toSrcImageRate = " + toSrcImageRate);
        Debug.Log("Passthrough_manager toDestImageRate = " + toDestImageRate);
    }
    static public void SetQuality(float quality)
    {
        VivePassthroughImageQualityChanged.Listen(OnImageQualityChanged);

        XrPassthroughConfigurationImageQualityHTC config = new XrPassthroughConfigurationImageQualityHTC();
        config.type = XrStructureType.XR_TYPE_PASSTHROUGH_CONFIGURATION_IMAGE_QUALITY_HTC;
        config.next = IntPtr.Zero;
        config.scale = quality;

        int size = Marshal.SizeOf(typeof(XrPassthroughConfigurationImageQualityHTC));
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(config, ptr, false);
        if (XR_HTC_passthrough_configuration.SetPassthroughConfigurationHTC(ptr) == XrResult.XR_SUCCESS)
        {
            Debug.Log("Passthrough_manager set scale = " + config.scale);
        }
        Marshal.FreeHGlobal(ptr);
    }


    // Utility Methods
    private void SetImageButtonsInteractable(bool interactable)
    {
        SetButtonsInteractable(imageQualityButtons, interactable);
        SetButtonsInteractable(imageRateButtons, interactable);
    }

    static public void SetRefreshRate(float rate)
    {
        XR_FB_display_refresh_rate.RequestDisplayRefreshRate(rate);

    }
    static public void Sharpening_Disactivate()
    {
        XR_HTC_composition_layer_extra_settings.Interop.DisableSharpening();
    }
    static public void SetConfig1()
    {
        sharpeningMode = XrSharpeningModeHTC.FAST;
        XR_HTC_composition_layer_extra_settings.Interop.EnableSharpening(sharpeningMode, sharpeningLevel);
    }

    static public void SetConfig2()
    {
        sharpeningMode = XrSharpeningModeHTC.NORMAL;
        XR_HTC_composition_layer_extra_settings.Interop.EnableSharpening(sharpeningMode, sharpeningLevel);
    }

    static public void SetConfig3()
    {
        sharpeningMode = XrSharpeningModeHTC.QUALITY;
        XR_HTC_composition_layer_extra_settings.Interop.EnableSharpening(sharpeningMode, sharpeningLevel);
    }

    static public void SetConfig4()
    {
        sharpeningMode = XrSharpeningModeHTC.AUTOMATIC;
        XR_HTC_composition_layer_extra_settings.Interop.EnableSharpening(sharpeningMode, sharpeningLevel);
    }

    static public void SetLevel(float num)
    {
        sharpeningLevel = num;
        XR_HTC_composition_layer_extra_settings.Interop.EnableSharpening(sharpeningMode, sharpeningLevel);
    }
    private void SetSharpeningLevelInteractable(bool interactable) => SetButtonsInteractable(sharpeningLevelButtons, interactable);
    private void SetFoveatedRenderingLevelInteractable(bool interactable) => SetButtonsInteractable(foveatedLevelButtons, interactable);

    private void SetButtonsInteractable(Button[] buttons, bool interactable)
    {
        foreach (var button in buttons)
        {
            button.interactable = interactable;
        }
    }

    private void ResetButtons(Button[] buttons)
    {
        foreach (var button in buttons)
        {
            button.interactable = true;
        }
    }

    private void ToggleButtonSelection(Button[] buttons, Button selectedButton)
    {
        foreach (var button in buttons)
        {
            button.interactable = button != selectedButton;
        }
    }

    private void ResetAll()
    {
        ResetButtons(imageQualityButtons);
        ResetButtons(imageRateButtons);
        ResetButtons(targetFrameRateButtons);
        ResetButtons(sharpeningModeButtons);
        ResetButtons(sharpeningLevelButtons);
        ResetButtons(foveatedModeButtons);
        ResetButtons(foveatedLevelButtons);

        SetImageButtonsInteractable(false);
        SetSharpeningLevelInteractable(false);
        SetFoveatedRenderingLevelInteractable(false);

        OnPassthroughEnable(true);
        SetRefreshRate(90);
        Sharpening_Disactivate();
        FoveationIsDisable();
    }

    public void ShowCanvas(bool state) => canvasObject.enabled = state;
    public void StartGame()
    {
        EventMediator.LeavePerformanceMode();
        ShowCanvas(false);
    }
}
