using com.HTC.Common;
using HTC.UnityPlugin.Vive;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MRSceneManager : MonoBehaviour
{
    [SerializeField]
    private bool enterSetup = false;

    [SerializeField]
    private PassthroughSetting passthrough;

    [SerializeField]
    private ScenePlaneGenerator planeGenerator;

    private const string SetupScene = "Setup";
    private const string GameFlow = "GameFlow";
    private const string RobotAssistant = "RobotAssistant";
    private const string LOG_TAG = "JelbeeMR";

    private MRScenePerceptionHelper scenePerceptionHelper;
    private bool isGameSceneLoaded = false;

    public bool isLoadSetup = false;

    protected void Awake()
    {
        isGameSceneLoaded = false;
        EventMediator.LeaveSetupMode += LeaveSetupMode;
        EventMediator.RestartGame += RestartGame;
    }

    private void OnDestroy()
    {
        EventMediator.LeaveSetupMode -= LeaveSetupMode;
        EventMediator.RestartGame -= RestartGame;
    }

    private void RestartGame()
    {
        SceneManager.UnloadSceneAsync(GameFlow);
        SceneManager.UnloadSceneAsync(RobotAssistant);
        SceneManager.LoadSceneAsync(GameFlow, LoadSceneMode.Additive);
        SceneManager.LoadSceneAsync(RobotAssistant, LoadSceneMode.Additive);
    }

    private void LeaveSetupMode()
    {
        isLoadSetup = false;
        SceneManager.UnloadSceneAsync(SetupScene);

        Initialize();
    }

    private void EnterSetupMode()
    {
        if (isGameSceneLoaded)
        {
            SceneManager.UnloadSceneAsync(GameFlow);
            SceneManager.UnloadSceneAsync(RobotAssistant);
            isGameSceneLoaded = false;
        }
        isLoadSetup = true;
        SceneManager.LoadSceneAsync(SetupScene, LoadSceneMode.Additive);
    }

    //#if UNITY_EDITOR
    private void Start()
    {

        SceneComponentManager.Instance.LoadScenePlanes();

        if (enterSetup)
        {
            EnterSetupMode();
            return;
        }

        Initialize();
    }
    /*
    #else
        private IEnumerator Start()
        {
            scenePerceptionHelper = new MRScenePerceptionHelper();
            scenePerceptionHelper.OnEnable();

            //wait for ScenePerceptionManager start scene
            yield return new WaitUntil(() => scenePerceptionHelper.isSceneComponentRunning == true);
            scenePerceptionHelper.StartScenePerception(onPerceptionStartSuccess, onPerceptionStartFailed);
        }

        private void onPerceptionStartSuccess()
        {
            SceneComponentManager.Instance.LoadScenePlanes();
            initialize();
        }

        private void onPerceptionStartFailed()
        {
            Log.e(LOG_TAG, "Start scene perception error, please check setting page and make sure the TRACKING MODE is set to DEFAULT");
        }



        private void OnDisable()
        {
            if (scenePerceptionHelper != null)
                scenePerceptionHelper.OnDisable();
        }
    #endif*/
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

    public void Initialize()
    {
        Log.i(LOG_TAG, "initialize");

        if (SceneComponentManager.Instance != null)
        {
            SceneComponentManager.Instance.GenerateScenePlanes(planeGenerator);
        }
        else
        {
            Debug.LogError("SceneComponentManager.Instance is null. Cannot generate scene planes.");
        }

 
        if (PivotManager.Instance != null)
        {
            if (PivotManager.Instance.IsSaveFileExisted)
            {
                PivotManager.Instance.Load();
            }
            else
            {
                PivotManager.Instance.AutoDetect();
            }

            if (PivotManager.Instance.IsValid)
            {
                if (passthrough != null)
                {
                    passthrough.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogError("Passthrough object is null. Cannot set active.");
                }

                SceneManager.LoadSceneAsync(GameFlow, LoadSceneMode.Additive);
                SceneManager.LoadSceneAsync(RobotAssistant, LoadSceneMode.Additive);
                isGameSceneLoaded = true;
            }
        }
        else
        {
            Debug.LogError("PivotManager.Instance is null. Cannot load or auto-detect pivots.");
        }
    }
    public void enableSetup()
    {
        if (ViveInput.GetPressDownEx(ControllerRole.LeftHand, ControllerButton.AKey) || ViveInput.GetPressDownEx(ControllerRole.RightHand, ControllerButton.AKey))
        {
            if (isLoadSetup == false)
                EnterSetupMode();
        }

    }


    private void Update()
    {
        Invoke("enableSetup", 1);

    }
}
