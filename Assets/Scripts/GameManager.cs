using UnityEngine;
using System;
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

    [SerializeField]
    private GameObject inputManager;
    [SerializeField]
    private GameObject audioManager;

    // Name of the game database in Assets/Database folder.
    [SerializeField]
    private string databaseName;
    // Base success rate of life skills
    public float lifeSkillBaseSuccessRate;
    
    public Canvas canvas;
    public GameObject windowSkillPrefab;
    public GameObject windowCharacterPrefab;

    // Cache of database results.
    private Dictionary<IDbCommand, DataTable> cache = new Dictionary<IDbCommand, DataTable>();

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
        //Instantiate(windowCharacterPrefab, canvas.transform);
    }

    /// <summary>
    ///     Queries the database.
    /// </summary>
    /// <param name="query">SQL query string to be executed.</param>
    /// <param name="args">Variable argument list for SQL parameters.</param>
    /// <returns>DataTable result of SQL query.</returns>
    public DataTable QueryDatabase(string query, params (string Key, object Value)[] args)
    {
        // Creates the database uri location
        string dbUri = string.Format("URI=file:Assets/Database/{0}", databaseName);

        // Opens a connection
        IDbConnection dbConnection = new SqliteConnection(dbUri);

        // Creates a sql query command
        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = query;

        // Adds a parameterized field for each in args.
        foreach (var pair in args)
        {
            var parameter = dbCommand.CreateParameter();
            parameter.ParameterName = pair.Key;
            parameter.Value = pair.Value;
            dbCommand.Parameters.Add(parameter);
        }

        // Find in cache if available.
        if (cache.ContainsKey(dbCommand))
        {
            return cache[dbCommand];
        }

        dbConnection.Open();

        // Executes the command, loads the table, and closes the reader.
        IDataReader reader = dbCommand.ExecuteReader();

        // Creates a new datatable.
        DataTable dt = new DataTable();
        dt.Load(reader);
        reader.Close();

        // Saves it to cache.
        cache[dbCommand] = dt;

        return dt;
    }
}