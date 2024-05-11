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
    public int count;
    // Maximum counts of method
    public float countMax;
    
    // Skill instance
    private Skill skill;
    // Player result
    public Result result = null;
    // Event handlers
    public EventManager countEvent = new EventManager();

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    /// <param name="newID">Method ID in database.</param>
    /// <param name="newName">Method name.</param>
    /// <param name="newXpGainEach">XP gain every count of method.</param>
    /// <param name="newCountMax">Maximum count of method.</param>
    public SkillTrainingMethod(Skill newSkill, int newID, string newName, 
        float newXpGainEach, float newCountMax)
    {
        ID = newID;
        name = newName;
        xpGainEach = newXpGainEach;
        countMax = newCountMax;
        // Creates an empty counter for current method.
        count = 0;

        skill = newSkill;

        Player.Instance.result.statusEvent.OnChange += Update;
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
            count += 1;
            skill.AddXP(xpGainEach);
            countEvent.RaiseOnChange();

            if (IsComplete())
            {
                Clear();
            }
        }
    }

    public bool IsComplete()
    {
        return count == countMax;
    }

    public void Clear()
    {
        Player.Instance.result.statusEvent.OnChange -= Update;
        countEvent.Clear();
    }

    //--------------------------------------------------------------------------
    // * Checks training requirements against the status flag.
    //--------------------------------------------------------------------------
    public bool CheckTraining()
    {
        switch(ID)
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
        return result.isSuccess;
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
        if (IsSuccess() && result.type == Result.Type.Gather && result.resourceGain > 1)
        {
            return true;
        }

        return false;
    }
}