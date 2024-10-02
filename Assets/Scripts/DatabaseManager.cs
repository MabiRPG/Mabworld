using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Data.Sqlite;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SkillModel
{
    // Primary key of skill
    public int ID;
    // Name of skill and category
    public string name;
    public int categoryID;
    // Skill description, details, skill icon, and sound effect when using
    public string description;
    public string details;
    public Sprite icon;
    public AudioClip sfx;
    public AnimationClip animationClip;
    // Starting, first and last ranks that can be reached
    public string startingRank;
    public string firstAvailableRank;
    public string lastAvailableRank;
    // Base loading time, use time, and cooldown
    public float baseLoadTime;
    public float baseUseTime;
    public float baseCooldown;
    // Does player start with skill?
    public bool isStartingWith;
    // Learnable? and learn condition
    public bool isLearnable;
    public int learnConditionID;
    // Passive or active
    public bool isPassive;

    // All ranks in string format
    public static List<string> ranks = new List<string>
        {"F", "E", "D", "C", "B", "A", "9", "8", "7", "6", "5", "4", "3", "2", "1"};

    public List<SkillStatModel> stats = new List<SkillStatModel>();
    public List<TrainingMethodModel> trainingMethods = new List<TrainingMethodModel>();

    public SkillModel(DatabaseManager database, DataRow row)
    {
        database.ParseRow(row, this, ("id", "ID"), ("category_id", "categoryID"));

        string statsQuery = @"SELECT skill_stat.*, skill_stat_type.name
                FROM skill
                JOIN skill_stat
                ON skill.id = skill_stat.skill_id
                JOIN skill_stat_type
                ON skill_stat.skill_stat_id = skill_stat_type.id
                WHERE skill.id = @id;";

        DataTable dt = database.Query(statsQuery, ("@id", ID));

        foreach (DataRow r in dt.Rows)
        {
            SkillStatModel stat = new SkillStatModel(database, r);
            stats.Add(stat);
        }

        string trainingQuery = @"SELECT *
            FROM training_method
            WHERE skill_id = @id;";

        dt = database.Query(trainingQuery, ("@id", ID));

        foreach (DataRow r in dt.Rows)
        {
            TrainingMethodModel trainingMethod = new TrainingMethodModel(database, r);
            trainingMethods.Add(trainingMethod);
        }
    }
}

public class SkillTypeModel
{
    public int ID;
    public string name;

    public SkillTypeModel(DatabaseManager database, DataRow row)
    {
        database.ParseRow(row, this, ("id", "ID"));
    }
}

public class SkillStatModel
{
    public int skillID;
    public int statID;
    public List<float> values = new List<float>(SkillModel.ranks.Count);

    public SkillStatModel()
    {
        for (int i = 0; i < values.Capacity; i++)
        {
            values.Add(0);
        }
    }

    public SkillStatModel(DatabaseManager database, DataRow row)
    {
        // Stat position field is the last column.
        int statPos = row.ItemArray.Length - 1;
        // Set the key to be the stat name, then slice the row by length of ranks
        // converting to string then float and back to array for the value.
        values.AddRange(
            row.ItemArray.Skip(2).Take(SkillModel.ranks.Count).Select(x => float.Parse(x.ToString())).ToArray());

        database.ParseRow(row, this, ("skill_id", "skillID"), ("skill_stat_id", "statID"));
    }
}

public class SkillStatTypeModel
{
    public int ID;
    public string name;

    public SkillStatTypeModel(DatabaseManager database, DataRow row)
    {
        database.ParseRow(row, this, ("id", "ID"));
    }
}

public class TrainingMethodModel
{
    public int skillID;
    public string rank;
    public int trainingMethodID;
    public float xpGainEach;
    public int countMax;

    public TrainingMethodModel()
    {
    }

    public TrainingMethodModel(DatabaseManager database, DataRow row)
    {
        database.ParseRow(row, this, ("skill_id", "skillID"), ("training_method_id", "trainingMethodID"));
    }
}

public class TrainingMethodTypeModel
{
    public int ID;
    public string name;

    public TrainingMethodTypeModel(DatabaseManager database, DataRow row)
    {
        database.ParseRow(row, this, ("id", "ID"));
    }
}


public class DatabaseManager
{
    private string databaseName;
   // Cache of database results.
    private Dictionary<string, DataTable> cache = new Dictionary<string, DataTable>();

    public DatabaseManager(string databaseName)
    {
        this.databaseName = databaseName;
    }

    /// <summary>
    ///     Queries the database.
    /// </summary>
    /// <param name="query">SQL query string to be executed.</param>
    /// <param name="args">Variable argument list for SQL parameters.</param>
    /// <returns>DataTable result of SQL query.</returns>
    public DataTable Query(string query, params (string Key, object Value)[] args)
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