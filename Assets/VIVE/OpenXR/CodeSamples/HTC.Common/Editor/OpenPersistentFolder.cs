using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public static class OpenPersistentFolder
{
    [MenuItem("HTC/Editor Extension/Open DataPath")]
    public static void OpenFolder()
    {
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
}
#endif