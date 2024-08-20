using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private Image splashArt;
    private Image loadingArt;

    [SerializeField]
    private GameObject buttonParent;
    private Button startButton;
    private Button saveButton;
    private Button loadButton;
    private Button creditsButton;
    private Button optionsButton;
    private Button quitButton;

    private void Awake()
    {
        splashArt = transform.Find("Splash Art").GetComponent<Image>();
        loadingArt = transform.Find("Loading Art").GetComponent<Image>();

        startButton = buttonParent.transform.Find("Start Button").GetComponent<Button>();
        saveButton = buttonParent.transform.Find("Save Button").GetComponent<Button>();
        loadButton = buttonParent.transform.Find("Load Button").GetComponent<Button>();
        creditsButton = buttonParent.transform.Find("Credits Button").GetComponent<Button>();
        optionsButton = buttonParent.transform.Find("Options Button").GetComponent<Button>();
        quitButton = buttonParent.transform.Find("Quit Button").GetComponent<Button>();
    }

    private void OnEnable()
    {
        startButton.onClick.AddListener(StartGame);
        saveButton.onClick.AddListener(SaveGame);
        loadButton.onClick.AddListener(LoadGame);
        creditsButton.onClick.AddListener(OpenCredits);
        optionsButton.onClick.AddListener(OpenOptions);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void Start()
    {
        splashArt.gameObject.SetActive(true);
        loadingArt.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        startButton.onClick.RemoveAllListeners();
        saveButton.onClick.RemoveAllListeners();
        loadButton.onClick.RemoveAllListeners();
        creditsButton.onClick.RemoveAllListeners();
        optionsButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();
    }

    private void StartGame()
    {
        StartCoroutine(LoadScene("Test Map"));
    }

    private void SaveGame()
    {
    }

    private void LoadGame()
    {
    }

    private void OpenCredits()
    {
    }

    private void OpenOptions()
    {
    }

    private void QuitGame()
    {
        Application.Quit();
    }

    private IEnumerator LoadScene(string sceneName)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            Debug.Log($"{asyncOperation.progress * 100}");
            yield return null;

            if (asyncOperation.progress >= 0.9f)
            {
                asyncOperation.allowSceneActivation = true;
            }
        }
    }
}
