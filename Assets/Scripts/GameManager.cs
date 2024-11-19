using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Data;
using UnityEngine.AddressableAssets;
using System.Collections;

/// <summary>
///     This class handles all game-wide processing. Refer to Game.instance for the 
///     specific instance.
/// </summary>
public class GameManager : MonoBehaviour 
{
    // Global instance of GameManager
    public static GameManager Instance {get; private set;}

    public InputController inputController;
    public LightController lightController;
    public AudioController audioController;
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
    [SerializeField]
    public GameObject skillBubblePrefab;
    [SerializeField]
    public GameObject dialogueBoxPrefab;
    
    // [Header("Window Prefabs")]
    public Canvas canvas;
    
    // Loot system
    // public LootGenerator lootGenerator = new LootGenerator();

    public GraphicRaycaster raycaster;

    public Scene baseScene;
    public GameObject baseCamera;
    public Scene levelScene;

    public Minimap minimap;
    public GameObject mainMenu;
    public GameObject loadingArt;
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
        }

        //DontDestroyOnLoad(gameObject);

        inputController = GetComponent<InputController>();
        lightController = GetComponent<LightController>();
        audioController = GetComponent<AudioController>();
        windowManager = GetComponent<WindowManager>();

        baseCamera = Camera.main.gameObject;
        raycaster = canvas.GetComponent<GraphicRaycaster>();
        minimap = canvas.GetComponentInChildren<Minimap>(true);
        mainMenu = canvas.GetComponentInChildren<MainMenu>(true).gameObject;
        loadingArt = canvas.GetComponentInChildren<LoadingScreen>(true).gameObject;

        Database = new DatabaseManager(databaseName);
    }

    /// <summary>
    ///     Called after all Awakes.
    /// </summary>
    private void Start()
    {
        WindowOptions windowOptions = canvas.GetComponentInChildren<WindowOptions>(true);
        windowOptions.gameObject.SetActive(true);

        gameStateMachine = gameObject.AddComponent<GameStateMachine>();
        gameStateMachine.SetState(new MenuState(gameStateMachine, "Base"));
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