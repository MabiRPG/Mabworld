using System.Data;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Linq;

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
    // Does player start with skill?
    public bool isStartingWith;
    // Learnable? and learn condition
    public bool isLearnable;
    public int learnConditionID;
    // Passive or active
    public bool isPassive;

    // Dictionary of basic skill info and specific rank stats.
    public Dictionary<string, float[]> stats = new Dictionary<string, float[]>();

    // Current index and rank of skill.
    public IntManager index = new IntManager();
    // Current xp and maximum rank xp.
    public FloatManager xp = new FloatManager();
    public FloatManager xpMax = new FloatManager();
    // Current cooldown timer
    public FloatManager cooldown = new FloatManager();

    // List of training methods at current rank
    public List<SkillTrainingMethod> methods = new List<SkillTrainingMethod>();

    // All ranks in string format
    public List<string> ranks = new List<string>
        {"F", "E", "D", "C", "B", "A", "9", "8", "7", "6", "5", "4", "3", "2", "1"};

    // Query strings to database.
    private const string skillQuery = @"SELECT * FROM skill WHERE id = @id LIMIT 1;";
    private const string statsQuery = @"SELECT skill_stat.*, skill_stat_type.name
        FROM skill
        JOIN skill_stat
        ON skill.id = skill_stat.skill_id
        JOIN skill_stat_type
        ON skill_stat.skill_stat_id = skill_stat_type.id
        WHERE skill.id = @id;";
    private const string methodsQuery = @"SELECT *
        FROM ""PROD Training Method Query""
        WHERE skill_id = @id AND rank = @rank";

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    /// <param name="ID">Skill ID in database.</param>
    public Skill(int ID)
    {
        this.ID = ID;
        LoadSkillInfo();
        CreateTrainingMethods();

        index.OnChange += AudioManager.Instance.PlayLevelUpSFX;
        index.OnChange += CreateTrainingMethods;
    }

    /// <summary>
    ///     Loads the skill info from the database.
    /// </summary>
    public void LoadSkillInfo() 
    {   
        // Gets the basic skill info
        DataTable dt = GameManager.Instance.QueryDatabase(skillQuery, ("@id", ID));   
        DataRow row = dt.Rows[0];
        GameManager.Instance.ParseDatabaseRow(row, this);

        // Gets the detailed skill info at every rank.
        dt = GameManager.Instance.QueryDatabase(statsQuery, ("@id", ID));
        
        foreach (DataRow r in dt.Rows)
        {
            // Stat position field is the last column.
            int statPos = r.ItemArray.Length - 1;
            // Set the key to be the stat name, then slice the row by length of ranks
            // converting to string then float and back to array for the value.
            stats.Add(r.ItemArray[statPos].ToString(), 
                r.ItemArray.Skip(2).Take(ranks.Count).Select(x => float.Parse(x.ToString())).ToArray());
        }
    }

    /// <summary>
    ///     Checks if has available ranks to rank up.
    /// </summary>
    /// <returns>True if can rank up.</returns>
    public bool CanRankUp()
    {
        return index.Value + 1 < ranks.Count;
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
        if (index.Value - 1 >= 0)
        {
            index.Value--;
        }
    }

    /// <summary>
    ///     Checks if the skill is at the given rank or greater.
    /// </summary>
    /// <param name="rank">String of the given rank</param>
    /// <returns></returns>
    public bool IsRankOrGreater(string rank)
    {
        if (ranks.Contains(rank))
        {
            return index.Value >= ranks.IndexOf(rank);
        }

        return false;
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
    ///     Gets the value of the stat at the current rank.
    /// </summary>
    /// <param name="key">String of stat name</param>
    /// <returns></returns>
    public float GetStat(string key)
    {
        if (stats.ContainsKey(key))
        {
            return stats[key][index.Value];
        }

        return 0;
    }

    /// <summary>
    ///     Gets the stat difference between the current rank and the previous.
    /// </summary>
    /// <param name="key">String of stat name</param>
    /// <returns>(Current - Previous) of the stat, 0 at the starting rank.</returns>
    public float GetStatBackwardDiff(string key)
    {
        if (stats.ContainsKey(key))
        {
            float curr = stats[key][index.Value];
            float prev = stats[key][Math.Max(0, index.Value - 1)];
            return curr - prev;
        }

        return 0;
    }

    /// <summary>
    ///     Gets the stat difference between the next rank and the current one.
    /// </summary>
    /// <param name="key">String of stat name</param>
    /// <returns>(Next - Current) of the stat, 0 at the final rank.</returns>
    public float GetStatForwardDiff(string key)
    {
        if (stats.ContainsKey(key))
        {
            float next = stats[key][Math.Min(ranks.Count - 1, index.Value + 1)];
            float curr = stats[key][index.Value];
            return next - curr;
        }

        return 0;
    }

    /// <summary>
    ///     Gets the current skill load time in seconds, including rank modifiers.
    /// </summary>
    /// <returns></returns>
    public float GetLoadTime()
    {
        return baseLoadTime + GetStat("load_time");
    }

    /// <summary>
    ///     Gets the current skill use time in seconds, including rank modifiers.
    /// </summary>
    /// <returns></returns>
    public float GetUseTime()
    {
        return baseUseTime + GetStat("use_time");
    }

    /// <summary>
    ///     Gets the current skill cooldown time in seconds, including rank modifiers.
    /// </summary>
    /// <returns></returns>
    public float GetCooldownTime()
    {
        return baseCooldown + GetStat("cooldown_time");
    }

    /// <summary>
    ///     Implements the rank-specific training methods.
    /// </summary>
    public void CreateTrainingMethods()
    {
        // Creates a new data table and queries the db.
        DataTable dt = GameManager.Instance.QueryDatabase(methodsQuery, ("@id", ID), ("@rank", ranks[index.Value]));
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
            SkillTrainingMethod method = new SkillTrainingMethod(this, row);
            // Adds the max xp from method to skill.
            xpMax.Value += method.xpGainEach * method.countMax;

            methods.Add(method);
        }

        if (!CanRankUp())
        {
            xpMax.Clear();
        }
    }

    /// <summary>
    ///     Uses the skill by playing the animation and sounds, and checking success.
    /// </summary>
    /// <typeparam name="T">Derived class of Type ResultHandler</typeparam>
    /// <param name="resultHandler">ResultHandler instance to manage the success or failure</param>
    /// <returns>Coroutine to be run.</returns>
    public IEnumerator Use<T>(T resultHandler) where T : ResultHandler
    {
        // Calculates the base use time for the skill.
        float useTime = GetUseTime();
        // 0.1 for debugging purposes...
        useTime = Math.Max(0.1f, useTime);

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
                audio.PlayOneShot(sfx);
            }

            currTime += interval;
        }

        // Calculate base success rate of skill
        float chance = GameManager.Instance.lifeSkillBaseSuccessRate;
        chance += Player.Instance.CalculateLifeSkillSuccessRate();
        // TODO : change to summation of previous ranks...
        chance += GetStat("success_rate_increase");

        // Change to percentage and roll die
        chance /= 100;
        float roll = UnityEngine.Random.Range(0f, 1f);

        // Handle success or fail here
        resultHandler.SetSuccess(chance >= roll);
    }

    public IEnumerator Cooldown(float time)
    {
        cooldown.Value = time;

        while (cooldown.Value > 0)
        {
            if (cooldown.Value > 10)
            {
                yield return new WaitForSecondsRealtime(1);
                cooldown.Value -= 1;
            }
            else
            {
                yield return new WaitForSecondsRealtime(0.1f);
                cooldown.Value -= 0.1f;
            }
        }
    }
}
