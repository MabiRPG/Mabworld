using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameState : State
{
    protected GameStateMachine machine;
    protected string sceneName;

    protected IEnumerator LoadScene()
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, 
            LoadSceneMode.Additive);

        GameManager.Instance.loadingArt.transform.SetAsLastSibling();
        GameManager.Instance.loadingArt.SetActive(true);
        LoadingScreen loadingScreen = GameManager.Instance.loadingArt.GetComponent<LoadingScreen>();
        loadingScreen.SetProgress(0);

        while (!asyncOperation.isDone)
        {
            loadingScreen.SetProgress(asyncOperation.progress);
            yield return null;
        }

        GameManager.Instance.loadingArt.SetActive(false);
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
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            yield return LoadScene();
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }

        InputController.Instance.SetBlockMouse(true);
        InputController.Instance.SetBlockKeyboard(true);
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
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            yield return LoadScene();
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }

        InputController.Instance.SetBlockMouse(false);
        InputController.Instance.SetBlockKeyboard(false);
        Camera.main.gameObject.SetActive(false);
        GameManager.Instance.canvas.worldCamera = Camera.main;        
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