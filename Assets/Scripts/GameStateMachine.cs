using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameState : State
{
    protected GameStateMachine machine;
    protected string sceneName;

    protected IEnumerator UnloadLevel()
    {
        if (!GameManager.Instance.levelScene.isLoaded)
        {
            yield break;
        }

        PrepareTransition();
        AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(GameManager.Instance.levelScene);

        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        GameManager.Instance.levelScene = default;
        GameManager.Instance.baseCamera.SetActive(true);
        GameManager.Instance.canvas.worldCamera = Camera.main;
    }

    protected IEnumerator LoadLevel()
    {
        if (GameManager.Instance.levelScene != default)
        {
            yield break;
        }

        PrepareTransition();

        GameManager.Instance.loadingArt.transform.SetAsLastSibling();
        GameManager.Instance.loadingArt.SetActive(true);
        LoadingScreen loadingScreen = GameManager.Instance.loadingArt.GetComponent<LoadingScreen>();
        loadingScreen.SetProgress(0);

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, 
            LoadSceneMode.Additive);

        while (!asyncOperation.isDone)
        {
            loadingScreen.SetProgress(asyncOperation.progress);
            yield return null;
        }

        GameManager.Instance.loadingArt.SetActive(false);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

        GameManager.Instance.baseCamera.SetActive(false);
        GameManager.Instance.canvas.worldCamera = Camera.main;
        GameManager.Instance.minimap.gameObject.SetActive(true);
    }

    private void PrepareTransition()
    {
        WindowManager.Instance.CloseAllWindows();
        GameManager.Instance.minimap.gameObject.SetActive(false);
    }
}

public class MenuState : GameState
{
    public MenuState(GameStateMachine machine, string sceneName)
    {
        this.machine = machine;
        this.sceneName = sceneName;
    }

    public override void OnEnter()
    {
        GameManager.Instance.mainMenu.SetActive(true);
        machine.State = this;
    }

    public override IEnumerator Main()
    {
        yield return UnloadLevel();
        GameManager.Instance.baseScene = SceneManager.GetActiveScene();
    }

    public override void OnExit()
    {
        machine.Task = null;
    }
}

public class PlayState : GameState
{
    public PlayState(GameStateMachine machine, string sceneName)
    {
        this.machine = machine;
        this.sceneName = sceneName;
    }

    public override void OnEnter()
    {
        machine.State = this;
        GameManager.Instance.mainMenu.SetActive(false);
    }

    public override IEnumerator Main()
    {
        yield return UnloadLevel();
        yield return LoadLevel();
        GameManager.Instance.levelScene = SceneManager.GetActiveScene();
    }

    public override void OnExit()
    {
        machine.Task = null;
    }
}

public class GameStateMachine : StateMachine
{
    public MenuState menuState;
    public override State DefaultState { get => menuState; }
}