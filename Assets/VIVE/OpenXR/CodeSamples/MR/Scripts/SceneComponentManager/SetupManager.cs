using com.HTC.WVRLoader;
using HTC.UnityPlugin.Vive;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VIVE.OpenXR.Toolkits;


namespace VIVE.OpenXR.Samples.Anchor
{
    public class SetupManager : MonoBehaviour
    {
        [SerializeField] private ScenePlaneGenerator generator;

        public PivotGizmo pivotGizmoPrefab;
        public Transform pivotGizmoRoot;

        [SerializeField] private Button windowButton;
        [SerializeField] private Button deskButton;
        [SerializeField] private Button wallButton;

        [SerializeField] private GameObject lightGizmo;
        [SerializeField] private Transform lightDeskGizmo;
        [SerializeField] private Light dirLight;

        private string currentKey = null;
        private PlaneController deskController;
        [SerializeField] private AnchorTest anchorTestHandle;
        public Text txt;

        public Dictionary<string, PivotGizmo> pivotGizmoMap = new Dictionary<string, PivotGizmo>();
        private Dictionary<ShapeTypeEnum, System.Action<PlaneController, string>> pivotSetupHandlerMap;

        readonly FutureTaskManager<XrSpace, XrResult> tmPA = new FutureTaskManager<XrSpace, XrResult>();


        void Start()
        {

            if (anchorTestHandle == null)
            {
                return;
            }

            SceneComponentManager.Instance.GenerateScenePlanes(generator);

            InitializeButtons();

            EnableManualPivotSelection1();
            EnableManualPivotSelection2();

            anchorTestHandle.isSetDesk = true;
            anchorTestHandle.isSetWall = true;
            anchorTestHandle.isSetWindow = true;
            anchorTestHandle.UINeedUpdate();

        }


        private void InitializeButtons()
        {
            if (windowButton != null)
            {
                windowButton.onClick.AddListener(() => SetCurrentKey("Window_1"));
            }

            if (deskButton != null)
            {
                deskButton.onClick.AddListener(() => SetCurrentKey("Desk"));
            }

            if (wallButton != null)
            {
                wallButton.onClick.AddListener(() => SetCurrentKey("Wall_1"));
            }


        }
        public void OnLightBtnClicked()
        {
            currentKey = null;
            lightGizmo.gameObject.SetActive(true);

            if (deskController != null)
            {
                lightDeskGizmo.position = deskController.transform.position + Vector3.up * 0.025f;
                lightDeskGizmo.forward = CalculateDeskForward(deskController);
            }
        }



        private void Update()
        {
            if (lightGizmo.gameObject.activeSelf)
            {
                if (ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Trigger))
                {
                    PivotManager.Instance.SetLight(dirLight.transform.position, dirLight.transform.forward);
                    lightGizmo.gameObject.SetActive(false);
                }
            }


            anchorTestHandle.UpdateTasks();

            anchorTestHandle.CheckExportedFiles();

            anchorTestHandle.EnumeratePersistedAnchors();

            anchorTestHandle.UpdateAnchorsIfPersistExist();

            anchorTestHandle.UpdateTrackableAnchorsPose();

