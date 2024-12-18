using com.HTC.Common;
using com.HTC.WVRLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VIVE.OpenXR;
using VIVE.OpenXR.PlaneDetection;
using VIVE.OpenXR.Toolkits.PlaneDetection;


public class SceneComponentManager : Singleton<SceneComponentManager>
{
    private const string LOG_TAG = "JelbeeMR";

    public Transform root;

    public int count = 0;

    [SerializeField] private ScenePlaneGenerator generator;

    public List<PlaneData> Planes = new List<PlaneData>();

    private List<PlaneController> planeControllers;
    public List<PlaneController> PlaneControllers { get { return planeControllers; } }

    private bool isPlaneActivated = true;

    IEnumerator Start()
    {
        if (!PlaneDetectionManager.IsSupported()) yield break;
    }
    public bool IsPlanesActivated
    {
        get
        {
            return isPlaneActivated;
        }
        set
        {
            isPlaneActivated = value;
            foreach (PlaneController planeController in PlaneControllers)
            {
                planeController.gameObject.SetActive(isPlaneActivated);
            }
        }
    }

    public void LoadScenePlanes()
    {
#if UNITY_EDITOR
        LoadScenePlaneFromTestFiles();
#else
        
        LoadScenePlaneFromAPI();
#endif
    }

    float time = 0;

    public IEnumerator GetAllPlanes()
    {
        if (!PlaneDetectionManager.IsSupported())
        {
            yield break;
        }

        var pd = PlaneDetectionManager.CreatePlaneDetector();
        if (pd == null)
        {
            yield break;
        }

        pd.BeginPlaneDetection();
        yield return null;

        var state = pd.GetPlaneDetectionState();
        bool isDone = false;
        time = 0;
        while (isDone)
        {
            switch (state)
            {
                case VivePlaneDetection.XrPlaneDetectionStateEXT.DONE_EXT:
                    Debug.Log("GetAllPlanes() state: " + state);
                    isDone = true;
                    break;
                case VivePlaneDetection.XrPlaneDetectionStateEXT.PENDING_EXT:
                    if (time + 0.5f > Time.unscaledTime)
                    {
                        time = Time.unscaledTime;
                        Debug.Log("GetAllPlanes() state: " + state);
                    }
                    yield return null;
                    continue;
                case VivePlaneDetection.XrPlaneDetectionStateEXT.NONE_EXT:
                case VivePlaneDetection.XrPlaneDetectionStateEXT.FATAL_EXT:
                case VivePlaneDetection.XrPlaneDetectionStateEXT.ERROR_EXT:
                    Debug.Log("GetAllPlanes() state: " + state);
                    PlaneDetectionManager.DestroyPlaneDetector(pd);
                    yield break;
            }
            yield return null;
            state = pd.GetPlaneDetectionState();
        }

        List<PlaneDetectorLocation> locations;
        if (pd.GetPlaneDetections(out locations) != XrResult.XR_SUCCESS)
        {
            yield break;
        }

        foreach (var location in locations)
        {
            count++;
            Debug.Log("GetAllPlanes() location.planeId: " + location.planeId);
            Debug.Log("GetAllPlanes() location.locationFlags: " + location.locationFlags);
            Debug.Log("GetAllPlanes() location.pose: " + location.pose);
            Debug.Log("GetAllPlanes() location.pose.rotation.eulerAngles: " + location.pose.rotation.eulerAngles);
            Debug.Log("GetAllPlanes() location.scale: " + location.size);
            Debug.Log("GetAllPlanes() location.orientation: " + location.orientation);
            Debug.Log("GetAllPlanes() location.semanticType: " + location.semanticType);
            Debug.Log("GetAllPlanes() location.polygonBufferCount: " + location.polygonBufferCount);
            var plane = pd.GetPlane(location.planeId);
            if (plane == null)
                continue;

            Debug.Log("GetAllPlanes() plane.scale: " + plane.scale);
            Debug.Log("GetAllPlanes() plane.center: " + plane.center);

            PlaneData planeData = ConvertOpenXRPlaneToPlaneData(location, plane);

            Planes.Add(planeData);


        }

        GenerateScenePlanes(generator);

        PlaneDetectionManager.DestroyPlaneDetector(pd);
    }
    List<GameObject> existPlanes = new List<GameObject>();

    public void OnDetectPlane()
    {
        OnClearObjects();
        CheckSupport();
        StartCoroutine(GetAllPlanes());
    }
    private void LoadScenePlaneFromAPI()
    {
        Invoke("OnDetectPlane", 1);
    }

