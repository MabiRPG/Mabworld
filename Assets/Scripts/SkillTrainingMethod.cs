using System.Collections.Generic;

/// <summary>
///     Handles all training method processing.
/// </summary>
public class SkillTrainingMethod
{
    // Creates dictionary for the method information, and status update flag from player.
    public Dictionary<string, object> method = new Dictionary<string, object>();
    public Dictionary<string, object> status = new Dictionary<string, object>();

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    /// <param name="skill_id">Skill ID in database.</param>
    /// <param name="rank">Current rank string representation.</param>
    /// <param name="method_id">Method ID in database.</param>
    /// <param name="methodName">Method name.</param>
    /// <param name="xp_gain_each">XP gain every count of method.</param>
    /// <param name="count_max">Maximum count of method.</param>
    public SkillTrainingMethod(object skill_id, object rank, object method_id, object methodName, 
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

    /// <summary>
    ///     Checks if less than maximum counts, and checks training requirements.
    /// </summary>
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