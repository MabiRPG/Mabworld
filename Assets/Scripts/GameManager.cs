using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.Collections.Generic;

/// <summary>
///     This class handles all game-wide processing. Refer to Game.instance for the 
///     specific instance.
/// </summary>
public class GameManager : MonoBehaviour 
{
    // Global instance of GameManager
    public static GameManager Instance {get; private set;}

    [Header("Managers")]
    [SerializeField]
    private GameObject inputManager;
    [SerializeField]
    private GameObject audioManager;

    [Header("Global Variables")]
    // Name of the game database in Assets/Database folder.
    [SerializeField]
    private string databaseName;
    // Base success rate of life skills
    public float lifeSkillBaseSuccessRate;
    
    [Header("Window Prefabs")]
    public Canvas canvas;
    public GameObject windowSkillPrefab;
    public GameObject windowCharacterPrefab;

    // Cache of database results.
    private Dictionary<string, DataTable> cache = new Dictionary<string, DataTable>();

    // RNG obj
    public System.Random rnd = new System.Random();

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

        DontDestroyOnLoad(gameObject);

        Instantiate(inputManager);
        Instantiate(audioManager);
        Instantiate(windowSkillPrefab, canvas.transform);
        Instantiate(windowCharacterPrefab, canvas.transform);
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
        string dbUri = string.Format("URI=file:Assets/Database/{0}", databaseName);

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
}