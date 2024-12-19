using System.Collections;
using UnityEngine;
public enum EnvType
{
    none = 0,
    Universe = 1,
    Forest = 2,
}
public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance;

    public GameObject[] envs;
    public EnvType currentEnv = EnvType.none;

    //queue
    private GameObject currentEnvObj;
    private GameObject nextEnvObj;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple instances of EnvironmentManager detected. Destroying the duplicate.");

            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void OnPortalOpen()
    {
        if (MRTutorialManager.Instance != null)
        {
            MRTutorialManager.Instance.ForceStopTutorial();
        }
        else
        {
            Debug.LogError("MRTutorialManager.Instance is null. Cannot stop tutorial.");
        }

        if (PortalSceneManager.Instance != null)
        {
            switch (PortalSceneManager.Instance.GetCurrentPortalOrder())
            {
                case 0:
                    ChangeEnvironment(EnvType.Forest);
                    break;
                case 1:
                    ChangeEnvironment(EnvType.Universe);
                    break;
                default:
                    Debug.LogWarning("Invalid portal order returned from PortalSceneManager.");
                    break;
            }
        }
        else
        {
            Debug.LogError("PortalSceneManager.Instance is null. Cannot get current portal order.");
        }

        switch (currentEnv)
        {
            case EnvType.Universe:
                if (MRTutorialManager.Instance != null)
                {
                    MRTutorialManager.Instance.ShowTutorial("Tutorial4_Planet", false);
                }
                else
                {
                    Debug.LogError("MRTutorialManager.Instance is null. Cannot show tutorial for Universe.");
                }

                if (BubbleLauncher.Instance != null)
                {
                    BubbleLauncher.Instance.gameObject.SetActive(false);
                }
                else
                {
                    Debug.LogError("BubbleLauncher.Instance is null. Cannot deactivate BubbleLauncher.");
                }
                break;
        }
    }


    public void ChangeEnvironment(EnvType envType)
    {
        if (currentEnvObj == envs[(int)envType - 1]) return;
        currentEnv = envType;
        nextEnvObj = envs[(int)envType - 1];
        StartCoroutine(OnChangeEnvironmentCoroutine(envType));
    }

    private IEnumerator OnChangeEnvironmentCoroutine(EnvType envType)
    {
        if (currentEnvObj)
        {
            nextEnvObj.SetActive(true);
            float currentValue = 1;
            float nextValue = 0;
            while (currentValue >= 0)
            {
                currentValue -= Time.deltaTime;
                nextValue += Time.deltaTime;
                currentEnvObj.GetComponent<MeshRenderer>().material.SetFloat("_ErosionValue", currentValue);
                nextEnvObj.GetComponent<MeshRenderer>().material.SetFloat("_ErosionValue", nextValue);
                yield return null;
            }
            currentEnvObj.SetActive(false);
        }
        else
        {
            nextEnvObj.SetActive(true);
            float nextValue = 0;
            while (nextValue < 1)
            {
                nextValue += Time.deltaTime * 0.3f;
                nextEnvObj.GetComponent<MeshRenderer>().material.SetFloat("_ErosionValue", nextValue);
                yield return null;
            }
        }

        currentEnvObj = nextEnvObj;
        nextEnvObj = null;
        yield return null;
    }
}