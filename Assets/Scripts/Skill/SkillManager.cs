using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
///     Handles all skill processing for an actor.
/// </summary>
[JsonObject]
public class SkillManager
{
    [JsonIgnore]
    public SkillBubble bubble;

    // Hashmap of skills on (Skill ID, Skill instance)
    public Dictionary<int, Skill> Skills;
    [JsonIgnore]
    public EventManager learnEvent;

    public Dictionary<int, string> Categories;

    [JsonRequired]
    private HashSet<int> learnedCategoryIDs;

    public SkillManager(SkillBubble bubble)
    {
        this.bubble = bubble;

        Skills = new Dictionary<int, Skill>();
        learnEvent = new EventManager();
        learnedCategoryIDs = new HashSet<int>();
        Categories = new Dictionary<int, string>();

        foreach ((int stageID, int substageID) in CultivationStageModel.stages.Keys)
        {
            if (!Categories.ContainsKey(stageID) && stageID < 1000)
            {
                Categories.Add(stageID, CultivationStageModel.stages[(stageID, substageID)].name);
            }
        }
    }

    /// <summary>
    ///     Gets the Skill instance from the actor, if it exists.
    /// </summary>
    /// <param name="ID">Skill ID in database</param>
    /// <returns>Skill instance of ID, null if it does not exist.</returns>
    public Skill Get(int ID)
    {
        if (IsLearned(ID))
        {
            return Skills[ID];
        }

        return null;
    }

    /// <summary>
    ///     Checks if the skill has been learned
    /// </summary>
    /// <param name="ID">Skill ID in database</param>
    /// <returns>True if learned, False otherwise</returns>
    public bool IsLearned(int ID)
    {
        return Skills.ContainsKey(ID);
    }

    /// <summary>
    ///     Checks if the skill has been learned
    /// </summary>
    /// <param name="skill">Skill instance to check</param>
    /// <returns>True if learned, False otherwise</returns>
    public bool IsLearned(Skill skill)
    {
        return Skills.ContainsValue(skill);
    }

    /// <summary>
    ///     Learns the skill
    /// </summary>
    /// <param name="ID">Skill ID in database</param>
    public void Learn(int ID)
    {
        if (IsLearned(ID))
        {
            return;
        }

        Skill skill = new Skill(ID);
        Skills.Add(ID, skill);
        learnedCategoryIDs.Add(skill.model.cultivationStageID);
        learnEvent.RaiseOnChange();
    }

    /// <summary>
    ///     Unlearns the skill
    /// </summary>
    /// <param name="ID">Skill ID in database</param>
    public void Unlearn(int ID)
    {
        if (!IsLearned(ID))
        {
            return;
        }

        Skills.Remove(ID);
    }

    public List<string> GetLearnedCategories()
    {
        List<string> categories = new List<string>();

        foreach (int ID in learnedCategoryIDs)
        {
            if (Categories.ContainsKey(ID))
            {
                categories.Add(Categories[ID]);
            }
        }

        return categories;
    }

    public List<Skill> GetLearnedSkillsByCategory(int categoryID)
    {
        List<Skill> skills = new List<Skill>();

        foreach (Skill skill in Skills.Values)
        {
            if (skill.model.cultivationStageID == categoryID)
            {
                skills.Add(skill);
            }
        }

        return skills;
    }

    /// <summary>
    ///     Prepares the skill to be readied (loaded) and gets the coroutine to be run.
    /// </summary>
    /// <param name="skill">Skill instance to ready</param>
    /// <returns>Coroutine to be run.</returns>
    public IEnumerator Ready(Skill skill)
    {
        if (IsLearned(skill) && bubble != null)
        {
            return bubble.Pulse(skill.model.icon, skill.GetLoadTime());
        }

        return null;
    }

    /// <summary>
    ///     Prepares the skill to be cancelled, and gets the coroutine to be run.
    /// </summary>
    /// <returns>Coroutine to be run.</returns>
    public IEnumerator Cancel()
    {
        if (bubble != null)
        {
            return bubble.Fade();
        }

        return null;
    }

    /// <summary>
    ///     Puts the skill on cooldown, and gets the coroutine to be run.
    /// </summary>
    /// <param name="skill">Skill instance to use</param>
    /// <returns>Coroutine to be run.</returns>
    public IEnumerator Cooldown(Skill skill)
    {
        return skill.StartCooldown(skill.GetCooldownTime());
    }
}