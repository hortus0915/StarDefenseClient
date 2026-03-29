using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeManager : MonoBehaviour
{
    private static GameModeManager instance;

    [SerializeField] private string titleSceneName = "TitleScene";
    [SerializeField] private string lobbySceneName = "LobbyScene";
    [SerializeField] private string stageSceneName = "StageScene";

    private bool isLoading;

    public static GameModeManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameModeManager>();

                if (instance == null)
                {
                    GameObject managerObject = new GameObject("GameModeManager");
                    instance = managerObject.AddComponent<GameModeManager>();
                }
            }

            return instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        _ = Instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadTitleScene()
    {
        LoadScene(titleSceneName);
    }

    public void LoadLobbyScene()
    {
        LoadScene(lobbySceneName);
    }

    public void LoadStageScene()
    {
        LoadScene(stageSceneName);
    }

    public void ReloadCurrentScene()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadScene(string sceneName)
    {
        if (isLoading || string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        if (loadOperation == null)
        {
            isLoading = false;
            yield break;
        }

        while (loadOperation.isDone == false)
        {
            yield return null;
        }

        isLoading = false;
    }
}