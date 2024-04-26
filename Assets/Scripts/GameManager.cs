using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;

/// <summary>
///     This class handles all game-wide processing. Refer to Game.instance for the 
///     specific instance.
/// </summary>
public class GameManager : MonoBehaviour 
{
    // Global instance of GameManager
    public static GameManager instance = null;

    // Name of the game database in Assets/Database folder.
    public string databaseName;

    public Canvas canvas;
    public GameObject windowSkillPrefab;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    private void Awake()
    {
        // Singleton recipe so only one instance is active at a time.
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        Instantiate(windowSkillPrefab, canvas.transform);
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
        dbConnection.Open();

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

        // Executes the command, loads the table, and closes the reader.
        IDataReader reader = dbCommand.ExecuteReader();

        // Creates a new datatable.
        DataTable dt = new DataTable();
        dt.Load(reader);
        reader.Close();

        return dt;
    }
}