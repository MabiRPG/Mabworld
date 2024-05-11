/// <summary>
///     Handles all player state results.
/// </summary>
public class Result
{
    // If action was success
    public bool isSuccess;
    // Type of action
    public enum Type
    {
        None,
        Gather
    }
    public Type type;
    public Skill skill;
    // Resource ID and gain if gather action
    public int resourceID;
    public int resourceGain;
    // Event handler for status
    public EventManager statusEvent = new EventManager();

    /// <summary>
    ///     Clears the status.
    /// </summary>
    public void Clear()
    {
        isSuccess = false;
        type = Type.None;
        skill = null;
        resourceID = -1;
        resourceGain = 0;
    }
}