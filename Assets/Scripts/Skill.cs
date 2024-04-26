using System.Data;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Events;

//==============================================================================
// ** Skill
//------------------------------------------------------------------------------
//  This class handles all skill processing. Refer to Player.instance.skills for the
//  class instances.
//==============================================================================

public class Skill 
{
    // Dictionary of basic skill info and specific rank stats.
    public Dictionary<string, object> info = new Dictionary<string, object>();
    public Dictionary<string, float[]> stats = new Dictionary<string, float[]>();

    // Current index and rank of skill.
    public int index = 0;
    public string rank;

    // Current xp and maximum rank xp.
    public float xp;
    public float xpMax;

    // List of training methods at current rank
    public List<TrainingMethod> methods = new List<TrainingMethod>();
    // Unity Event for advancement
    public UnityEvent rankUpEvent = new UnityEvent();

    // All ranks in string format
    private string[] ranks = {"F", "E", "D", "C", "B", "A", "9", "8", "7", "6", "5", "4", "3", "2", "1"};

    // Query strings to database.
    private const string skillQuery = @"SELECT * FROM skills WHERE skill_id = @id LIMIT 1;";
    private const string statsQuery = @"SELECT skill_stats.*, skill_stats_type.stat
        FROM skills
        JOIN skill_stats
        ON skills.skill_id = skill_stats.skill_id
        JOIN skill_stats_type
        ON skill_stats.stat_id = skill_stats_type.stat_id
        WHERE skills.skill_id = @id;";
    private const string methodsQuery = @"SELECT training_methods_type.method, 
            training_methods.method_id, training_methods.xp_gain_each, training_methods.count_max 
        FROM training_methods
        JOIN training_methods_type
        ON training_methods.method_id = training_methods_type.method_id
        JOIN skills
        ON training_methods.skill_id = skills.skill_id
        WHERE training_methods.skill_id = @id AND training_methods.rank = @rank;";

    //--------------------------------------------------------------------------
    // * Object Initialization
    //       int id : database skill_id of the skill.
    //--------------------------------------------------------------------------
    public Skill(int id)
    {
        LoadSkillInfo(id);
        rank = ranks[index];
        CreateTrainingMethods();
    }

    //--------------------------------------------------------------------------
    // * Loads the skill info from the database.
    //       int id : database skill_id of the skill.
    //--------------------------------------------------------------------------
    public void LoadSkillInfo(int id) 
    {   
        // Creates an empty data table for our queries.
        DataTable dt = new DataTable();
        // Gets the basic skill info
        dt = GameManager.instance.QueryDatabase(skillQuery, ("@id", id));   

        // Iterate over all rows and columns, inserts into dictionary.
        foreach (DataRow row in dt.Rows)
        {
            foreach (DataColumn column in dt.Columns)
            {
                info.Add(column.ColumnName, row[column]);
            }
        }

        dt.Clear();

        // Gets the detailed skill info at every rank.
        dt = GameManager.instance.QueryDatabase(statsQuery, ("@id", id));
        
        // Iterate over all rows and columns, inserts into dictionary.
        foreach (DataRow row in dt.Rows)
        {       
            // Stat position field is the last column.
            int statPos = row.ItemArray.Length - 1;
            // Set the key to be the stat name, then slice the row by length of ranks
            // converting to string then float and back to array for the value.
            stats.Add(row.ItemArray[statPos].ToString(), 
                row.ItemArray.Skip(2).Take(ranks.Length).Select(x => float.Parse(x.ToString())).ToArray());
        }
    }

    public bool CanRankUp()
    {
        return index + 1 < ranks.Length;
    }

    //--------------------------------------------------------------------------
    // * Ranks up the skill
    //--------------------------------------------------------------------------
    public void RankUp() 
    {
        index++;
        rank = ranks[index];
        xp = 0;
        CreateTrainingMethods();
        rankUpEvent.Invoke();
    }

