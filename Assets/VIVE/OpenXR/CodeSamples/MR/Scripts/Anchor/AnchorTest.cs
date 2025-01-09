// Copyright HTC Corporation All Rights Reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using VIVE.OpenXR.Feature;
using VIVE.OpenXR.Toolkits;
using VIVE.OpenXR.Toolkits.Anchor;


namespace VIVE.OpenXR.Samples.Anchor
{

    public class AnchorTest : MonoBehaviour
    {
        public Transform rig;
        public Transform anchorPoseD;
        public Transform anchorPose1;
        public Transform anchorPose2;
        public Transform anchorPose3;
        public Transform obj;
        public GameObject panchorObj1;
        public GameObject panchorObj2;
        public GameObject panchorObj3;
        public AnchorManager.Anchor anchor1;
        public AnchorManager.Anchor anchor2;
        public AnchorManager.Anchor anchor3;
        string anchor1FromPA;
        string anchor2FromPA;
        string anchor3FromPA;
        public TextMeshProUGUI statusResponse;
        public TextMeshProUGUI statusAnchor;

        public Button btnClearSpatialAnchors;

        public Button btnClearPersistedAnchors;
        public Button btnAcquirePAC;
        public Button btnReleasePAC;

        public Button btnExportAll;
        public Button btnImportAll;
        public Button btnClearExportedAnchors;

        public Button btnReloadScene;

        public Button setwall;
        public Button setwindow;
        public Button setdesk;

        public XRInputSubsystem xrInputSubsystem;

        public Transform parentTransform;

        public int lastPACount = -1;
        private int lastFoundExportedFilesCount = -1;
        private FutureTask<(XrResult, IntPtr)> taskPAC = null;

        public bool isAnchorSupported;
        public bool isPAnchorSupported;
        public bool isPACollectionAcquired;

        // 持 means "hold", "persisted".
        public string utf8Word = "persisted";

        // In Update, check enumerated spatial anchors to update these flags
        public bool hasPersistedAnchor1;
        public string persistedAnchor1Name;
        public bool hasPersistedAnchor2;
        public string persistedAnchor2Name;
        public bool hasPersistedAnchor3;
        public string persistedAnchor3Name;

        public bool isSetWall = false;
        public bool isSetWindow = false;
        public bool isSetDesk = false;
        // Any anchor file in the app's local storage set flag to true
        bool hasPersistedAnchorFiles;

        bool doesUIInteractionsNeedUpdated = false;
        float lastUpdateTime = 0;

        readonly FutureTaskManager<XrSpace, XrResult> tmPA = new FutureTaskManager<XrSpace, XrResult>();
        public FutureTaskManager<string, (XrResult, AnchorManager.Anchor)> tmFPA = new FutureTaskManager<string, (XrResult, AnchorManager.Anchor)>();
        readonly List<Task> tasks = new List<Task>();


        public SetupManager setup;

        void GetXRInputSubsystem()
        {
            List<XRInputSubsystem> xrSubsystemList = new List<XRInputSubsystem>();
            SubsystemManager.GetSubsystems(xrSubsystemList);
            foreach (var xrSubsystem in xrSubsystemList)
            {
                if (xrSubsystem.running)
                {
                    xrInputSubsystem = xrSubsystem;
                    break;
                }
            }
        }

        private void OnEnable()
        {
            btnClearSpatialAnchors.onClick.AddListener(OnClearAllAnchors);

            btnAcquirePAC.onClick.AddListener(OnAcquirePersistedAnchorCollection);
            btnReleasePAC.onClick.AddListener(OnReleasePersistedAnchorCollection);

            btnClearPersistedAnchors.onClick.AddListener(OnClearPersistedAnchors);
            btnExportAll.onClick.AddListener(OnExportAll);
            btnImportAll.onClick.AddListener(OnImportAll);
            btnClearExportedAnchors.onClick.AddListener(OnClearExportedAnchors);

            btnReloadScene.onClick.AddListener(OnReloadScene);
            setwall.onClick.AddListener(OnSetWall);
            setwindow.onClick.AddListener(OnSetWindow);
            setdesk.onClick.AddListener(OnSetDesk);
        }

        private void OnSetWall()
        {
            OnCreateAnchor1();
            CheckSupported();
            OnPersistAnchor1();
            CheckSupported();
            isSetWall = true;
            UINeedUpdate();

        }
        private void OnSetWindow()
        {
            OnCreateAnchor2();
            CheckSupported();
            OnPersistAnchor2();
            CheckSupported();
            isSetWindow = true;
            UINeedUpdate();
        }

