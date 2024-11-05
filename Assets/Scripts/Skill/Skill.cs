using System.Data;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Linq;

/// <summary>
///     Handles all skill processing.
/// </summary>
public class Skill : SkillModel
{
    // Current index and rank of skill.
    public IntManager index = new IntManager();
    // Current xp and maximum rank xp.
    public FloatManager xp = new FloatManager();
    public FloatManager xpMax = new FloatManager();
    // Current cooldown timer
    public FloatManager cooldown = new FloatManager();

    // List of training methods at current rank
    public List<SkillTrainingMethod> methods = new List<SkillTrainingMethod>();

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    /// <param name="ID">Skill ID in database.</param>
    public Skill(int ID) : base(GameManager.Instance.Database, ID)
    {
        index.OnChange += AudioController.Instance.PlayLevelUpSFX;
        index.OnChange += CreateTrainingMethods;
        CreateTrainingMethods();
    }

    /// <summary>
    ///     Checks if has available ranks to rank up.
    /// </summary>
    /// <returns>True if can rank up.</returns>
    public bool CanRankUp()
    {
        return index.Value + 1 < ranks.Count && index.Value + 1 < ranks.IndexOf(lastAvailableRank);
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
        if (index.Value - 1 >= ranks.IndexOf(firstAvailableRank))
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
        // if (stats.ContainsKey(key))
        // {
        //     return stats[key][index.Value];
        // }

        return stats.Where(v => SkillStatTypeModel.FindByID(v.Value.statID) == key)
            .Select(v => v.Value).Select(v => v.values[index.Value]).FirstOrDefault();

        // return 0;
    }

    /// <summary>
    ///     Gets the stat difference between the current rank and the previous.
    /// </summary>
    /// <param name="key">String of stat name</param>
    /// <returns>(Current - Previous) of the stat, 0 at the starting rank.</returns>
    public float GetStatBackwardDiff(string key)
    {
        // if (stats.ContainsKey(key))
        // {
        //     float curr = stats[key][index.Value];
        //     float prev = stats[key][Math.Max(0, index.Value - 1)];
        //     return curr - prev;
        // }

        if (!stats.ContainsKey(SkillStatTypeModel.FindByName(key)))
        {
            return 0;
        }

        SkillStatModel stat = stats
            .Where(v => SkillStatTypeModel.FindByID(v.Value.statID) == key)
            .Select(v => v.Value).First();

        float curr = stat.values[index.Value];
        float prev = stat.values[index.Value - 1];

        return curr - prev;
    }

    /// <summary>
    ///     Gets the stat difference between the next rank and the current one.
    /// </summary>
    /// <param name="key">String of stat name</param>
    /// <returns>(Next - Current) of the stat, 0 at the final rank.</returns>
    public float GetStatForwardDiff(string key)
    {
        // if (stats.ContainsKey(key))
        // {
        //     float next = stats[key][Math.Min(ranks.Count - 1, index.Value + 1)];
        //     float curr = stats[key][index.Value];
        //     return next - curr;
        // }

        if (!stats.ContainsKey(SkillStatTypeModel.FindByName(key)))
        {
            return 0;
        }

        SkillStatModel stat = stats
            .Where(v => SkillStatTypeModel.FindByID(v.Value.statID) == key)
            .Select(v => v.Value).First();

        float curr = stat.values[index.Value];
        float next = stat.values[Math.Min(ranks.Count - 1, index.Value + 1)];

        return next - curr;
    }

    /// <summary>
    ///     Gets the current skill load time in seconds, including rank modifiers.
    /// </summary>
    /// <returns></returns>
    public float GetLoadTime()
    {
        return baseLoadTime + GetStat("Load Time");
    }

    /// <summary>
    ///     Gets the current skill use time in seconds, including rank modifiers.
    /// </summary>
    /// <returns></returns>
    public float GetUseTime()
    {
        return baseUseTime + GetStat("Use Time");
    }

    /// <summary>
    ///     Gets the current skill cooldown time in seconds, including rank modifiers.
    /// </summary>
    /// <returns></returns>
    public float GetCooldownTime()
    {
        return baseCooldown + GetStat("Cooldown Time");
    }

    public bool CanUse()
    {
        return cooldown.Value == 0;
    }

    /// <summary>
    ///     Implements the rank-specific training methods.
    /// </summary>
    public void CreateTrainingMethods()
    {
        foreach (SkillTrainingMethod method in methods)
        {
            method.Clear();
        }

        methods.Clear();
        // Resets the max xp gainable.
        xpMax.Value = 0;

        // For every method, create a new method and insert into list.
        List<TrainingMethodModel> rankMethods = trainingMethods
            .Where(v => v.Key.Item2 == ranks[index.Value])
            .Select(v => v.Value)
            .ToList();

        foreach (TrainingMethodModel methodModel in rankMethods)
        {
            SkillTrainingMethod method =
                new SkillTrainingMethod(this, methodModel.trainingMethodID, methodModel.rank);
            xpMax.Value += methodModel.xpGainEach * methodModel.countMax;
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
    public IEnumerator Use(ResultController resultController)
    {
        // Calculates the base use time for the skill.
        float useTime = GetUseTime();
        float currTime = 0;
        // Interval for which audio should play
        float audioInterval = 1f;
        AudioSource audio = Player.Instance.gameObject.GetComponent<AudioSource>();

        while (useTime - currTime > Time.deltaTime)
        {
            // Find the remaining interval time and yield
            float interval = Math.Min(useTime - currTime, audioInterval);
            yield return new WaitForSecondsRealtime(interval);

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
        bool isSuccess = chance >= roll;
        resultController.Handle(isSuccess);

        GameManager.Instance.ExecuteCoroutine(StartCooldown(GetCooldownTime()));
    }

    /// <summary>
    ///     Places the skill on cooldown.
    /// </summary>
    /// <param name="time">Cooldown time in seconds.</param>
    /// <returns>Coroutine to be run.</returns>
    public IEnumerator StartCooldown(float time)
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

        cooldown.Value = 0;
    }
}
