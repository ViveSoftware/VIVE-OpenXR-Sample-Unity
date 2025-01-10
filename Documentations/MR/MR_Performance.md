# MR Performance Setting - OpenXR
Copyright 2024, HTC Corporation. All rights reserved.
## About
Jelbee MR Performance Setting is an MR experience optimization demo designed for [VIVE XR Elite](https://www.vive.com/us/product/vive-xr-elite/overview/) and [VIVE Focus Vision](https://www.vive.com/us/product/vive-focus-vision/overview/). This demo showcases the impact of performance-related settings on MR experiences, allowing developers to fine-tune features and evaluate their effects. Key performance settings available for adjustment include:
+	Passthrough image quality / image rate
+	Foveated rendering
+	Target Framerate
+	Sharpening
## Requirements
+	VIVE XR Elite, VIVE Focus Vision
+	Unity 2021.3.16f1 or newer
+	VIVE OpenXR Plugin 2.5.0 or newer
+	VIVE Input Utility 1.19.0-preview.12 or newer
## Settings & Build Setup
+	Set Build `Settings > Platform` to Android
+   Import [[Customizable Planet Shaders]](https://assetstore.unity.com/packages/vfx/shaders/customizable-planet-shaders-131872) package into project.
+   Add scenes `BaseScene`, `GameFlow`, `RobotAssistant`, `Setup` and `MRPerformance` to the build setttings accordingly.
+	Check OpenXR in `Project Settings > XR Plug-in Management > Initialize XR on Startup`

## How to Play with XR Elite / Focus Vision
1.	Start the app, press B button on the right controller to adjust the MR settings.
2.  Click the button for the item you want to configurate.
3.  After completing the configurations, select `Start`. 

![mrperformance01.png](https://github.com/ViveSoftware/VIVE-OpenXR-Sample-Unity/blob/main/Documentations/Medias/mrperformance01.png)

## Developer Guidelines
+	**PerformanceController.cs** : Control the performance of MR Related features.  

## Third Party Assets
+	[Customizable Planet Shaders](https://assetstore.unity.com/packages/vfx/shaders/customizable-planet-shaders-131872) (not included in project)
+	[DOTween](http://dotween.demigiant.com/)

#### **For more infomation of performance tunning, please go to [Performance tunnung | VIVE OpenXR Developer Guide](https://developer.vive.com/resources/openxr/unity/performance-tunning/).**