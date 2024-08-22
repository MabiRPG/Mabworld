using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using System;
using System.Reflection;
using System.Text;
using System.Globalization;
using UnityEngine.UI;

/// <summary>
///     This class handles all game-wide processing. Refer to Game.instance for the 
///     specific instance.
/// </summary>
public class GameManager : MonoBehaviour 
{
    // Global instance of GameManager
    public static GameManager Instance {get; private set;}

    private InputController inputController;
    private LightController lightController;
    private AudioController audioController;
    public WindowManager windowManager;

    [Header("Global Variables")]
    // Name of the game database in Assets/Database folder.
    [SerializeField]
    private string databaseName;
    // Base success rate of life skills
    public float lifeSkillBaseSuccessRate;

    [Header("Universal Prefabs")]
    [SerializeField]
    public GameObject skillBubblePrefab;
    
    [Header("Window Prefabs")]
    public Canvas canvas;
    // [SerializeField]
    // private GameObject windowSkillPrefab;
    // [SerializeField]
    // private GameObject windowCharacterPrefab;
    // [SerializeField]
    // private GameObject windowInventoryPrefab;
    // [SerializeField]
    // private GameObject windowCraftingPrefab;

    // Cache of database results.
    private Dictionary<string, DataTable> cache = new Dictionary<string, DataTable>();
    // Loot system
    public LootGenerator lootGenerator = new LootGenerator();

    public GraphicRaycaster raycaster;

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
        raycaster = canvas.GetComponent<GraphicRaycaster>();
        mainMenu = canvas.GetComponentInChildren<MainMenu>(true).gameObject;
        loadingArt = canvas.GetComponentInChildren<LoadingScreen>(true).gameObject;
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

    /// <summary>
    ///     Queries the database.
    /// </summary>
    /// <param name="query">SQL query string to be executed.</param>
    /// <param name="args">Variable argument list for SQL parameters.</param>
    /// <returns>DataTable result of SQL query.</returns>
    public DataTable QueryDatabase(string query, params (string Key, object Value)[] args)
    {
        // Construct a cache key from parameters and sql string.
        string cacheKey = query;

        foreach (var (Key, Value) in args)
        {
            cacheKey += string.Format("({0},{1})", Key, Value);
        }

        // Find in cache if available.
        if (cache.ContainsKey(cacheKey))
        {
            return cache[cacheKey];
        }

        // Creates the database uri location
        string dbUri = "URI=file:" + Application.streamingAssetsPath + "/mabinogi.db";

        // Opens a connection
        IDbConnection dbConnection = new SqliteConnection(dbUri);

        // Creates a sql query command
        IDbCommand dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText = query;

        // Adds a parameterized field for each in args.
        foreach (var (Key, Value) in args)
        {
            var parameter = dbCommand.CreateParameter();
            parameter.ParameterName = Key;
            parameter.Value = Value;
            dbCommand.Parameters.Add(parameter);
        }

        dbConnection.Open();

        // Executes the command, loads the table, and closes the reader.
        IDataReader reader = dbCommand.ExecuteReader();

        // Creates a new datatable.
        DataTable dt = new DataTable();
        dt.Load(reader);

        reader.Close();
        dbConnection.Close();

        // Saves it to cache.
        cache[cacheKey] = dt;

        return dt;
    }

    /// <summary>
    ///     Parses the database row and transforms it into a C# class model
    ///     with custom parameter mapping.
    /// </summary>
    /// <param name="row">DataRow result from querying the database.</param>
    /// <param name="model"></param>
    /// <param name="customMap"></param>
    public void ParseDatabaseRow(DataRow row, object model, 
        params (string Key, string FieldName)[] customMap)
    {
        foreach(DataColumn column in row.Table.Columns)
        {
            FieldInfo field = null;
            
            // Iterate over custom map to see if name exists, if so, use custom fieldname.
            foreach(var (Key, FieldName) in customMap)
            {
                if (Key == column.ColumnName)
                {
                    field = model.GetType().GetField(FieldName,
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    break;
                }
            }

            if (field == null)
            {
                // Gets the field name from the database column
                string name = ConvertSnakeCaseToCamelCase(column.ColumnName);
                // Finds the field in the class model using the default naming convention, 
                // if it exists
                field = model.GetType().GetField(name, 
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                // Continues if nothing can be found.
                if (field == null)
                {
                    continue;
                }
            }

            // Gets the value in the table, converts it to a string
            string s = row[column].ToString();
            object value;

            // If field is NULL in database, ignore and move on.
            if (s.Length == 0)
            {
                continue;
            }

            // Checks the type of the class field, and converts the database value
            // to the appropriate type
            switch(field.FieldType)
            {
                case Type t when t == typeof(int):
                    value = int.Parse(s);
                    break;
                case Type t when t == typeof(float):
                    value = float.Parse(s);
                    break;
                case Type t when t == typeof(char):
                    value = char.Parse(s);
                    break;
                case Type t when t == typeof(bool):
                    // Bools are stored as "0" or "1" in database, since there is no
                    // inherent bool type
                    if (s == "1")
                    {
                        value = true;
                    }
                    else
                    {
                        value = false; 
                    }

                    break;
                case Type t when t == typeof(Sprite):
                    value = LoadAsset<Sprite>(s);
                    break;
                case Type t when t == typeof(AudioClip):
                    value = LoadAsset<AudioClip>(s);
                    break;
                case Type t when t == typeof(IntManager):
                    value = new IntManager(int.Parse(s));
                    break;
                case Type t when t == typeof(FloatManager):
                    value = new FloatManager(float.Parse(s));
                    break;
                case Type t when t == typeof(StringManager):
                    value = new StringManager(s);
                    break;
                case Type t when t == typeof(BoolManager):
                    if (s == "1")
                    {
                        value = new BoolManager(true);
                    }
                    else
                    {
                        value = new BoolManager(false);
                    }

                    break;
                default:
                    value = s;
                    break;
            }

            // Sets the class field to the value.
            field.SetValue(model, value);
        }
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

    /// <summary>
    ///     Converts a snake case string to camel case.
    /// </summary>
    /// <param name="snakeCase">Snake case string</param>
    /// <returns>Camel case string equivalent</returns>
    public string ConvertSnakeCaseToCamelCase(string snakeCase)
    {
        if (string.IsNullOrEmpty(snakeCase))
        {
            return snakeCase;
        }

        // Split the snake_case string into words
        string[] words = snakeCase.Split('_');
        if (words.Length == 0)
        {
            return snakeCase;
        }

        // Initialize the StringBuilder with the first word (in lowercase)
        StringBuilder camelCase = new StringBuilder(words[0].ToLower(CultureInfo.InvariantCulture));

        // Iterate over the remaining words
        for (int i = 1; i < words.Length; i++)
        {
            string word = words[i];
            if (!string.IsNullOrEmpty(word))
            {
                // Capitalize the first letter and append the rest in lowercase
                camelCase.Append(char.ToUpper(word[0], CultureInfo.InvariantCulture));
                camelCase.Append(word.Substring(1).ToLower(CultureInfo.InvariantCulture));
            }
        }

        return camelCase.ToString();
    }
}