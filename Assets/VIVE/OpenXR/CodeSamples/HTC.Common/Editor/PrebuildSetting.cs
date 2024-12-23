using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

class PrebuildSetting : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    const string QualitySettingName = "Medium";

    public void OnPreprocessBuild(BuildReport report)
    {
        for(int i=0; i<QualitySettings.names.Length; ++i)
        {
            if (QualitySettings.names[i] == QualitySettingName)
            {
                Debug.Log($"Set Quality setting to: {QualitySettingName}");
                QualitySettings.SetQualityLevel(i, true);
                break;
            }
        }
    }
}
#endif