    //--------------------------------------------------------------------------
    // * Ranks down the skill
    //--------------------------------------------------------------------------
    public void RankDown()
    {
        if (index - 1 >= 0)
        {
            index--;
            rank = ranks[index];
        }
    }

    //--------------------------------------------------------------------------
    // * Adds xp to the skill
    //       float x : new amount to add
    //--------------------------------------------------------------------------
    public void AddXP(float x)
    {
        xp += x;
    }

    //--------------------------------------------------------------------------
    // * Implements the rank-specific training methods.
    //--------------------------------------------------------------------------
    public void CreateTrainingMethods()
    {
        // Creates a new data table and queries the db.
        DataTable dt = new DataTable();
        dt = GameManager.instance.QueryDatabase(methodsQuery, ("@id", info["skill_id"]), ("@rank", rank));
        // Clears the previous training methods.
        methods.Clear();
        // Resets the max xp gainable.
        xpMax = 0;

        // For every method, create a new method and insert into list.
        foreach (DataRow row in dt.Rows)
        {
            TrainingMethod method = new TrainingMethod(info["skill_id"], rank, row["method_id"],
                row["method"], row["xp_gain_each"], row["count_max"]);
            // Adds the max xp from method to skill.
            xpMax += float.Parse(row["xp_gain_each"].ToString()) * float.Parse(row["count_max"].ToString());

            methods.Add(method);
        }
    }
}

//==============================================================================
// ** TrainingMethod
//------------------------------------------------------------------------------
//  This class handles all training method processing. Refer to the main skill's
//  TrainingMethod List for instances.
//==============================================================================

public class TrainingMethod
{
    // Creates dictionary for the method information, and status update flag from player.
    public Dictionary<string, object> method = new Dictionary<string, object>();
    public Dictionary<string, object> status = new Dictionary<string, object>();

    //--------------------------------------------------------------------------
    // * Object Initialization
    //       int skill_id : database skill_id of the skill.
    //       object rank : string of current rank.
    //       object method_id : database method_id
    //       object methodName : name of the method
    //       object xp_gain_each : how much xp is gained every count of method
    //       object count_max : how many maximum counts of method are allowed
    //--------------------------------------------------------------------------
    public TrainingMethod(object skill_id, object rank, object method_id, object methodName, 
        object xp_gain_each, object count_max)
    {
        // Creates an empty counter for current method.
        method.Add("count", 0);
        // Inserts rest into dictionary
        method.Add("skill_id", skill_id);
        method.Add("rank", rank);
        method.Add("name", methodName);
        method.Add("method_id", method_id);
        method.Add("xp_gain_each", xp_gain_each);
        method.Add("count_max", count_max);
    }

    //--------------------------------------------------------------------------
    // * Checks if less than maximum counts, and checks training requirements.
    //--------------------------------------------------------------------------
    public void Update()
    {
        if ((int)method["count"] < (int)method["count_max"] && CheckTraining())
        {
            method["count"] = (int)method["count"] + 1;
        }
    }

    //--------------------------------------------------------------------------
    // * Checks training requirements against the status flag.
    //--------------------------------------------------------------------------
    public bool CheckTraining()
    {
        switch((int)method["method_id"])
        {
            case 1:
                return IsSuccess();
            case 2:
                return IsFail();
            case 3:
                return GatherTwoOrMore();
        }

        return false;
    }

    //--------------------------------------------------------------------------
    // * Checks if the action was a success
    //--------------------------------------------------------------------------
    public bool IsSuccess()
    {
        if ((bool)status["success"] == true)
        {
            return true;
        }
        
        return false;
    }

    //--------------------------------------------------------------------------
    // * Checks if the action was a failure
    //--------------------------------------------------------------------------
    public bool IsFail()
    {
        return !IsSuccess();
    }

    //--------------------------------------------------------------------------
    // * Checks if two or more resources were gathered at once.
    //--------------------------------------------------------------------------
    public bool GatherTwoOrMore()
    {
        if (IsSuccess() && (string)status["action"] == "gather" && (int)status["resourceGain"] > 1)
        {
            return true;
        }

        return false;
    }
}