        private void OnSetDesk()
        {
            OnCreateAnchor3();
            CheckSupported();
            OnPersistAnchor3();
            CheckSupported();
            isSetDesk = true;
            UINeedUpdate();
        }
        /*
        private void OnAcquirePersistedAnchorCollection()
        {
            if (AnchorManager.IsPersistedAnchorCollectionAcquired()) return;
            if (taskPAC != null) return;

            taskPAC = AnchorManager.AcquirePersistedAnchorCollection();
            // Acturally, it will auto complete by default.  Just make sure we can continue with AutoCompleteTask
            taskPAC.AutoComplete();
            taskPAC.AutoCompleteTask.ContinueWith((act) =>
            {
                taskPAC = null;
                CheckSupported();
                UINeedUpdate();
            });
        }
        */
        private void OnAcquirePersistedAnchorCollection()
        {
            if (AnchorManager.IsPersistedAnchorCollectionAcquired()) return;
            if (taskPAC != null) return;

            try
            {
                taskPAC = AnchorManager.AcquirePersistedAnchorCollection();
                taskPAC.AutoComplete();
                taskPAC.AutoCompleteTask.ContinueWith((act) =>
                {
                    taskPAC?.Dispose();
                    taskPAC = null;
                    CheckSupported();
                    UINeedUpdate();
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error acquiring persisted anchor collection: {ex.Message}");
                taskPAC?.Dispose();
                taskPAC = null;
            }
        }


        private void OnReleasePersistedAnchorCollection()
        {
            if (taskPAC != null)
            {
                taskPAC.Dispose();
                taskPAC = null;
            }

            anchor1FromPA = null;
            anchor2FromPA = null;
            anchor3FromPA = null;
            hasPersistedAnchor1 = false;
            hasPersistedAnchor2 = false;
            hasPersistedAnchor3 = false;
            persistedAnchor1Name = null;
            persistedAnchor2Name = null;
            persistedAnchor3Name = null;

            AnchorManager.ReleasePersistedAnchorCollection();
            CheckSupported();
            UINeedUpdate();
        }





        private void OnDisable()
        {
            // Remove listeners
            btnClearSpatialAnchors.onClick.RemoveListener(OnClearAllAnchors);
            btnAcquirePAC.onClick.RemoveListener(OnAcquirePersistedAnchorCollection);
            btnReleasePAC.onClick.RemoveListener(OnReleasePersistedAnchorCollection);
            btnClearPersistedAnchors.onClick.RemoveListener(OnClearPersistedAnchors);
            btnExportAll.onClick.RemoveListener(OnExportAll);
            btnImportAll.onClick.RemoveListener(OnImportAll);
            btnClearExportedAnchors.onClick.RemoveListener(OnClearExportedAnchors);
            btnReloadScene.onClick.RemoveListener(OnReloadScene);

            setwall.onClick.RemoveListener(OnSetWall);
            setwindow.onClick.RemoveListener(OnSetWindow);
            setdesk.onClick.RemoveListener(OnSetDesk);

            // No need to clear all persistance anchor.  Reload scene can test the persist.

            // Disable Persistance Anchor
            AnchorManager.ReleasePersistedAnchorCollection();

            // Dispose all anchors
            OnClearAllAnchors();

            foreach (var task in tmFPA.GetTasks())
            {
                task.Item2.Dispose();
            }

            // Dispose all tasks
            foreach (var task in tasks)
            {
                task.Dispose();
            }
            tasks.Clear();



            // Clear all task managers.  It's readonly, so no need to dispose.
            tmPA.Clear();
            tmFPA.Clear();
        }

        private void OnDestroy()
        {
            if (taskPAC != null)
            {
                taskPAC.Dispose();
                taskPAC = null;
            }

            OnClearAllAnchorsInner(true);
            foreach (var task in tmFPA.GetTasks())
            {
                task.Item2.Dispose();
            }
            tmFPA.Clear();
            // Dispose all tasks
            foreach (var task in tasks)
            {
                task.Dispose();
            }
            tasks.Clear();
        }

        public void UINeedUpdate()
        {
            doesUIInteractionsNeedUpdated = true;
        }

        public void UpdateUIInteractions()
        {
            if ((Time.time - lastUpdateTime) > 2.0f)
            {
                lastUpdateTime = Time.time;
                doesUIInteractionsNeedUpdated = true;
            }

            if (!doesUIInteractionsNeedUpdated)
                return;
            doesUIInteractionsNeedUpdated = false;

            bool hasAnchor1 = anchor1 != null;
            bool hasAnchor2 = anchor2 != null;
            bool hasAnchor3 = anchor3 != null;

            bool hasAnchor = hasAnchor1 || hasAnchor2 || hasAnchor3;

            // Create / Follow / Reset / Dispose
            btnClearSpatialAnchors.interactable = hasAnchor;

            // Create Persisted Anchor Collection / Destroy Persisted Anchor Collection
            // If has created, can destroy
            // If has not created, can create
            btnAcquirePAC.interactable = isPAnchorSupported && !isPACollectionAcquired && taskPAC == null;
            btnReleasePAC.interactable = isPAnchorSupported && isPACollectionAcquired;

            // Persist / Unpersist / Clear
            // If has anchor, can persist
            // If has persisted anchor, can unpersist
            // If has persisted anchor, can clear
            btnClearPersistedAnchors.interactable = isPACollectionAcquired && (hasPersistedAnchor1 || hasPersistedAnchor2 || hasPersistedAnchor3);

            // Export / Import / Clear
            // If has persisted anchor, can export
            // If has exported anchor, can import
            // If has exported anchor, can clear
            btnExportAll.interactable = isPACollectionAcquired && (hasPersistedAnchor1 || hasPersistedAnchor2) && tasks.Count == 0;
            btnImportAll.interactable = isPACollectionAcquired && hasPersistedAnchorFiles && tasks.Count == 0;
            // Not to clear when exporting or importing
            btnClearExportedAnchors.interactable = hasPersistedAnchorFiles && tasks.Count == 0;


            setwall.interactable = !isSetWall;
            setwindow.interactable = !isSetWindow;
            setdesk.interactable = !isSetDesk;

            string sa1 = "", sa2 = "", sa3 = "";
            if (hasAnchor1)
                sa1 = "A1: " + anchor1.Name + (anchor1.IsTrackable ? " T" : "") + (anchor1.IsPersisted ? " P" : "") + "\n";
            if (hasAnchor2)
                sa2 = "A2: " + anchor2.Name + (anchor2.IsTrackable ? " T" : "") + (anchor2.IsPersisted ? " P" : "") + "\n";
            if (hasAnchor3)
                sa3 = "A3: " + anchor3.Name + (anchor3.IsTrackable ? " T" : "") + (anchor3.IsPersisted ? " P" : "") + "\n";

            statusAnchor.text = sa1 + sa2 + sa3 +
                (hasPersistedAnchor1 ? "PA1: " + persistedAnchor1Name + "\n" : "") +
                (hasPersistedAnchor2 ? "PA2: " + persistedAnchor2Name + "\n" : "") +
                (hasPersistedAnchor3 ? "PA3: " + persistedAnchor3Name + "\n" : "");
        }

        void CheckSupported()
        {
            isAnchorSupported = AnchorManager.IsSupported();
            Debug.Log("AnchorTestHandle: Is Anchor supported: " + isAnchorSupported);
            isPAnchorSupported = AnchorManager.IsPersistedAnchorSupported();
            Debug.Log("AnchorTestHandle: Is Persisted Anchor supported: " + isPAnchorSupported);
            isPACollectionAcquired = AnchorManager.IsPersistedAnchorCollectionAcquired();
            Debug.Log("AnchorTestHandle: Is Persisted Anchor Collection acquired: " + isPACollectionAcquired);
        }

        IEnumerator Start()
        {


            CheckSupported();
            UINeedUpdate();
            UpdateUIInteractions();

            yield return null;  // yield and let Time.unscaledTime to be updated
            float t = Time.unscaledTime;
            while (xrInputSubsystem == null)
            {
                yield return null;
                GetXRInputSubsystem();
                if (Time.unscaledTime - t > 5)
                {
#if UNITY_EDITOR
                    Debug.LogError("Get XRInputSubsystem timeout.  Check if you acturally enable the OpenXR's Mock Runtime.");
#else
                    Debug.LogError("Get XRInputSubsystem timeout.  Check if you acturally enable any VR runtime."); 
#endif
                    statusResponse.text = "No VR runtime";
                    yield break;
                }
            }

            // Check again
            CheckSupported();
            UINeedUpdate();

            if (!isPAnchorSupported) yield break;

            Debug.Log("AnchorTestHandle: AcquirePersistedAnchorCollection");

            taskPAC = AnchorManager.AcquirePersistedAnchorCollection();
            taskPAC.Debug = true;
            while (!AnchorManager.IsPersistedAnchorCollectionAcquired())
            {
                Debug.Log("AnchorTestHandle: Wait PersistedAnchorCollection acquired");
                yield return null;
            }
            if (taskPAC != null)
            {
                taskPAC.Dispose();
                taskPAC = null;
            }

            AnchorManager.GetPersistedAnchorProperties(out ViveAnchor.XrPersistedAnchorPropertiesGetInfoHTC properties);
            Debug.Log("AnchorTestHandle: PersistAnchorProperties.maxPersistedAnchorCount=" + properties.maxPersistedAnchorCount);

            CheckSupported();

            Debug.Log("AnchorTestHandle: Start() finished");

            UINeedUpdate();
            UpdateUIInteractions();

        }

        public Pose GetRelatedPoseToRig(Transform t)
        {
            return new Pose(rig.InverseTransformPoint(t.position), Quaternion.Inverse(rig.rotation) * t.rotation);
        }

        string MakeAnchorName(string userName)
        {
            // userName means user defined name.  Add frame count to make it unique.
            return userName + " (" + Time.frameCount + ")";
        }

        string MakePersistedAnchorName(string anchorName)
        {
            // the anchor name support UTF-8 encoding.
            return anchorName + utf8Word;
        }

        string MakeSpatialAnchorName(string persistedName)
        {
            // Remove the utf8Word from the anchor name
            return persistedName.Substring(0, persistedName.Length - utf8Word.Length);
        }

        /// <summary>
        /// Help create anchor by anchor manager
        /// </summary>
        /// <param name="relatedPose">pose related to camera rig</param>
        /// <param name="name">the anchor's name</param>
        /// <returns></returns>
        AnchorManager.Anchor CreateAnchor(Pose relatedPose, string name)
        {
            if (!AnchorManager.IsSupported())
            {
                Debug.LogError("AnchorManager: Anchor is not supported.");
                statusResponse.text = "Anchor is not supported.";
                return null;
            }
            var anchor = AnchorManager.CreateAnchor(relatedPose, MakeAnchorName(name));
            if (anchor == null)
            {
                statusResponse.text = "Create " + name + " failed";
                Debug.LogError("Create " + name + " failed");
                // Even error, still got.  Use fake data.
                return anchor;
            }
            else
            {
                string msg = "Create Anchor n=" + anchor.Name + " space=" + anchor.GetXrSpace() + " at p=" + relatedPose.position + " & r=" + relatedPose.rotation.eulerAngles;
                statusResponse.text = msg;
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
            UINeedUpdate();
        }

        public void OnCreateAnchor2()
        {
            if (anchor2 != null)
            {
                anchor2.Dispose();
                anchor2 = null;
            }
            anchor2 = CreateAnchor(GetRelatedPoseToRig(anchorPose2), "anchor2");
            UINeedUpdate();
        }

        public void OnCreateAnchor3()
        {
            if (anchor3 != null)
            {
                anchor3.Dispose();
                anchor3 = null;
            }
            anchor3 = CreateAnchor(GetRelatedPoseToRig(anchorPose3), "anchor3");
            UINeedUpdate();
        }

        public void MoveObjToAnchor(AnchorManager.Anchor anchor)
        {
            if (!AnchorManager.IsSupported())
                return;

            if (anchor == null)
            {
                statusResponse.text = "anchor is null";
                return;
            }

            if (AnchorManager.GetTrackingSpacePose(anchor, out Pose pose))
            {
                // Convert tracking space pose to world space pose
                obj.position = rig.TransformPoint(pose.position);
                obj.rotation = rig.rotation * pose.rotation;

                //statusResponse.text = "Obj move to " + anchor.GetSpatialAnchorName();
            }
            else
            {
                statusResponse.text = "Fail to get anchor's pose";
            }
        }
        private void OnClearAllAnchorsInner(bool isDestroy = false)
        {
            Debug.Log("AnchorTestHandle: OnClearAllAnchors()");
            try
            {
                if (!isDestroy)
                {
                    // Not to touch object when destroy
                    if (obj != null && anchorPoseD != null)
                    {
                        obj.position = anchorPoseD.position;
                        obj.rotation = anchorPoseD.rotation;
                    }

                    if (statusResponse != null)
                    {
                        if (hasPersistedAnchor1 || hasPersistedAnchor2)
                            statusResponse.text = "Dispose spatial anchors but will create again by persisted";
                        else
                            statusResponse.text = "Dispose spatial anchors";
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("AnchorTestHandle: OnClearAllAnchorsInner: " + e.Message);
            }

            if (anchor1 != null)
            {
                anchor1.Dispose();
                anchor1 = null;
            }
            if (anchor2 != null)
            {
                anchor2.Dispose();
                anchor2 = null;
            }
            if (anchor3 != null)
            {
                anchor3.Dispose();
                anchor3 = null;
            }
        }

        public void OnClearAllAnchors()
        {
            OnClearAllAnchorsInner();
        }
        public void OnPersistAnchor1()
        {
            Debug.Log("AnchorTestHandle: OnPersistAnchor1()");

            if (anchor1 == null)
            {
                statusResponse.text = "anchor1 is null";
                return;
            }

            var newName = MakePersistedAnchorName(MakeAnchorName("anchor1"));

            var xrSpace = anchor1.GetXrSpace();
            if (tmPA.GetTask(xrSpace) != null)
            {
                statusResponse.text = "Persist " + newName + " is running, please wait";
                return;
            }

            try
            {
                using (var task = AnchorManager.PersistAnchor(anchor1, newName))
                {
                    if (task == null)
                    {
                        throw new InvalidOperationException("Failed to create persistence task.");
                    }

                    tmPA.AddTask(xrSpace, task);
                    statusResponse.text = "Persist " + newName + " task is running, please wait";
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error persisting anchor: {ex.Message}");
                statusResponse.text = $"Persist anchor failed：{ex.Message}";
            }
        }



        public void OnPersistAnchor2()
        {
            Debug.Log("AnchorTestHandle: OnPersistAnchor2()");

            if (anchor2 == null)
            {
                statusResponse.text = "anchor2 is null";
                return;
            }

            var newName = MakePersistedAnchorName(MakeAnchorName("anchor2"));

            var xrSpace = anchor2.GetXrSpace();
            if (tmPA.GetTask(xrSpace) != null)
            {
                statusResponse.text = "Persist " + newName + " is running, please wait";
                return;
            }

            try
            {
                using (var task = AnchorManager.PersistAnchor(anchor2, newName))
                {
                    if (task == null)
                    {
                        throw new InvalidOperationException("Failed to create persistence task.");
                    }

                    tmPA.AddTask(xrSpace, task);
                    statusResponse.text = "Persist " + newName + " task is running, please wait";
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error persisting anchor: {ex.Message}");
                statusResponse.text = $"Persist anchor failed：{ex.Message}";
            }
        }

        public void OnPersistAnchor3()
        {
            Debug.Log("AnchorTestHandle: OnPersistAnchor3()");

            if (anchor3 == null)
            {
                statusResponse.text = "anchor3 is null";
                return;
            }

            var newName = MakePersistedAnchorName(MakeAnchorName("anchor3"));

            var xrSpace = anchor3.GetXrSpace();
            if (tmPA.GetTask(xrSpace) != null)
            {
                statusResponse.text = "Persist " + newName + " is running, please wait";
                return;
            }

            try
            {
                using (var task = AnchorManager.PersistAnchor(anchor3, newName))
                {
                    if (task == null)
                    {
                        throw new InvalidOperationException("Failed to create persistence task.");
                    }

                    tmPA.AddTask(xrSpace, task);
                    statusResponse.text = "Persist " + newName + " task is running, please wait";
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error persisting anchor: {ex.Message}");
                statusResponse.text = $"Persist anchor failed：{ex.Message}";
            }
        }



        Pose ConvertWorldPoseToLocal(Pose worldPose, Transform parentTransform)
        {
            Vector3 localPosition = parentTransform.InverseTransformPoint(worldPose.position);
            Quaternion localRotation = Quaternion.Inverse(parentTransform.rotation) * worldPose.rotation;
            return new Pose(localPosition, localRotation);
        }

        private void CreatePivotFromPose(string name, Pose pose)
        {
            if (setup.pivotGizmoPrefab == null || setup.pivotGizmoRoot == null)
            {
                statusResponse.text = "PivotGizmoPrefab or PivotGizmoRoot is not assigned.";
                return;
            }


            if (setup.pivotGizmoMap.TryGetValue(name, out PivotGizmo existingPivot))
            {

                existingPivot.transform.localPosition = pose.position;
                existingPivot.transform.localRotation = pose.rotation;
                statusResponse.text = $"Moved existing Pivot: {name} to position: {existingPivot.transform.position}";
            }
            else
            {

                PivotGizmo pivotGizmo = Instantiate(setup.pivotGizmoPrefab, setup.pivotGizmoRoot);
                pivotGizmo.SetLabel(name);
                pivotGizmo.transform.localPosition = pose.position;
                pivotGizmo.transform.localRotation = pose.rotation;

                setup.pivotGizmoMap[name] = pivotGizmo;

                statusResponse.text = $"Created new Pivot: {name}, position: {pivotGizmo.transform.position}";
            }
        }


        public void OnClearPersistedAnchors()
        {
            Debug.Log("AnchorTestHandle: OnClearPersistedAnchors()");
            var ret = AnchorManager.ClearPersistedAnchors() == XrResult.XR_SUCCESS;
            if (ret)
            {
                hasPersistedAnchor1 = false;
                hasPersistedAnchor2 = false;
                hasPersistedAnchor3 = false;
                persistedAnchor1Name = null;
                persistedAnchor2Name = null;
                persistedAnchor3Name = null;

                statusResponse.text = "Clear persisted anchors success";
            }
            else
                statusResponse.text = "Clear persisted anchors failed: " + ret;
        }
        //update 2024/12/13
        public IEnumerator WaitExportFinish()
        {
            while (tasks.Count > 0)
            {
                Debug.Log("AnchorTestHandle: WaitExportFinish: " + tasks.Count);
                foreach (var task in tasks)
                {
                    if (task == null)
                    {
                        Debug.LogWarning("Found a null task in the list. Skipping.");
                        continue;
                    }

                    if (task.IsCompleted)
                    {
                        // Safely cast task to Task<(XrResult, string, byte[])>
                        Task<(XrResult, string, byte[])> t = task as Task<(XrResult, string, byte[])>;
                        if (t == null)
                        {
                            Debug.LogError("Task could not be cast to Task<(XrResult, string, byte[])>. Skipping.");
                            continue;
                        }

                        // Safely access t.Result
                        var result = t.Result;
                        if (result.Item1 == XrResult.XR_SUCCESS)
                        {
                            // Write to file
                            string path = GetExportPath();
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }

                            string paname = result.Item2;
                            if (!paname.EndsWith(utf8Word))
                                throw new System.Exception("Anchor name is not ended with " + utf8Word);
                            paname = paname.Substring(0, paname.Length - 1);
                            string file = Path.Combine(path, paname + ".pa");
                            Debug.Log("AnchorTestHandle: Data length: " + result.Item3.Length);
                            File.WriteAllBytes(file, result.Item3);
                            Debug.Log("AnchorTestHandle: Exported anchor to " + file);
                            statusResponse.text = "Exported anchor to " + file;
                        }
                        else
                        {
                            Debug.LogError("AnchorTestHandle: Export persisted anchor failed: " + result.Item1);
                            statusResponse.text = "Export persisted anchor failed: " + result.Item1;
                        }

                        tasks.Remove(task);
                        break;
                    }
                }

                yield return null;
            }

            UINeedUpdate();

            statusResponse.text = "All persisted anchors exported";
        }

        public void OnExportAll()
        {
            Debug.Log("AnchorTestHandle: OnExportAll()");
            // Create export folder
            string path = GetExportPath();
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            OnClearExportedAnchors();

            if (tasks.Count > 0)
            {
                statusResponse.text = "Other task is running, please wait";
                return;
            }

            statusResponse.text = "Exporting anchors";

            if (hasPersistedAnchor1)
            {
                Task t = AnchorManager.ExportPersistedAnchor(persistedAnchor1Name);
                tasks.Add(t);
            }

            if (hasPersistedAnchor2)
            {
                Task t = AnchorManager.ExportPersistedAnchor(persistedAnchor2Name);
                tasks.Add(t);
            }
            if (hasPersistedAnchor3)
            {
                Task t = AnchorManager.ExportPersistedAnchor(persistedAnchor3Name);
                tasks.Add(t);
            }

            // Has tasks, export/import/clear buttons should be disabled
            UINeedUpdate();

            StartCoroutine(WaitExportFinish());
        }

        public IEnumerator WaitImportFinish()
        {
            while (tasks.Count > 0)
            {
                foreach (var task in tasks)
                {
                    if (task.IsCompleted)
                    {
                        tasks.Remove(task);
                        break;
                    }
                }
                yield return null;
            }

            // No tasks, export/import/clear buttons should be enabled
            UINeedUpdate();
            statusResponse.text = "All persisted anchors imported";
        }

        public void OnImportAll()
        {
            Debug.Log("AnchorTestHandle: OnImportAll()");
            if (tasks.Count > 0)
            {
                statusResponse.text = "Other task is running, please wait";
                return;
            }

            var list = GetExportedFilesList();
            if (list == null)
            {
                statusResponse.text = "No exported anchor files";
                return;
            }

            string names = "";
            try
            {
                foreach (var file in list)
                {
                    Task tread = new Task(async () =>
                    {
                        try
                        {
                            Debug.Log("AnchorTestHandle: Read file: " + file);
                            var data = File.ReadAllBytes(file);
                            Debug.Log("AnchorTestHandle: data length: " + data.Length);

                            AnchorManager.GetPersistedAnchorNameFromBuffer(data, out string name);
                            Debug.Log("AnchorTestHandle: data is from " + name);
                            names += name + "\n";

                            Task<XrResult> importTask = AnchorManager.ImportPersistedAnchor(data);
                            await importTask;

                            if (importTask.Result == XrResult.XR_SUCCESS)
                                Debug.Log("AnchorTestHandle: Import persisted anchor success");
                            else
                                Debug.LogError("AnchorTestHandle: Import persisted anchor failed: " + importTask.Result);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError("AnchorTestHandle: Read file " + file + " failed: " + e.Message);
                            statusResponse.text = "Read file " + file + " failed: " + e.Message;
                        }
                    });
                    tread.Start();
                    tasks.Add(tread);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("AnchorTestHandle: Import persisted anchor failed: " + e.Message);
                statusResponse.text = "Import persisted anchor failed: " + e.Message;
            }

            // Has tasks, export/import/clear buttons should be disabled
            UINeedUpdate();
            statusResponse.text = "Importing " + list.Count + "persist anchor(s):\n" + names;


            StartCoroutine(WaitImportFinish());
        }

        public void OnClearExportedAnchors()
        {
            Debug.Log("AnchorTestHandle: OnClearExportedAnchors()");
            var list = GetExportedFilesList();
            if (list != null)
            {
                foreach (var file in list)
                {
                    Debug.Log("AnchorTestHandle: Delete file " + file);
                    File.Delete(file);
                }
                hasPersistedAnchorFiles = false;
                statusResponse.text = "Del " + list.Count + " files";
            }
            UINeedUpdate();
        }

        string GetExportPath()
        {
            return Path.Combine(Application.persistentDataPath, "Anchors");
        }

        List<string> GetExportedFilesList()
        {
            // Get persistance storage path
            // Check if any anchor file in the app's local storage
            string path = GetExportPath();
            if (!Directory.Exists(path)) return null;

            string[] files = Directory.GetFiles(path);
            List<string> anchorFiles = new List<string>();
            if (files.Length == 0) return null;
            foreach (string file in files)
            {
                if (file.EndsWith(".pa"))
                    anchorFiles.Add(file);
            }
            // Print log if files count changed
            if (anchorFiles.Count != lastFoundExportedFilesCount)
            {
                Debug.Log("AnchorTestHandle: Found " + anchorFiles.Count + " anchor files");
                foreach (var file in anchorFiles)
                    Debug.Log("AnchorTestHandle:     " + file);
                lastFoundExportedFilesCount = anchorFiles.Count;
            }

            hasPersistedAnchorFiles = anchorFiles.Count > 0;
            return anchorFiles;
        }

        public void CheckExportedFiles()
        {
            if (hasPersistedAnchorFiles) return;

            GetExportedFilesList();
        }

        public void EnumeratePersistedAnchors()
        {
            if (!isAnchorSupported || !isPAnchorSupported || !isPACollectionAcquired)
                return;

            if (AnchorManager.GetNumberOfPersistedAnchors(out int count) != XrResult.XR_SUCCESS)
            {
                Debug.LogError("AnchorTestHandle: GetNumberOfPersistedAnchors failed");
                return;
            }

            var tmpHasPA1 = hasPersistedAnchor1;
            var tmpHasPA2 = hasPersistedAnchor2;
            var tmpHasPA3 = hasPersistedAnchor3;
            hasPersistedAnchor1 = false;
            hasPersistedAnchor2 = false;
            hasPersistedAnchor3 = false;
            persistedAnchor1Name = null;
            persistedAnchor2Name = null;
            persistedAnchor3Name = null;

            if (count != lastPACount)
            {
                Debug.Log("AnchorTestHandle: GetNumberOfPersistedAnchors=" + count);
                lastPACount = count;
            }

            if (count == 0)
                return;

            if (AnchorManager.EnumeratePersistedAnchorNames(out string[] names) != XrResult.XR_SUCCESS || names == null)
            {
                Debug.LogError("AnchorTestHandle: EnumeratePersistedAnchorNames failed or returned null");
                return;
            }

            foreach (var name in names)
            {
                if (name.EndsWith(utf8Word))
                {
                    if (name.StartsWith("anchor1"))
                    {
                        if (!tmpHasPA1)
                            Debug.Log("AnchorTestHandle: Found persisted anchor1: " + name);

                        hasPersistedAnchor1 = true;
                        persistedAnchor1Name = name;
                    }
                    else if (name.StartsWith("anchor2"))
                    {
                        if (!tmpHasPA2)
                            Debug.Log("AnchorTestHandle: Found persisted anchor2: " + name);
                        hasPersistedAnchor2 = true;
                        persistedAnchor2Name = name;
                    }
                    else if (name.StartsWith("anchor3"))
                    {
                        if (!tmpHasPA3)
                            Debug.Log("AnchorTestHandle: Found persisted anchor3: " + name);
                        hasPersistedAnchor3 = true;
                        persistedAnchor3Name = name;
                    }
                }

                // Handle other names if necessary
            }

            if (hasPersistedAnchor1 != tmpHasPA1 && hasPersistedAnchor1 == false)
                Debug.Log("AnchorTestHandle: Lost persisted anchor1");

            if (hasPersistedAnchor2 != tmpHasPA2 && hasPersistedAnchor2 == false)
                Debug.Log("AnchorTestHandle: Lost persisted anchor2");

            if (hasPersistedAnchor3 != tmpHasPA2 && hasPersistedAnchor3 == false)
                Debug.Log("AnchorTestHandle: Lost persisted anchor3");

            if (hasPersistedAnchor1 != tmpHasPA1 || hasPersistedAnchor2 != tmpHasPA2 || hasPersistedAnchor3 != tmpHasPA3)
                UINeedUpdate();
        }

        /// <summary>
        /// If both anchor and persited anchor are existed, check if the anchor is created from the persisted anchor.  If not, destroy the anchor.
        /// </summary>
        /// <param name="hasPA">Persisted anchor exist</param>
        /// <param name="paName">Persisted anchor's name</param>
        /// <param name="anchor">if anchor is not null, anchor exist</param>
        /// <param name="anchorFromPA">If the anchor is from a persisted anchor, the persisted anchor's name</param>
        /// <returns>If the anchor need be destroyed</returns>
        bool NeedDestroy(bool hasPA, string paName, AnchorManager.Anchor anchor, string anchorFromPA)
        {
            // If no persisted anchor, no need to destroy the anchor.
            if (!hasPA)
                return false;
            // If no anchor, no need to destroy the anchor.
            if (anchor == null)
                return false;
            // If the anchor is created from the same persisted anchor, no need to destroy it.
            if (anchorFromPA == paName)
                return false;
            // If the anchor is created from other persisted anchor, need to destroy it.
            return true;
        }

        bool NeedCancelCFPA(bool hasPA, string paName, FutureTask<(XrResult, AnchorManager.Anchor)> task, string anchorFromPA)
        {
            // If task is already created from the same persisted anchor, no need to cancel it.
            if (anchorFromPA == paName)
                return false;
            // If task is already created from the same persisted anchor, no need to cancel it.
            // If no persisted anchor, keep the task.
            return hasPA && task != null;
        }



        // If persisted anchors are existed, create anchor from them.
        public void UpdateAnchorsIfPersistExist()
        {
            // If both anchor and persited anchor are existed, check if the anchor is created from the persisted anchor.  If not, dispose the anchor.

            // Check if the anchor is realted to the persisted anchor.
            bool a1NeedDispose = NeedDestroy(hasPersistedAnchor1, persistedAnchor1Name, anchor1, anchor1FromPA);
            bool a2NeedDispose = NeedDestroy(hasPersistedAnchor2, persistedAnchor2Name, anchor2, anchor2FromPA);
            bool a3NeedDispose = NeedDestroy(hasPersistedAnchor3, persistedAnchor3Name, anchor3, anchor3FromPA);

            if (a1NeedDispose)
            {
                Debug.Log("AnchorTestHandle: Dispose existed anchor1 because we want to create it from persisted anchor");
                anchor1.Dispose();
                anchor1 = null;
                anchor1FromPA = "";
            }

            if (a2NeedDispose)
            {
                Debug.Log("AnchorTestHandle: Dispose existed anchor2 because we want to create it from persisted anchor");
                anchor2.Dispose();
                anchor2 = null;
                anchor2FromPA = "";
            }

            if (a3NeedDispose)
            {
                Debug.Log("AnchorTestHandle: Dispose existed anchor3 because we want to create it from persisted anchor");
                anchor3.Dispose();
                anchor3 = null;
                anchor3FromPA = "";
            }
            // Use TaskManager to keep the tasks
            var task1 = tmFPA.GetTask(anchor1FromPA);
            var task2 = tmFPA.GetTask(anchor2FromPA);
            var task3 = tmFPA.GetTask(anchor3FromPA);

            // Check if the task is related to the persisted anchor.
            bool cfpa1NeedCancel = NeedCancelCFPA(hasPersistedAnchor1, persistedAnchor1Name, task1, anchor1FromPA);
            bool cfpa2NeedCancel = NeedCancelCFPA(hasPersistedAnchor2, persistedAnchor2Name, task2, anchor2FromPA);
            bool cfpa3NeedCancel = NeedCancelCFPA(hasPersistedAnchor3, persistedAnchor3Name, task2, anchor3FromPA);

            if (cfpa1NeedCancel)
            {
                tmFPA.RemoveTask(task1);
                task1 = null;
                anchor1FromPA = "";
            }

            if (cfpa2NeedCancel)
            {
                tmFPA.RemoveTask(task2);
                task2 = null;
                anchor2FromPA = "";
            }
            if (cfpa3NeedCancel)
            {
                tmFPA.RemoveTask(task3);
                task3 = null;
                anchor3FromPA = "";
            }

            bool needCreateAnchor1 = hasPersistedAnchor1 && anchor1 == null && task1 == null && !string.IsNullOrEmpty(persistedAnchor1Name);
            if (needCreateAnchor1)
            {
                try
                {
                    task1?.Dispose();

                    using (var tempTask = AnchorManager.CreateSpatialAnchorFromPersistedAnchor(
                        persistedAnchor1Name,
                        MakeSpatialAnchorName(persistedAnchor1Name)))
                    {
                        if (tempTask == null)
                        {
                            throw new InvalidOperationException("CreateSpatialAnchorFromPersistedAnchor failed to return a valid task.");
                        }

                        tmFPA.AddTask(persistedAnchor1Name, tempTask);
                        task1 = tempTask;
                        anchor1FromPA = persistedAnchor1Name;
                    }
                }
                catch (FormatException ex)
                {
                    Debug.LogError($"Anchor creation failed due to formatting error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Unexpected exception during anchor creation: {ex.Message}");
                }
            }

            bool needCreateAnchor2 = hasPersistedAnchor2 && anchor2 == null && task2 == null && !string.IsNullOrEmpty(persistedAnchor2Name);
            if (needCreateAnchor2)
            {
                try
                {
                    task2?.Dispose();

                    using (var tempTask = AnchorManager.CreateSpatialAnchorFromPersistedAnchor(
                        persistedAnchor2Name,
                        MakeSpatialAnchorName(persistedAnchor2Name)))
                    {
                        if (tempTask == null)
                        {
                            throw new InvalidOperationException("CreateSpatialAnchorFromPersistedAnchor failed to return a valid task.");
                        }

                        tmFPA.AddTask(persistedAnchor2Name, tempTask);
                        task2 = tempTask;
                        anchor2FromPA = persistedAnchor2Name;
                    }
                }
                catch (FormatException ex)
                {
                    Debug.LogError($"Anchor creation failed due to formatting error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Unexpected exception during anchor creation: {ex.Message}");
                }
            }

            bool needCreateAnchor3 = hasPersistedAnchor3 && anchor3 == null && task3 == null && !string.IsNullOrEmpty(persistedAnchor3Name);
            if (needCreateAnchor3)
            {
                try
                {
                    task3?.Dispose();

                    using (var tempTask = AnchorManager.CreateSpatialAnchorFromPersistedAnchor(
                        persistedAnchor3Name,
                        MakeSpatialAnchorName(persistedAnchor3Name)))
                    {
                        if (tempTask == null)
                        {
                            throw new InvalidOperationException("CreateSpatialAnchorFromPersistedAnchor failed to return a valid task.");
                        }

                        tmFPA.AddTask(persistedAnchor3Name, tempTask);
                        task3 = tempTask;
                        anchor3FromPA = persistedAnchor3Name;
                    }
                }
                catch (FormatException ex)
                {
                    Debug.LogError($"Anchor creation failed due to formatting error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Unexpected exception during anchor creation: {ex.Message}");
                }
            }

            if (task1 != null && task1.IsPollCompleted)
            {
                try
                {
                    var result = task1.Complete();
                    tmFPA.RemoveTask(task1);
                    if (result.Item1 == XrResult.XR_SUCCESS)
                    {
                        anchor1 = result.Item2;
                        Debug.Log($"Anchor created successfully from persisted anchor: {persistedAnchor1Name}");
                    }
                    else
                    {
                        Debug.LogError($"Failed to create anchor from persisted anchor {persistedAnchor1Name}: {result.Item1}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Exception during task completion: {ex.Message}");
                }
                finally
                {
                    task1?.Dispose();
                    task1 = null;
                }
            }


            if (task2 != null && task2.IsPollCompleted)
            {
                try
                {
                    var result = task2.Complete();
                    tmFPA.RemoveTask(task2);
                    if (result.Item1 == XrResult.XR_SUCCESS)
                    {
                        anchor2 = result.Item2;
                        Debug.Log($"Anchor created successfully from persisted anchor: {persistedAnchor2Name}");
                    }
                    else
                    {
                        Debug.LogError($"Failed to create anchor from persisted anchor {persistedAnchor2Name}: {result.Item1}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Exception during task completion: {ex.Message}");
                }
                finally
                {
                    task2?.Dispose();
                    task2 = null;
                }
            }
            if (task3 != null && task3.IsPollCompleted)
            {
                try
                {
                    var result = task3.Complete();
                    tmFPA.RemoveTask(task3);
                    if (result.Item1 == XrResult.XR_SUCCESS)
                    {
                        anchor3 = result.Item2;
                        Debug.Log($"Anchor created successfully from persisted anchor: {persistedAnchor3Name}");
                    }
                    else
                    {
                        Debug.LogError($"Failed to create anchor from persisted anchor {persistedAnchor3Name}: {result.Item1}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Exception during task completion: {ex.Message}");
                }
                finally
                {
                    task3?.Dispose();
                    task3 = null;
                }
            }
        }

        float lastUpdateTimeTAP = -1;

        public void UpdateTrackableAnchorsPose()
        {
            bool panchorObj1ShouldBeSeen = false;
            bool panchorObj2ShouldBeSeen = false;
            bool panchorObj3ShouldBeSeen = false;

            if (anchor1 == null || !anchor1.IsTrackable)
                panchorObj1ShouldBeSeen = false;

            if (anchor2 == null || !anchor2.IsTrackable)
                panchorObj2ShouldBeSeen = false;

            if (anchor3 == null || !anchor3.IsTrackable)
                panchorObj3ShouldBeSeen = false;

            if (anchor1 != null && anchor1.IsTrackable)
            {
                if (AnchorManager.GetTrackingSpacePose(anchor1, out Pose pose))
                {
                    panchorObj1ShouldBeSeen = true;
                    panchorObj1.transform.position = rig.TransformPoint(pose.position);
                    panchorObj1.transform.rotation = rig.rotation * pose.rotation;
                }
                else
                {
                    panchorObj1ShouldBeSeen = false;
                    Debug.LogError("AnchorTestHandle: GetTrackingSpacePose for anchor1 is failed");
                }
            }

            if (anchor2 != null && anchor2.IsTrackable)
            {
                if (AnchorManager.GetTrackingSpacePose(anchor2, out Pose pose))
                {
                    panchorObj2ShouldBeSeen = true;
                    panchorObj2.transform.position = rig.TransformPoint(pose.position);
                    panchorObj2.transform.rotation = rig.rotation * pose.rotation;
                }
                else
                {
                    panchorObj2ShouldBeSeen = false;
                    Debug.LogError("AnchorTestHandle: GetTrackingSpacePose for anchor2 is failed");
                }
            }

            if (anchor3 != null && anchor3.IsTrackable)
            {
                if (AnchorManager.GetTrackingSpacePose(anchor3, out Pose pose))
                {
                    panchorObj3ShouldBeSeen = true;
                    panchorObj3.transform.position = rig.TransformPoint(pose.position);
                    panchorObj3.transform.rotation = rig.rotation * pose.rotation;
                }
                else
                {
                    panchorObj3ShouldBeSeen = false;
                    Debug.LogError("AnchorTestHandle: GetTrackingSpacePose for anchor3 is failed");
                }
            }

            var switchObj1 = panchorObj1ShouldBeSeen != panchorObj1.activeInHierarchy;
            if (switchObj1)
                panchorObj1.SetActive(panchorObj1ShouldBeSeen);

            var switchObj2 = panchorObj2ShouldBeSeen != panchorObj2.activeInHierarchy;
            if (switchObj2)
                panchorObj2.SetActive(panchorObj2ShouldBeSeen);

            var switchObj3 = panchorObj3ShouldBeSeen != panchorObj3.activeInHierarchy;
            if (switchObj3)
                panchorObj3.SetActive(panchorObj3ShouldBeSeen);

            // force update log once if switchObj1 or switchObj2
            if (switchObj1 || switchObj2 || switchObj3)
                lastUpdateTimeTAP = -1;

            if (panchorObj1ShouldBeSeen || panchorObj2ShouldBeSeen || panchorObj3ShouldBeSeen)
            {
                if (Time.time - lastUpdateTimeTAP > 0.25f)
                {
                    StackTraceLogType stackTraceLogTypeOrigin = Application.GetStackTraceLogType(LogType.Log);
                    Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
                    if (panchorObj1ShouldBeSeen)
                    {
                        var pos = panchorObj1.transform.localPosition;
                        var rot = panchorObj1.transform.localRotation.eulerAngles;
                        //statusResponse.text="AnchorTestHandle: panchorObj1 pos=" + pos.x + "," + pos.y + "," + pos.z;
                        Debug.Log("AnchorTestHandle: panchorObj1 rot=" + rot.x + "," + rot.y + "," + rot.z);
                    }

                    if (panchorObj2ShouldBeSeen)
                    {
                        var pos = panchorObj2.transform.localPosition;
                        var rot = panchorObj2.transform.localRotation.eulerAngles;
                        //statusResponse.text = "AnchorTestHandle: panchorObj2 pos=" + pos.x + "," + pos.y + "," + pos.z;
                        Debug.Log("AnchorTestHandle: panchorObj2 rot=" + rot.x + "," + rot.y + "," + rot.z);
                    }
                    if (panchorObj3ShouldBeSeen)
                    {
                        var pos = panchorObj3.transform.localPosition;
                        var rot = panchorObj3.transform.localRotation.eulerAngles;
                        //statusResponse.text = "AnchorTestHandle: panchorObj2 pos=" + pos.x + "," + pos.y + "," + pos.z;
                        Debug.Log("AnchorTestHandle: panchorObj3 rot=" + rot.x + "," + rot.y + "," + rot.z);
                    }
                    Application.SetStackTraceLogType(LogType.Log, stackTraceLogTypeOrigin);
                    lastUpdateTimeTAP = Time.time;
                }
            }
        }


        List<FutureTask<XrResult>> toRemovePA = new List<FutureTask<XrResult>>();
        List<FutureTask<(XrResult, AnchorManager.Anchor)>> toRemoveFPA = new List<FutureTask<(XrResult, AnchorManager.Anchor)>>();

        public void UpdateTasks()
        {
            toRemovePA.Clear();
            // Check persist anchor tasks
            foreach (var taskTuple in tmPA.GetTasks())
            {
                var anchor = taskTuple.Item1;
                var task = taskTuple.Item2;
                if (!AnchorManager.GetSpatialAnchorName(anchor, out string name))
                    Debug.LogError("Faild to get anchor name: " + anchor);

                if (task.IsPollCompleted)
                {
                    toRemovePA.Add(task);
                    if (task.PollResult == XrResult.XR_SUCCESS)
                    {
                        var result = task.Complete();
                        if (result == XrResult.XR_SUCCESS)
                        {
                            statusResponse.text = "AnchorTestHandle: Persist anchor " + anchor + "=" + name + " success";

                        }
                        else
                        {
                            statusResponse.text = "AnchorTestHandle: Persist anchor " + anchor + "=" + name + " failed: " + result;
                        }
                    }
                    else
                    {
                        statusResponse.text = "AnchorTestHandle: Persist anchor " + anchor + "=" + name + " failed: " + task.PollResult;
                    }
                }
            }
            foreach (var task in toRemovePA)
            {
                tmPA.RemoveTask(task);
            }
            toRemovePA.Clear();

            // Check create from persisted anchor tasks
            toRemoveFPA.Clear();

            foreach (var taskTuple in tmFPA.GetTasks())
            {
                var paName = taskTuple.Item1;
                var task = taskTuple.Item2;

                if (task.IsPollCompleted)
                {
                    toRemoveFPA.Add(task);

                    if (task.PollResult == XrResult.XR_SUCCESS)
                    {
                        var result = task.Complete();
                        if (result.Item1 == XrResult.XR_SUCCESS)
                        {
                            Debug.Log($"AnchorTestHandle: Create anchor from persisted anchor {paName} success");

                            if (paName == persistedAnchor1Name)
                            {
                                anchor1 = result.Item2;
                                AnchorManager.GetTrackingSpacePose(anchor1, out Pose pose);
                                CreatePivotFromPose("Wall_1", pose);
                            }
                            else if (paName == persistedAnchor2Name)
                            {
                                anchor2 = result.Item2;
                                AnchorManager.GetTrackingSpacePose(anchor2, out Pose pose);
                                CreatePivotFromPose("Window_1", pose);
                            }
                            else if (paName == persistedAnchor3Name)
                            {
                                anchor3 = result.Item2;
                                AnchorManager.GetTrackingSpacePose(anchor3, out Pose pose);
                                CreatePivotFromPose("Desk", pose);
                            }
                        }
                        else
                        {
                            Debug.LogError($"AnchorTestHandle: Failed to create anchor from {paName}: {result.Item1}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"AnchorTestHandle: Poll failed for {paName}: {task.PollResult}");
                    }

                    // Dispose of the completed or failed task
                    task.Dispose();
                }
            }

            foreach (var task in toRemoveFPA)
            {
                tmFPA.RemoveTask(task);
            }
            toRemoveFPA.Clear();
        }

        void Update()
        {


            if (!isAnchorSupported)
            {
                UpdateUIInteractions();
                return;
            }
        }



        public void OnReloadScene()
        {
            Debug.Log("AnchorTestHandle: OnReloadScene()");
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