    public void OnClearObjects()
    {
        Debug.Log("OnClearObjects()");
        CheckSupport();
        existPlanes.ForEach((obj) => Destroy(obj));
    }

    public bool CheckSupport()
    {
        if (!PlaneDetectionManager.IsSupported())
        {
            throw new NotSupportedException("PlaneDetection is not supported.");
        }
        return true;
    }


    private PlaneData ConvertOpenXRPlaneToPlaneData(PlaneDetectorLocation location, VIVE.OpenXR.Toolkits.PlaneDetection.Plane openxrPlane)
    {
        PlaneData planeData = new PlaneData();

        Pose planePose = location.pose;
        Vector3 planePositionUnity = planePose.position;
        Quaternion planeRotationUnity = planePose.rotation * Quaternion.Euler(90f, 0f, 0f);

        // Transform matrix to apply position and rotation to vertices
        Matrix4x4 trs = Matrix4x4.TRS(planePositionUnity, planeRotationUnity, Vector3.one);

        Vector3[] vertices = openxrPlane.verticesRaw;
        if (vertices.Length >= 4)
        {
            planeData.Points = new Vector3[4];

            planeData.Points[0] = trs.MultiplyPoint(vertices[3]);
            planeData.Points[1] = trs.MultiplyPoint(vertices[2]);
            planeData.Points[2] = trs.MultiplyPoint(vertices[1]);
            planeData.Points[3] = trs.MultiplyPoint(vertices[0]);
        }
        else
        {
            Debug.LogWarning("Insufficient vertices provided by OpenXR plane data.");
            return null;
        }

        // Calculate the center of the plane
        planeData.Center = (planeData.Points[0] + planeData.Points[2]) / 2f;
        planeData.Width = location.size.x;
        planeData.Height = location.size.y;
        planeData.Type = PlaneLabelToStr(location.semanticType);
        planeData.UID = Guid.NewGuid().ToString();

        // Orientation as Vector4 (Quaternion)
        planeData.Orientation = new Vector4(
            planeRotationUnity.x,
            planeRotationUnity.y,
            planeRotationUnity.z,
            planeRotationUnity.w
        );

        return planeData;
    }


    private static string PlaneLabelToStr(VivePlaneDetection.XrPlaneDetectorSemanticTypeEXT label)
    {
        switch (label)
        {

            //case VivePlaneDetection.XrPlaneDetectorSemanticTypeEXT.CEILING_EXT: return "window";
            case VivePlaneDetection.XrPlaneDetectorSemanticTypeEXT.FLOOR_EXT: return "floor";
            case VivePlaneDetection.XrPlaneDetectorSemanticTypeEXT.WALL_EXT: return "wall";
            case VivePlaneDetection.XrPlaneDetectorSemanticTypeEXT.PLATFORM_EXT: return "table";
            default: return "window";
        }
    }

    private void LoadScenePlaneFromTestFiles()
    {
        TextAsset asset = Resources.Load<TextAsset>("SampleSceneComponents");
        if (asset != null)
        {
            string text = asset.text;
            Resources.UnloadAsset(asset);

            List<PlaneData> planes;
            ParseToObjects(text, out planes);

            Planes = planes;
        }
        else
        {
            Planes = new List<PlaneData>();
        }
    }
    private static bool ParseToObjects<T>(string str, out List<T> sceneObjs) where T : SceneObjectData, new()
    {
        try
        {
            string[] shapeStrs = str.Trim().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            sceneObjs = new List<T>();

            foreach (string shapeStr in shapeStrs)
            {
                T obj = new T();
                if (obj.FromString(shapeStr))
                {
                    sceneObjs.Add(obj);
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.Log("Error occurred when parsing scene components from file: " + e.Message);
            sceneObjs = null;
            return false;
        }
    }

    public void GenerateScenePlanes(ScenePlaneGenerator generator)
    {
        if (planeControllers != null)
        {
            foreach (PlaneController plane in planeControllers)
            {
                Destroy(plane.gameObject);
            }
            planeControllers = null;
        }
        planeControllers = generator.GenerateScenePlanes(Planes);
        foreach (PlaneController planeController in PlaneControllers)
        {
            planeController.gameObject.SetActive(isPlaneActivated);
        }
    }

    private static class Log
    {
        public static void i(string tag, string msg, bool inEditor = false)
        {
            Debug.LogFormat("{0} {1}", tag, msg);
        }

        public static void e(string tag, string msg, bool inEditor = false)
        {
            Debug.LogFormat("{0} {1}", tag, msg);
        }
    }
}