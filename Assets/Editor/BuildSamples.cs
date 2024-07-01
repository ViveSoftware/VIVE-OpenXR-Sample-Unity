// Copyright HTC Corporation All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace VIVE.OpenXR.StarterSamples.Editor
{
    public class BuildSamples : EditorWindow
    {
		#region Sample - Basic
		public class BuildSampleBasic
		{
			private static string bundleVersion = "0.1";
			private static string apkProductName = "Basic";
			private static string apkName = "vive_unity_Basic.apk";

			private static string sceneName = "Basic";
			private static string apkPackageName = "com.vive.unity.Basic";
			private static string[] levels = new string[] { "Assets/ViveOpenXRStarterSamples/Basic/Basic.unity" };

			private static string _destinationPath;
			private static void CustomizedCommandLine()
			{
				Dictionary<string, Action<string>> cmdActions = new Dictionary<string, Action<string>>
				{
					{
						"-destinationPath", delegate(string argument)
						{
							_destinationPath = argument;
						}
					}
				};

				Action<string> actionCache;
				string[] cmdArguments = Environment.GetCommandLineArgs();

				for (int count = 0; count < cmdArguments.Length; count++)
				{
					if (cmdActions.ContainsKey(cmdArguments[count]))
					{
						actionCache = cmdActions[cmdArguments[count]];
						actionCache(cmdArguments[count + 1]);
					}
				}

				if (string.IsNullOrEmpty(_destinationPath))
				{
					_destinationPath = Path.GetDirectoryName(Application.dataPath);
				}
			}

			private static void GeneralSettings()
			{
				PlayerSettings.Android.bundleVersionCode = 1;
				PlayerSettings.bundleVersion = bundleVersion;
				PlayerSettings.companyName = "HTC Corp.";
				PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
				PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
			}

			private static string SearchScenePath(string sceneName)
			{
				sceneName = Path.GetFileNameWithoutExtension(sceneName);
				var list = AssetDatabase.FindAssets(sceneName + " t:scene");
				foreach (var sceneGUID in list)
				{
					var scenePath = AssetDatabase.GUIDToAssetPath(sceneGUID);
					if (Path.GetFileNameWithoutExtension(scenePath) == sceneName)
						return scenePath;
				}
				return default;
			}

			public static void BuildApk(string version, string product, string name)
			{
				bundleVersion = version;
				apkProductName = product;
				apkName = name;

				CustomizedCommandLine();
				BuildApkInner(_destinationPath, false, false, levels, true, false, true);
			}

			private static void ApplyPlayerSettings(bool isIL2CPP = true, bool isSupport32 = false, bool isSupport64 = true)
			{
				Debug.Log("ApplyPlayerSettings");

				GeneralSettings();

				if (!isSupport32 && !isSupport64)
					isSupport32 = true;

				PlayerSettings.productName = apkProductName;

				PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, apkPackageName);
				var VRTestAppPath = SearchScenePath(sceneName);
				var VRTestAppDir = Path.GetDirectoryName(VRTestAppPath);
				var IconPath = VRTestAppDir.ToString().Replace("\\", "/") + "/Textures/test.png";
				Texture2D icon = (Texture2D)AssetDatabase.LoadAssetAtPath(IconPath, typeof(Texture2D));
				if (icon == null)
					Debug.LogError("Fail to read app icon");

				Texture2D[] group = { icon, icon, icon, icon, icon, icon };

				PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, group);
				PlayerSettings.gpuSkinning = false;
				PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, true);
				PlayerSettings.graphicsJobs = true;

				// This can help check the Settings by text editor
				EditorSettings.serializationMode = SerializationMode.ForceText;

				// Force use GLES31
				PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
				UnityEngine.Rendering.GraphicsDeviceType[] apis = { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 };
				PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, apis);
				PlayerSettings.openGLRequireES31 = false;
				PlayerSettings.openGLRequireES31AEP = false;

				if (isIL2CPP)
					PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
				else
					PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
				if (isSupport32)
				{
					if (isSupport64)
						PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
					else
						PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
				}
				else
				{
					PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
				}

				PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
				PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel29;

				AssetDatabase.SaveAssets();
			}

			private static void BuildApkInner(string destPath, bool run, bool development, string[] levels, bool isIL2CPP = true, bool isSupport32 = false, bool isSupport64 = true)
			{
				ApplyPlayerSettings(isIL2CPP, isSupport32, isSupport64);

				string outputFilePath = string.IsNullOrEmpty(destPath) ? apkName : destPath + "/" + apkName;
				BuildOptions extraFlags = BuildOptions.None;
				BuildOptions buildOptions = (run ? BuildOptions.AutoRunPlayer : BuildOptions.None) | (development ? BuildOptions.Development : BuildOptions.None) | extraFlags;

				BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
				{
					//assetBundleManifestPath = "",
					options = buildOptions,
					target = BuildTarget.Android,
					scenes = levels,
					targetGroup = BuildTargetGroup.Android,
					locationPathName = outputFilePath
				};
				BuildPipeline.BuildPlayer(buildPlayerOptions);
			}
		}
		#endregion

		[MenuItem("VIVE/Build Starter Samples")]
        public static void ShowWindow()
        {
            GetWindow(typeof(BuildSamples));
        }

        private string m_TextVersion = "0.1";
		private Vector2 scrollPos = Vector2.zero;
        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Enter the sample version:", MessageType.Info);
            m_TextVersion = EditorGUILayout.TextField(m_TextVersion);
			
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

			#region Sample - Basic
			EditorGUILayout.HelpBox("Enter the Basic sample product name:", MessageType.Info);
			string sampleNameProduct = "Basic";
			sampleNameProduct = EditorGUILayout.TextField(sampleNameProduct);

			EditorGUILayout.HelpBox("Enter the Basic sample APK name:", MessageType.Info);
			string sampleNameBasic = "vive_unity_Basic.apk";
			sampleNameBasic = EditorGUILayout.TextField(sampleNameBasic);

			EditorGUILayout.HelpBox("Build Starter Samples.", MessageType.Info);
            if (GUILayout.Button("Build Sample - Basic"))
            {
				BuildSampleBasic.BuildApk(m_TextVersion, sampleNameProduct, sampleNameBasic);
			}
			#endregion

			EditorGUILayout.EndScrollView();

			Repaint();
        }
    }
}
#endif