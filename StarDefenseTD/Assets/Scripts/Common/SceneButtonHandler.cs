using UnityEngine;

public class SceneButtonHandler : MonoBehaviour
{
    public void GoToTitle()
    {
        GameModeManager.Instance.LoadTitleScene();
    }

    public void GoToLobby()
    {
        GameModeManager.Instance.LoadLobbyScene();
    }

    public void GoToStage()
    {
        GameModeManager.Instance.LoadStageScene();
    }

    public void ReloadScene()
    {
        GameModeManager.Instance.ReloadCurrentScene();
    }

    public void LoadScene(string sceneName)
    {
        GameModeManager.Instance.LoadScene(sceneName);
    }
}