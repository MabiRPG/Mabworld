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
    public ValueManager count = new ValueManager();
    // Maximum counts of method
    public int countMax;
    
    // Skill instance
    private Skill skill;
    // Player result
    private Result result = null;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    /// <param name="skill">Skill instance method belongs to.</param>
    /// <param name="ID">Method ID in database.</param>
    /// <param name="row">Database row to construct class.</param>
    public SkillTrainingMethod(Skill skill, DataRow row)
    {
        GameManager.Instance.ParseDatabaseRow(row, this, ("training_method_id", "ID"));
        // Creates an empty counter for current method.
        count.Value = 0;

        this.skill = skill;

        Player.Instance.result.trainingEvent.OnChange += Update;
    }

    /// <summary>
    ///     Checks if less than maximum counts, and checks training requirements.
    /// </summary>
    public void Update()
    {
        result = Player.Instance.result;

        if (skill != result.skill)
        {
            return;
        }

        if (CheckTraining())
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
        Player.Instance.result.trainingEvent.OnChange -= Update;
        count.Clear();
    }

    /// <summary>
    ///     Checks the training requirements against the status.
    /// </summary>
    /// <returns></returns>
    public bool CheckTraining()
    {
        switch(ID)
        {
            case 1:
                return IsSuccess();
            case 2:
                return IsFail();
            case 3:
                return IsGatherTwoOrMore();
        }

        return false;
    }

    /// <summary>
    ///     Checks if the action was a success.
    /// </summary>
    /// <returns></returns>
    public bool IsSuccess()
    {
        return result.isSuccess;
    }

    /// <summary>
    ///     Checks if the action was a failure.
    /// </summary>
    /// <returns></returns>
    public bool IsFail()
    {
        return !IsSuccess();
    }

    /// <summary>
    ///     Checks if two or more resources were gathered at once.
    /// </summary>
    /// <returns></returns>
    public bool IsGatherTwoOrMore()
    {
        if (IsSuccess() && result.type == Result.Type.Gather && result.resourceGain > 1)
        {
            return true;
        }

        return false;
    }

    public bool IsGatherOakLog()
    {
        if (IsSuccess() && result.type == Result.Type.Gather && result.resourceID == -1)
        {
        }

        return false;
    }
}