using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Data;
using UnityEngine.AddressableAssets;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using System;

/// <summary>
///     This class handles all game-wide processing. Refer to Game.instance for the 
///     specific instance.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Global instance of GameManager
    public static GameManager Instance { get; private set; }

    [NonSerialized]
    public InputController inputController;
    [NonSerialized]
    public LightController lightController;
    [NonSerialized]
    public AudioController audioController;
    [NonSerialized]
    public WindowManager windowManager;
    public GameObject overlay;

    [Header("Global Variables")]
    // Name of the game database in Assets/Database folder.
    [SerializeField]
    private string databaseName;
    public DatabaseManager Database;
    // Base success rate of life skills
    public float lifeSkillBaseSuccessRate;

    [Header("Universal Prefabs")]
    public GameObject skillBubblePrefab;
    public GameObject dialogueBoxPrefab;

    // [Header("Window Prefabs")]
    public Canvas screenCanvas;
    public Canvas worldCanvas;

    // Loot system
    // public LootGenerator lootGenerator = new LootGenerator();

    public GraphicRaycaster raycaster;

    public Scene baseScene;
    [NonSerialized]
    public GameObject baseCamera;
    public Scene levelScene;

    [NonSerialized]
    public Minimap minimap;
    [NonSerialized]
    public GameObject mainMenu;
    [NonSerialized]
    public GameObject loadingArt;
    [NonSerialized]
    public GameStateMachine gameStateMachine;

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
        minimap = screenCanvas.GetComponentInChildren<Minimap>(true);
        mainMenu = screenCanvas.GetComponentInChildren<MainMenu>(true).gameObject;
        loadingArt = screenCanvas.GetComponentInChildren<LoadingScreen>(true).gameObject;

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

    public void Save()
    {
        string json = JsonConvert.SerializeObject(Player.Instance, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        });
        Debug.Log(json);
        using StreamWriter sw = new StreamWriter("./Saves/saveTest.json");
        sw.Write(json);
    }

    public void Load()
    {
        using StreamReader sr = new StreamReader("./Saves/saveTest.json");
        string json = sr.ReadToEnd();
        JsonConvert.PopulateObject(json, Player.Instance, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        });
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

    public void ExecuteCoroutine(IEnumerator fn)
    {
        StartCoroutine(fn);
    }

    public void KillCoroutine(IEnumerator fn)
    {
        StopCoroutine(fn);
    }
}