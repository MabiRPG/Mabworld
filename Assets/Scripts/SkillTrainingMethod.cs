using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

/// <summary>
///     Handles all training method processing.
/// </summary>
public class SkillTrainingMethod
{
    // ID of method
    public int ID;
    // Name of method
    public string name;
    // XP gain for each count of method
    public float xpGainEach;
    // Current method counter
    public IntManager count = new IntManager();
    // Maximum counts of method
    public int countMax;
    private int itemID;
    
    // Skill instance
    private Skill skill;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    /// <param name="skill">Skill instance method belongs to.</param>
    /// <param name="ID">Method ID in database.</param>
    /// <param name="row">Database row to construct class.</param>
    public SkillTrainingMethod(Skill skill, DataRow row)
    {
        GameManager.Instance.ParseDatabaseRow(row, this, 
            ("training_method_id", "ID"), ("item_id", "itemID"));
        // Creates an empty counter for current method.
        count.Value = 0;

        this.skill = skill;

        Player.Instance.trainingEvent += Update;
    }

    /// <summary>
    ///     Checks if less than maximum counts, and checks training requirements.
    /// </summary>
    public void Update(MapResourceResultHandler resultHandler)
    {
        if (skill != resultHandler.skill)
        {
            return;
        }


        if (CheckTraining(resultHandler))
        {
            count.Value += 1;
            skill.AddXP(xpGainEach);

            // Clears the event handler when done
            if (IsComplete())
            {
                Clear();
            }
        }
    }

    /// <summary>
    ///     Checks if the training method has been complete
    /// </summary>
    /// <returns>True if complete, False otherwise</returns>
    public bool IsComplete()
    {
        return count.Value == countMax;
    }

    public void Clear()
    {
        Player.Instance.trainingEvent -= Update;
        count.Clear();
    }

    /// <summary>
    ///     Checks the training requirements against the status.
    /// </summary>
    /// <returns></returns>
    public bool CheckTraining(MapResourceResultHandler resultHandler)
    {
        if (ID == 1)
        {
            return IsSuccess(resultHandler);
        }
        else if (ID == 2)
        {
            return IsFail(resultHandler);
        }
        else if (ID == 3)
        {
            return IsGatherTwoOrMore(resultHandler);
        }
        else if (new List<int> { 4, 6, 8, 10, 12, 14, 16, 18, 20 }.Contains(ID))
        {
            return IsGatherResource(resultHandler);
        }
        else if (new List<int> { 5, 7, 9, 11, 13, 15, 17, 19, 21 }.Contains(ID))
        {
            return IsFullyGatherResource(resultHandler);
        }

        return false;
    }

    /// <summary>
    ///     Checks if the action was a success.
    /// </summary>
    /// <returns></returns>
    public bool IsSuccess(MapResourceResultHandler resultHandler)
    {
        return resultHandler.isSuccess;
    }

    /// <summary>
    ///     Checks if the action was a failure.
    /// </summary>
    /// <returns></returns>
    public bool IsFail(MapResourceResultHandler resultHandler)
    {
        return !IsSuccess(resultHandler);
    }

    /// <summary>
    ///     Checks if two or more resources were gathered at once.
    /// </summary>
    /// <returns></returns>
    public bool IsGatherTwoOrMore(MapResourceResultHandler resultHandler)
    {
        if (IsSuccess(resultHandler) && resultHandler.type == ResultHandler.Type.Gather
                && resultHandler.resourceGain > 1)
        {
            return true;
        }

        return false;
    }

    public bool IsGatherResource(MapResourceResultHandler resultHandler)
    {
        if (IsSuccess(resultHandler) && resultHandler.type == ResultHandler.Type.Gather 
                && resultHandler.resourceID == itemID)
        {
            return true;
        }

        return false;
    }

    public bool IsFullyGatherResource(MapResourceResultHandler resultHandler)
    {
        if (IsGatherResource(resultHandler) && resultHandler.isEmpty)
        {
            return true;
        }

        return false;
    }
}