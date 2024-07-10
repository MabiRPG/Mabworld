using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Handles all skill processing for an actor.
/// </summary>
public class SkillManager
{
    public SkillBubble bubble;

    // Hashmap of skills on (Skill ID, Skill instance)
    public Dictionary<int, Skill> Skills = new Dictionary<int, Skill>();

    public SkillManager(SkillBubble bubble)
    {
        this.bubble = bubble;
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

        Skills.Add(ID, new Skill(ID));
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

    /// <summary>
    ///     Prepares the skill to be readied (loaded) and gets the coroutine to be run.
    /// </summary>
    /// <param name="skill">Skill instance to ready</param>
    /// <returns>Coroutine to be run.</returns>
    public IEnumerator Ready(Skill skill)
    {
        if (IsLearned(skill) && bubble != null)
        {
            return bubble.Pulse(skill.icon, skill.GetLoadTime());
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
    ///     Prepares the skill to be used, and gets the coroutine to be run.
    /// </summary>
    /// <param name="skill">Skill instance to use</param>
    /// <param name="resultHandler">ResultHandler derived class object to handle events</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Coroutine to be run.</returns>
    public IEnumerator Use<T>(Skill skill, T resultHandler) where T : ResultHandler
    {
        return skill.Use(resultHandler);
    }

    public IEnumerator Cooldown(Skill skill)
    {
        return skill.Cooldown(skill.GetCooldownTime());
    }
}