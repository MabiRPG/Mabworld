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
    public int ID;

    // Dictionary of basic skill info and specific rank stats.
    public Dictionary<string, object> info = new Dictionary<string, object>();
    public Dictionary<string, float[]> stats = new Dictionary<string, float[]>();

    // Current index and rank of skill.
    public int index = 0;
    public EventManager indexEvent = new EventManager();
    public string rank;
    // Icon sprite
    public Sprite sprite;

    // Current xp and maximum rank xp.
    public float xp;
    public float xpMax;
    public EventManager xpEvent = new EventManager();
    public EventManager xpMaxEvent = new EventManager();

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
        rank = ranks[index];

        CreateTrainingMethods();

        indexEvent.OnChange += AudioManager.Instance.PlayLevelUpSFX;
    }

    /// <summary>
    ///     Loads the skill info from the database.
    /// </summary>
    /// <param name="ID">Skill ID in database.</param>
    public void LoadSkillInfo(int ID) 
    {   
        // Gets the basic skill info
        DataTable dt = GameManager.Instance.QueryDatabase(skillQuery, ("@id", ID));   

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
        dt = GameManager.Instance.QueryDatabase(statsQuery, ("@id", ID));
        
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

    /// <summary>
    ///     Checks if has available ranks to rank up.
    /// </summary>
    /// <returns>True if can rank up.</returns>
    public bool CanRankUp()
    {
        return index + 1 < ranks.Length;
    }

    /// <summary>
    ///     Ranks up the skill.
    /// </summary>
    public void RankUp() 
    {
        index++;
        rank = ranks[index];
        xp = 0;
        CreateTrainingMethods();
        indexEvent.RaiseOnChange();
        xpEvent.RaiseOnChange();

        if (!CanRankUp())
        {
            indexEvent.Clear();
        }
    }

    /// <summary>
    ///     Ranks down the skill.
    /// </summary>
    public void RankDown()
    {
        if (index - 1 >= 0)
        {
            index--;
            rank = ranks[index];
        }
    }

    /// <summary>
    ///     Adds xp to the skill.
    /// </summary>
    /// <param name="x">Amount of xp to add.</param>
    public void AddXP(float x)
    {
        xp += x;
        xpEvent.RaiseOnChange();

        if (!CanRankUp() && xp == xpMax)
        {
            xpEvent.Clear();
        }
    }

    /// <summary>
    ///     Implements the rank-specific training methods.
    /// </summary>
    public void CreateTrainingMethods()
    {
        // Creates a new data table and queries the db.
        DataTable dt = GameManager.Instance.QueryDatabase(methodsQuery, ("@id", info["skill_id"]), ("@rank", rank));
        // Clears the previous training methods.
        foreach (SkillTrainingMethod method in methods)
        {
            method.Clear();
        }

        methods.Clear();
        // Resets the max xp gainable.
        xpMax = 0;

        // For every method, create a new method and insert into list.
        foreach (DataRow row in dt.Rows)
        {
            int methodID = int.Parse(row["method_id"].ToString());
            string methodName = row["method"].ToString();
            float xpGainEach = float.Parse(row["xp_gain_each"].ToString());
            float countMax = float.Parse(row["count_max"].ToString());

            SkillTrainingMethod method = new SkillTrainingMethod(this, methodID, methodName,
                xpGainEach, countMax);
            // Adds the max xp from method to skill.
            xpMax += xpGainEach * countMax;

            methods.Add(method);
        }

        xpMaxEvent.RaiseOnChange();

        if (!CanRankUp())
        {
            xpMaxEvent.Clear();
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
            useTime += stats["use_time"][index];
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
        float chance = GameManager.Instance.lifeSkillBaseSuccessRate + Player.Instance.LifeSkillSuccessRate();

        // Add skill specific modifiers
        if (stats.ContainsKey("success_rate_increase"))
        {
            chance += stats["success_rate_increase"][index];
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
            result.resourceGain = Player.Instance.LuckyGainMultiplier();
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