            anchorTestHandle.UpdateUIInteractions();


#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StartGame();
            }
#endif
        }

        private void SetCurrentKey(string key)
        {
            currentKey = key;
            Debug.Log($"Current key set to: {currentKey}");
        }

        public void EnableManualPivotSelection1()
        {
            if (SceneComponentManager.Instance == null || SceneComponentManager.Instance.PlaneControllers == null)
            {
                Debug.LogError("SceneComponentManager or PlaneControllers is null. Ensure it is initialized before calling this method.");
                return;
            }

            // Wall
            foreach (PlaneController planeController in SceneComponentManager.Instance.PlaneControllers)
            {
                if (planeController == null)
                {
                    Debug.LogWarning("Found a null PlaneController. Skipping this iteration.");
                    continue;
                }

                var setupPlaneController = planeController as SetupPlaneController;
                if (setupPlaneController != null)
                {
                    setupPlaneController.OnClickedHandler += OnScenePlaneClicked;
                }
                else
                {
                    Debug.LogWarning($"PlaneController {planeController.name} is not a SetupPlaneController.");
                }

                if (planeController.ShapeType == ShapeTypeEnum.table)
                {
                    deskController = planeController;
                }
            }

            pivotSetupHandlerMap = new Dictionary<ShapeTypeEnum, System.Action<PlaneController, string>>
        {
               
            { ShapeTypeEnum.wall, SetPivotOnVerticalPlane1 },
            { ShapeTypeEnum.table, SetPivotOnHorizontalPlane }
        };
        }
        //update 2024/12/13
        public void EnableManualPivotSelection2()
        {
            if (SceneComponentManager.Instance == null || SceneComponentManager.Instance.PlaneControllers == null)
            {
                Debug.LogError("SceneComponentManager or PlaneControllers is null. Ensure it is initialized before calling this method.");
                return;
            }

            // Window
            foreach (PlaneController planeController in SceneComponentManager.Instance.PlaneControllers)
            {
                if (planeController == null)
                {
                    Debug.LogWarning("Found a null PlaneController. Skipping this iteration.");
                    continue;
                }

                var setupPlaneController = planeController as SetupPlaneController;
                if (setupPlaneController != null)
                {
                    setupPlaneController.OnClickedHandler += OnScenePlaneClicked;
                }
                else
                {
                    Debug.LogWarning($"PlaneController {planeController.name} is not a SetupPlaneController.");
                }

                if (planeController.ShapeType == ShapeTypeEnum.table)
                {
                    deskController = planeController;
                }
            }

            pivotSetupHandlerMap = new Dictionary<ShapeTypeEnum, System.Action<PlaneController, string>>
        {
                  { ShapeTypeEnum.window, SetPivotOnVerticalPlane2 },
           
            { ShapeTypeEnum.table, SetPivotOnHorizontalPlane }
        };
        }
        private void OnScenePlaneClicked(PlaneController planeController)
        {

            if (!string.IsNullOrEmpty(currentKey))
            {
                pivotSetupHandlerMap[planeController.ShapeType]?.Invoke(planeController, currentKey);
            }
        }

        private void SetPivotOnVerticalPlane1(PlaneController planeController, string pivotKey)
        {
            if (planeController == null)
            {
                Debug.LogError("PlaneController is null. Cannot set pivot on vertical plane.");
                return;
            }

            if (anchorTestHandle == null)
            {
                Debug.LogError("AnchorTestHandle is null. Cannot set anchor positions.");
                return;
            }

            Vector3 pos = planeController.transform.position;
            Vector3 forward = -planeController.transform.forward;

            anchorTestHandle.anchorPose1.position = pos;
            anchorTestHandle.anchorPose1.rotation = Quaternion.LookRotation(forward);

            if (PivotManager.Instance != null)
            {
                PivotManager.Instance.SetPivot(pivotKey, pos, forward);
            }
            else
            {
                Debug.LogError("PivotManager.Instance is null. Cannot set pivot.");
            }

            if (pivotKey == "Wall_1" && currentKey == "Wall_1")
                anchorTestHandle.isSetWall = false;

          

            anchorTestHandle.UINeedUpdate();
        }

        private void SetPivotOnVerticalPlane2(PlaneController planeController, string pivotKey)
        {
            if (planeController == null)
            {
                Debug.LogError("PlaneController is null. Cannot set pivot on vertical plane.");
                return;
            }

            if (anchorTestHandle == null)
            {
                Debug.LogError("AnchorTestHandle is null. Cannot set anchor positions.");
                return;
            }

            Vector3 pos = planeController.transform.position;
            Vector3 forward = -planeController.transform.forward;

            anchorTestHandle.anchorPose2.position = pos;
            anchorTestHandle.anchorPose2.rotation = Quaternion.LookRotation(forward);

            if (PivotManager.Instance != null)
            {
                PivotManager.Instance.SetPivot(pivotKey, pos, forward);
            }
            else
            {
                Debug.LogError("PivotManager.Instance is null. Cannot set pivot.");
            }

            if (pivotKey == "Window_1" && currentKey == "Window_1")
                anchorTestHandle.isSetWindow = false;

           

            anchorTestHandle.UINeedUpdate();
        }

        private void SetPivotOnHorizontalPlane(PlaneController planeController, string pivotKey)
        {
            if (planeController == null)
            {
                Debug.LogError("PlaneController is null. Cannot set pivot on horizontal plane.");
                return;
            }

            if (anchorTestHandle == null)
            {
                Debug.LogError("AnchorTestHandle is null. Cannot set anchor positions.");
                return;
            }

            Vector3 pos = planeController.transform.position;
            Vector3 forward = CalculateDeskForward(planeController);

            anchorTestHandle.anchorPose3.position = pos;
            anchorTestHandle.anchorPose3.rotation = Quaternion.LookRotation(forward);

            if (PivotManager.Instance != null)
            {
                PivotManager.Instance.SetPivot(pivotKey, pos, forward);
            }
            else
            {
                Debug.LogError("PivotManager.Instance is null. Cannot set pivot.");
            }

            if (pivotKey == "Desk" && currentKey == "Desk")
                anchorTestHandle.isSetDesk = false;

            anchorTestHandle.UINeedUpdate();
        }

        private static Vector3 CalculateDeskForward(PlaneController planeController)
        {
            if (planeController == null)
            {
                Debug.LogError("PlaneController is null. Cannot calculate desk forward direction.");
                return Vector3.forward;
            }

            if (planeController.Data?.Points == null || planeController.Data.Points.Length < 4)
            {
                Debug.LogError("PlaneController.Data.Points is null or insufficient. Cannot calculate desk forward direction.");
                return Vector3.forward;
            }

            Vector3 pos = planeController.transform.position;
            Vector3 horizontalDir = Vector3.Scale(pos - Camera.main.transform.position, new Vector3(1, 0, 1)).normalized;

            Vector3 vec1 = (planeController.Data.Points[1] - planeController.Data.Points[0]).normalized;
            Vector3 vec2 = (planeController.Data.Points[3] - planeController.Data.Points[0]).normalized;

            return Vector3.Dot(horizontalDir, vec1) < Vector3.Dot(horizontalDir, vec2) ? vec1 : vec2;
        }

        public void SavePivots()
        {
            if (PivotManager.Instance != null)
            {
                PivotManager.Instance.Save();
            }
            else
            {
                Debug.LogError("PivotManager.Instance is null. Cannot save pivots.");
            }

            if (anchorTestHandle != null)
            {
                anchorTestHandle.OnExportAll();
            }
            else
            {
                Debug.LogError("AnchorTestHandle is null. Cannot export all.");
            }

            Debug.Log("All pivots saved.");
        }

        public void StartGame()
        {

            EventMediator.LeaveSetupMode();

        }
    }
}