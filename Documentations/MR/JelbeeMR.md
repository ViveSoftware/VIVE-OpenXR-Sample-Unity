# Jelbee MR Demo - OpenXR
Copyright 2024, HTC Corporation. All rights reserved.
## About
Jelbee MR Demo is an MR experience example for [VIVE XR Elite](https://www.vive.com/us/product/vive-xr-elite/overview/) and [VIVE Focus Vision](https://www.vive.com/us/product/vive-focus-vision/overview/). It demonstrates MR applications such as virtual room setup, gestures, interactions, and grabbing objects into MR. There are four experiences in the Demo:
+	Whack-A-Mole on the desk 
+	Bubbling with OK gesture 
+	MR Portal 
+	Grabbing planets into MR
## Requirements
+	VIVE XR Elite, VIVE Focus Vision
+	Unity 2021.3.16f1 or newer
+	VIVE OpenXR Plugin 2.5.0 or newer
+	VIVE Input Utility 1.19.0-preview.12 or newer
## Settings & Build Setup
+	Set Build `Settings > Platform` to Android
+   Import [[Customizable Planet Shaders]](https://assetstore.unity.com/packages/vfx/shaders/customizable-planet-shaders-131872) package into project.
+	Set BaseScene as the starting scene
+	Check OpenXR in `Project Settings > XR Plug-in Management > Initialize XR on Startup`
## How to Walk Through the Game Flow in Editor
1.	After opening the project, open the Base Scene and click the play button. 
2.	Press the Spacebar to simulate pressing the virtual start button. 
3.	**Level 1 Desktop Whack-A-Mole** : Press the spacebar to simulate pat Jelbee. 
4.	**Level 2 Bubbling OK gesture** : Use cursor to control the direction of the bubble spray. 
5.	**Level 3 MR Portal-Forest** : Use the cursor to draw circle and shoot the portal, it can only be shot on the virtual wall. 
6.	**Level 3 MR Portal-Universe** : Press the Spacebar to generate planet.
## How to Play with XR Elite / Focus Vision
1.	Setup the environment in `Settings / Boundary / MR Room Setup`, this demo requires surrounding walls, a desk, and a window. 
2.	Start the app, press the A button on the right controller to set pivots in the environment.
    - Click the button for the item you want to configure.(Button `Wall`, `Desk` or `Window`)
    - Select the desired plane.
    - Click button `Set Wall`,  `Set Desk` or `Set Window` to configurate.
    - After completing the three configurations, select `Save`.
    - Finally, click `Start Game`. 
3.	Put down the controller and directly press the virtual start button on the table by your hand. 
4.	**Level 1 Whack-A-Mole** : Pat Jelbee with your hand, hit Jelbee three times to enter the next level. 
5.	**Level 2 Bubbling** : Use the OK gesture to make bubbles, make bubbles to Jelbee three times to enter the next level. 
6.	**Level 3 MR Portal-Forest** : Draw a circle towards the wall to create a portal. The first portal only shows the forest outside, and it will be closed automatically after 10 seconds. 
7.	**Level 3 MR Portal-Universe** : Draw a circle towards the wall to create a portal. The second portal is the space environment, when the sun appears, make fist gesture to grab a planet in, when you open your hand, the planet will join the solar system.

## Developer Guidelines
+	**MRFlowManager.cs** : The main game flow.
+	**SceneComponentManager.cs** : Load the plane data which users created in MR Room Setup.
+	**PivotManager.cs** : Allows developers to customize keys and pivot for the planes needed in the game, also specify the required plane by reading the configuration file. For example, the key "Desk" can get the virtual desktop pivot position specified by the user. 
+	**SetupManager.cs** : Let user specify the plane corresponding to the key. 
+	**PortalSceneManager.cs** : The flow of the MR Portal levels and the drawing method of the portal. 
+	**PortalBallBehaviour.cs** : If you want to add a new portal, you need to inherit this script, such as UniversePortalBall.cs.

## Third Party Assets
+	[Customizable Planet Shaders](https://assetstore.unity.com/packages/vfx/shaders/customizable-planet-shaders-131872) (not included in project)
+	[DOTween](http://dotween.demigiant.com/)