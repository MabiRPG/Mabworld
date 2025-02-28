using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Data;
using UnityEngine.AddressableAssets;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

/// <summary>
///     This class handles all game-wide processing. Refer to Game.instance for the 
///     specific instance.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class GameManager : MonoBehaviour
{
    // Global instance of GameManager
    public static GameManager Instance { get; private set; }

    [NonSerialized]
    [JsonIgnore]
    public InputController inputController;
    [NonSerialized]
    [JsonIgnore]
    public LightController lightController;
    [NonSerialized]
    [JsonIgnore]
    public AudioController audioController;
    [NonSerialized]
    [JsonIgnore]
    public WindowManager windowManager;
    [JsonIgnore]
    public GameObject overlay;

    [Header("Global Variables")]
    // Name of the game database in Assets/Database folder.
    [SerializeField]
    [JsonIgnore]
    private string databaseName;
    [JsonIgnore]
    public DatabaseManager Database;
    // Base success rate of life skills
    [JsonIgnore]
    public float lifeSkillBaseSuccessRate;

    [Header("Universal Prefabs")]
    [JsonIgnore]
    public GameObject playerPrefab;
    [JsonIgnore]
    public GameObject skillBubblePrefab;
    [JsonIgnore]
    public GameObject dialogueBoxPrefab;

    // [Header("Window Prefabs")]
    [JsonIgnore]
    public Canvas screenCanvas;
    [JsonIgnore]
    public Canvas worldCanvas;

    [JsonIgnore]
    public GraphicRaycaster raycaster;

    [JsonIgnore]
    public Scene baseScene;
    [NonSerialized]
    [JsonIgnore]
    public GameObject baseCamera;
    [JsonIgnore]
    public Scene levelScene;

    [NonSerialized]
    [JsonIgnore]
    public GameObject mainMenu;
    [NonSerialized]
    [JsonIgnore]
    public GameObject loadingArt;
    [NonSerialized]
    [JsonIgnore]
    public GameStateMachine gameStateMachine;
    [NonSerialized]
    [JsonIgnore]
    public GameObject HUD;

    [JsonIgnore]
    private readonly string savePath = "./Saves";

    [JsonProperty]
    private string _targetSceneName;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    private void Awake()
    {
        // Singleton recipe so only one instance is active at a time.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        inputController = GetComponent<InputController>();
        lightController = GetComponent<LightController>();
        audioController = GetComponent<AudioController>();
        windowManager = GetComponent<WindowManager>();

        baseCamera = Camera.main.gameObject;
        raycaster = screenCanvas.GetComponent<GraphicRaycaster>();
        mainMenu = screenCanvas.GetComponentInChildren<MainMenu>(true).gameObject;
        loadingArt = screenCanvas.GetComponentInChildren<LoadingScreen>(true).gameObject;
        HUD = screenCanvas.GetComponentInChildren<HUD>(true).gameObject;

        Database = new DatabaseManager(databaseName);
    }

    /// <summary>
    ///     Called after all Awakes.
    /// </summary>
    private void Start()
    {
        WindowOptions windowOptions = screenCanvas.GetComponentInChildren<WindowOptions>(true);
        windowOptions.gameObject.SetActive(true);

        gameStateMachine = gameObject.AddComponent<GameStateMachine>();
        gameStateMachine.SetState(new MenuState(gameStateMachine, "Base"));
    }

    public void ChangeScene(string targetSceneName, int targetPointID, bool loadGame = false)
    {
        PlayState state = new PlayState(gameStateMachine, targetSceneName, loadGame);
        _targetSceneName = targetSceneName;

        if (!loadGame)
        {
            state.exitAction += () =>
            {
                MapTransfer[] transfers = FindObjectsByType<MapTransfer>(FindObjectsSortMode.None);

                foreach (MapTransfer transfer in transfers)
                {
                    if (transfer.targetPointID == targetPointID && transfer.canReceive)
                    {
                        Vector2 pos = transfer.gameObject.transform.localPosition;
                        Player.Instance.navMeshAgent.Warp(pos);
                        break;
                    }
                }
            };
        }
        else
        {
            state.exitAction += () =>
            {
                using StreamReader sr = new StreamReader(savePath + "/player.json");
                string playerJson = sr.ReadToEnd();

                JsonConvert.PopulateObject(
                    playerJson,
                    Player.Instance,
                    new JsonSerializerSettings
                    {
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects
                    }
                );
            };
        }

        gameStateMachine.SetState(state);
    }

    public void StartGame()
    {
        ChangeScene("Homestead", 0);
    }

    public void SaveGame()
    {
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        string playerJson = JsonConvert.SerializeObject(Player.Instance, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        });

        Debug.Log(playerJson);

        using (StreamWriter sw = new StreamWriter(savePath + "/player.json"))
        {
            sw.Write(playerJson);
        }

        string gameManagerJson = JsonConvert.SerializeObject(this);

        Debug.Log(gameManagerJson);

        using (StreamWriter sw = new StreamWriter(savePath + "/gameManager.json"))
        {
            sw.Write(gameManagerJson);
        }

        Debug.Log("Saved successfully!");
    }

    public void LoadGame()
    {
        if (!Directory.Exists(savePath))
        {
            throw new ArgumentNullException("Directory does not exist!");
        }

        using StreamReader sr = new StreamReader(savePath + "/gameManager.json");
        string gameManagerJson = sr.ReadToEnd();
        JObject jObj = JObject.Parse(gameManagerJson);
        string targetSceneName = (string)jObj["_targetSceneName"];

        if (targetSceneName == "")
        {
            throw new ArgumentNullException("Target scene does not exist!");
        }

        ChangeScene(targetSceneName, 0, true);
    }

    public DataTable QueryDatabase(string query, params (string Key, object Value)[] args)
    {
        return Database.Read(query, args);
    }

    public void ParseDatabaseRow(DataRow row, object model,
        params (string Key, string FieldName)[] customMap)
    {
        Database.ParseRow(row, model, customMap);
    }

    /// <summary>
    ///     Loads the asset through the addressable system.
    /// </summary>
    /// <typeparam name="T">Type of asset</typeparam>
    /// <param name="key">Addressable key of asset</param>
    /// <returns>Addressable asset to manipulate</returns>
    public T LoadAsset<T>(string key)
    {
        T t = Addressables.LoadAssetAsync<T>(key).WaitForCompletion();
        return t;
    }

    public void CreatePlayer()
    {
        Instantiate(playerPrefab);
    }

    public void ExecuteCoroutine(IEnumerator fn)
    {
        StartCoroutine(fn);
    }

    public void KillCoroutine(IEnumerator fn)
    {
        StopCoroutine(fn);
    }
}