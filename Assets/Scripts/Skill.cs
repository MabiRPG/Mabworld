using System.Data;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.AddressableAssets;

/// <summary>
///     Handles all skill processing.
/// </summary>
public class Skill 
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
    public char startingRank;
    public char firstAvailableRank;
    public char lastAvailableRank;
    // Base loading time, use time, and cooldown
    public float baseLoadTime;
    public float baseUseTime;
    public float baseCooldown;
    public float baseSuccessRate;
    // Does player start with skill?
    public bool isStartingWith;
    // Learnable? and learn condition
    public bool isLearnable;
    public int learnConditionID;
    // Passive or active
    public bool isPassive;

    // Dictionary of basic skill info and specific rank stats.
    public Dictionary<string, object> info = new Dictionary<string, object>();
    public Dictionary<string, float[]> stats = new Dictionary<string, float[]>();

    // Current index and rank of skill.
    public ValueManager index = new ValueManager();
    // Icon sprite
    public Sprite sprite;

    // Current xp and maximum rank xp.
    public ValueManager xp = new ValueManager();
    public ValueManager xpMax = new ValueManager();

    // List of training methods at current rank
    public List<SkillTrainingMethod> methods = new List<SkillTrainingMethod>();

    // All ranks in string format
    public string[] ranks = {"F", "E", "D", "C", "B", "A", "9", "8", "7", "6", "5", "4", "3", "2", "1"};

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

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    /// <param name="ID">Skill ID in database.</param>
    public Skill(int ID)
    {
        this.ID = ID;
        LoadSkillInfo(this.ID);

        sprite = Addressables.LoadAssetAsync<Sprite>(info["icon_name"].ToString()).WaitForCompletion();

        CreateTrainingMethods();

        index.OnChange += AudioManager.Instance.PlayLevelUpSFX;
        index.OnChange += CreateTrainingMethods;
    }

    /// <summary>
    ///     Loads the skill info from the database.
    /// </summary>
    /// <param name="ID">Skill ID in database.</param>
    public void LoadSkillInfo(int ID) 
    {   
        // Gets the basic skill info
        DataTable dt = GameManager.Instance.QueryDatabase(skillQuery, ("@id", ID));   
        DataRow row = dt.Rows[0];

        // Inserts into dictionary.
        foreach (DataColumn column in dt.Columns)
        {
            info.Add(column.ColumnName, row[column]);
        }

        // Gets the detailed skill info at every rank.
        dt = GameManager.Instance.QueryDatabase(statsQuery, ("@id", ID));
        row = dt.Rows[0];
        
        // Inserts into dictionary.
        // Stat position field is the last column.
        int statPos = row.ItemArray.Length - 1;
        // Set the key to be the stat name, then slice the row by length of ranks
        // converting to string then float and back to array for the value.
        stats.Add(row.ItemArray[statPos].ToString(), 
            row.ItemArray.Skip(2).Take(ranks.Length).Select(x => float.Parse(x.ToString())).ToArray());
    }

    /// <summary>
    ///     Checks if has available ranks to rank up.
    /// </summary>
    /// <returns>True if can rank up.</returns>
    public bool CanRankUp()
    {
        return index.ValueInt + 1 < ranks.Length;
    }

    /// <summary>
    ///     Ranks up the skill.
    /// </summary>
    public void RankUp() 
    {
        index.Value++;
        xp.Value = 0;
        
        if (!CanRankUp())
        {
            index.Clear();
        }
    }

    /// <summary>
    ///     Ranks down the skill.
    /// </summary>
    public void RankDown()
    {
        if (index.ValueInt - 1 >= 0)
        {
            index.Value--;
        }
    }

    /// <summary>
    ///     Adds xp to the skill.
    /// </summary>
    /// <param name="x">Amount of xp to add.</param>
    public void AddXP(float x)
    {
        xp.Value += x;

        if (!CanRankUp() && xp.Value == xpMax.Value)
        {
            xp.Clear();
        }
    }

    /// <summary>
    ///     Implements the rank-specific training methods.
    /// </summary>
    public void CreateTrainingMethods()
    {
        // Creates a new data table and queries the db.
        DataTable dt = GameManager.Instance.QueryDatabase(methodsQuery, ("@id", info["skill_id"]), ("@rank", ranks[index.ValueInt]));
        // Clears the previous training methods.
        foreach (SkillTrainingMethod method in methods)
        {
            method.Clear();
        }

        methods.Clear();
        // Resets the max xp gainable.
        xpMax.Value = 0;

        // For every method, create a new method and insert into list.
        foreach (DataRow row in dt.Rows)
        {
            int methodID = int.Parse(row["method_id"].ToString());
            string methodName = row["method"].ToString();
            float xpGainEach = float.Parse(row["xp_gain_each"].ToString());
            int countMax = int.Parse(row["count_max"].ToString());

            SkillTrainingMethod method = new SkillTrainingMethod(this, methodID, methodName,
                xpGainEach, countMax);
            // Adds the max xp from method to skill.
            xpMax.Value += xpGainEach * countMax;

            methods.Add(method);
        }

        if (!CanRankUp())
        {
            xpMax.Clear();
        }
    }

    public IEnumerator Use()
    {
        // Makes the player busy.
        Player.Instance.isBusy = true;
        // Calculates the base use time for the skill.
        float useTime = float.Parse(info["base_use_time"].ToString());

        // Adds skill specific time modifiers
        if (stats.ContainsKey("use_time"))
        {
            useTime += stats["use_time"][index.ValueInt];
        }

        useTime = Math.Max(0, useTime);

        // Debug purposes...
        useTime = 0.1f;

        float currTime = 0;
        // Interval for which audio should play
        float audioInterval = 1f;
        AudioSource audio = Player.Instance.gameObject.GetComponent<AudioSource>();

        while (useTime - currTime > Time.deltaTime)
        {
            // Find the remaining interval time and yield
            float interval = Math.Min(useTime - currTime, audioInterval);
            yield return new WaitForSeconds(interval);

            // Play a sound if audio interval is reached
            if (interval % audioInterval == 0)
            {
                audio.clip = Addressables.LoadAssetAsync<AudioClip>("pickaxe").WaitForCompletion();
                audio.Play();
            }

            currTime += interval;
        }

        // Calculate base success rate of skill
        float chance = GameManager.Instance.lifeSkillBaseSuccessRate + Player.Instance.CalculateLifeSkillSuccessRate();

        // Add skill specific modifiers
        if (stats.ContainsKey("success_rate_increase"))
        {
            chance += stats["success_rate_increase"][index.ValueInt];
        }

        // Change to percentage and roll die
        chance /= 100;
        float roll = (float)GameManager.Instance.rnd.NextDouble();

        // Handle success or fail here
        Result result = Player.Instance.result;

        result.Clear();
        result.skill = this;
        result.type = Result.Type.Gather;

        if (chance >= roll)
        {
            result.isSuccess = true;
            result.resourceGain = Player.Instance.CalculateLuckyGainMultiplier();
        }
        else
        {
            result.isSuccess = false;
        }       

        result.statusEvent.RaiseOnChange();

        // Makes the player available again.
        Player.Instance.isBusy = false;
    }
}
