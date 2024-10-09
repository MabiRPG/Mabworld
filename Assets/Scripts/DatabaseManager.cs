using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;
using Mono.Data.Sqlite;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DatabaseManager
{
    private string databaseName;
   // Cache of database results.
    private Dictionary<string, DataTable> cache = new Dictionary<string, DataTable>();

    public DatabaseManager(string databaseName)
    {
        this.databaseName = databaseName;
    }

    public DataTable ReadTable(string query, Dictionary<string, ModelFieldReference> fieldMap)
    {
        // Construct a cache key from parameters and sql string.
        string cacheKey = query;

        foreach ((string columnName, ModelFieldReference field) in fieldMap)
        {
            cacheKey += string.Format("({0},{1})", "@" + columnName, field.Get());
        }

        // Find in cache if available.
        if (cache.ContainsKey(cacheKey))
        {
            return cache[cacheKey];
        }

        // Creates the database uri location
        string dbUri = "URI=file:" + Application.streamingAssetsPath + "/" + databaseName;

        // Opens a connection
        IDbConnection dbConnection = new SqliteConnection(dbUri);

        // Creates a sql query command
        IDbCommand dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText = query;

        // Adds a parameterized field for each in args.
        foreach ((string columnName, ModelFieldReference field) in fieldMap)
        {
            var parameter = dbCommand.CreateParameter();
            parameter.ParameterName = "@" + columnName;
            parameter.Value = field.Get();
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
    ///     Queries the database.
    /// </summary>
    /// <param name="query">SQL query string to be executed.</param>
    /// <param name="args">Variable argument list for SQL parameters.</param>
    /// <returns>DataTable result of SQL query.</returns>
    public DataTable Read(string query, params (string Key, object Value)[] args)
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
        string dbUri = "URI=file:" + Application.streamingAssetsPath + "/" + databaseName;

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

    public int Write(string query, Dictionary<string, ModelFieldReference> fieldMap)
    {
        // Creates the database uri location
        string dbUri = "URI=file:" + Application.streamingAssetsPath + "/" + databaseName;

        // Opens a connection
        IDbConnection dbConnection = new SqliteConnection(dbUri);

        // Creates a sql query command
        IDbCommand dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText = query;

        // Adds a parameterized field for each in args.
        foreach ((string columnName, ModelFieldReference field) in fieldMap)
        {
            var parameter = dbCommand.CreateParameter();
            parameter.ParameterName = "@" + columnName;
            dynamic value = field.Get();

            switch (field.Type())
            {
                case Type t when t == typeof(bool):
                    value = value ? 1 : 0;
                    break;
                case Type t when t == typeof(Sprite):
                    value = AddToAddressables(value);
                    break;
                case Type t when t == typeof(AudioClip):
                    value = AddToAddressables(value);
                    break;
                default:
                    break;
            }

            parameter.Value = value;
            dbCommand.Parameters.Add(parameter);
        }

        dbConnection.Open();
        return dbCommand.ExecuteNonQuery();
    }

    public void ParseRow(DataRow row, Dictionary<string, ModelFieldReference> fieldMap)
    {
        foreach(DataColumn column in row.Table.Columns)
        {
            if (!fieldMap.ContainsKey(column.ColumnName))
            {
                continue;
            }

            var field = fieldMap[column.ColumnName];
            
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
            switch(field.Type())
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
            field.Set(value);
        }        
    }

    /// <summary>
    ///     Parses the database row and transforms it into a C# class model
    ///     with custom parameter mapping.
    /// </summary>
    /// <param name="row">DataRow result from querying the database.</param>
    /// <param name="model"></param>
    /// <param name="customMap"></param>
    public void ParseRow(DataRow row, object model, 
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
    private string ConvertSnakeCaseToCamelCase(string snakeCase)
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

    private string AddToAddressables(UnityEngine.Object asset)
    {
        if (asset == default)
        {
            return "";
        }

        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        string assetPath = AssetDatabase.GetAssetPath(asset);
        string assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
        settings.CreateAssetReference(assetGUID);

        return settings.FindAssetEntry(assetGUID).address;
    }
}