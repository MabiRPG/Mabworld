/// <summary>
///     Base class for all result handling (i.e. what to do after a skill is used).
///     Must be inherited and implemented to be used.
/// </summary>
public abstract class ResultHandler
{
    // If the resulting action was a success or not.
    public bool isSuccess;
    // The type of action that was done
    public enum Type
    {
        None,
        Gather,
    }
    public Type type;
    // The skill that was used, if any.
    public Skill skill;

    /// <summary>
    ///     Changes the success state and updates all subscribed members.
    /// </summary>
    /// <param name="state">True if success, False otherwise.</param>
    public abstract void SetSuccess(bool state);
}

/// <summary>
///     Derived class of ResultHandler specifically for interactable map resources.
///     Updates the map resource, and triggers the loot generation as required.
/// </summary>
public class MapResourceResultHandler : ResultHandler
{
    private int lootTableID;
    public int resourceID;
    // How many of the resource the player gets.
    public int resourceGain;
    // How much is left of the map resource.
    private int remainingResource;
    // If the map resource is empty
    public bool isEmpty;
    public EventManager mapEvent = new EventManager();

    /// <summary>
    ///     Sets the current resource.
    /// </summary>
    /// <param name="skill">Skill instance to collect resource.</param>
    /// <param name="lootTableID">Loot Table ID in database.</param>
    /// <param name="remainingResource">Remaining resource in the pile.</param>
    public void SetResource(Skill skill, int lootTableID, int remainingResource)
    {
        type = Type.Gather;
        this.skill = skill;
        this.lootTableID = lootTableID;
        this.remainingResource = remainingResource;

        if (remainingResource > 0)
        {
            isEmpty = false;
        }
    }

    /// <summary>
    ///     Sets the success of the result.
    /// </summary>
    /// <param name="state">True if success, False otherwise.</param>
    public override void SetSuccess(bool state)
    {
        isSuccess = state;

        if (isSuccess)
        {
            // Gives 100 experience to the player on success
            Player.Instance.AddXP(100);

            // If there is a loot table, generate loot and give it to the player.
            // Also, deduct the gained resources from the pile.
            if (lootTableID != -1)
            {
                GameManager.Instance.lootGenerator.SetLootTable(lootTableID);
                (resourceID, resourceGain) = GameManager.Instance.lootGenerator.Generate();

                remainingResource--;

                if (remainingResource == 0)
                {
                    isEmpty = true;
                }
            }
        }

        mapEvent.RaiseOnChange();
        Player.Instance.OnUsed();
        Player.Instance.MapResourceRaiseOnChange(this);
    }
} 