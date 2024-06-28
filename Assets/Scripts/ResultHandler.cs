using System.Data;

public abstract class ResultHandler
{
    public bool isSuccess;
    public enum Type
    {
        None,
        Gather,
    }

    public Type type;
    public Skill skill;

    public abstract void SetSuccess(bool state);
}

public class MapResourceResultHandler : ResultHandler
{
    private int lootTableID;
    public int resourceID;
    public int resourceGain;
    private int remainingResource;
    public bool isEmpty;
    public EventManager mapEvent = new EventManager();

    public void SetResource(Skill skill, int lootTableID, int remainingResource)
    {
        type = Type.Gather;
        this.skill = skill;
        this.lootTableID = lootTableID;
        this.remainingResource = remainingResource;
    }

    public override void SetSuccess(bool state)
    {
        isSuccess = state;

        if (isSuccess && lootTableID != -1)
        {
            GameManager.Instance.lootGenerator.SetLootTable(lootTableID);
            (resourceID, resourceGain) = GameManager.Instance.lootGenerator.Generate();

            remainingResource -= resourceGain;

            if (remainingResource <= 0)
            {
                isEmpty = true;
            }
        }

        mapEvent.RaiseOnChange();
        Player.Instance.OnUsed();
        Player.Instance.MapResourceRaiseOnChange(this);
    }
} 