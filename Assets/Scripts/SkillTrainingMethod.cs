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
    public ValueManager xpGainEach = new ValueManager();
    // Current method counter
    public ValueManager count = new ValueManager();
    // Maximum counts of method
    public ValueManager countMax = new ValueManager();
    
    // Skill instance
    private Skill skill;
    // Player result
    public Result result = null;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    /// <param name="ID">Method ID in database.</param>
    /// <param name="name">Method name.</param>
    /// <param name="xpGainEach">XP gain every count of method.</param>
    /// <param name="countMax">Maximum count of method.</param>
    public SkillTrainingMethod(Skill skill, int ID, string name, 
        float xpGainEach, float countMax)
    {
        this.ID = ID;
        this.name = name;
        this.xpGainEach.Value = xpGainEach;
        this.countMax.Value = countMax;
        // Creates an empty counter for current method.
        count.Value = 0;

        this.skill = skill;

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
            count.Value += 1;
            skill.AddXP(xpGainEach.Value);

            if (IsComplete())
            {
                Clear();
            }
        }
    }

    public bool IsComplete()
    {
        return count.Value == countMax.Value;
    }

    public void Clear()
    {
        Player.Instance.result.statusEvent.OnChange -= Update;
        count.Clear();
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