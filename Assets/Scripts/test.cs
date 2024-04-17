using System.Data;
using UnityEngine;
using Mono.Data.Sqlite;

public class test
{
    public string databaseName;
    public string tableName;

    // Start is called before the first frame update
    void Start()
    {
        IDbConnection dbConnection = CreateAndOpenDatabase();
    }

    private IDbConnection CreateAndOpenDatabase() // 3
    {
        // Open a connection to the database.
        string dbUri = string.Format("URI=file:Assets/Database/{0}", databaseName); // 4
        IDbConnection dbConnection = new SqliteConnection(dbUri); // 5
        dbConnection.Open(); // 6

        // Create a table for the hit count in the database if it does not exist yet.
        IDbCommand dbCommand = dbConnection.CreateCommand(); // 6
        dbCommand.CommandText = "SELECT * FROM skills"; // 7
        IDataReader reader = dbCommand.ExecuteReader(); // 8

        while (reader.Read()) {
            Debug.Log(reader["method"]);
        }

        reader.Close();

        return dbConnection;
    }
}