using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

/// <summary>
///     Handles all training method processing.
/// </summary>
public class SkillTrainingMethod : TrainingMethodModel
{
    public IntManager count = new IntManager();

    // Skill instance
    private Skill skill;

    public SkillTrainingMethod(Skill skill, int methodID, string rank)
        : base(GameManager.Instance.Database, skill.ID, methodID, rank)
    {
        this.skill = skill;
        count.Value = 0;
        // name = TrainingMethodTypeModel.FindByID(methodID);
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
        switch (TrainingMethodTypeModel.FindByID(trainingMethodID))
        {
            case "Success":
                return IsSuccess(resultHandler);
            case "Fail":
                return IsFail(resultHandler);
            case "Gather":
                return IsGatherResource(resultHandler);
            case "Fully gather":
                return IsFullyGatherResource(resultHandler);
            default:
                break;
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
                && resultHandler.resourceID == int.Parse(param1))
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