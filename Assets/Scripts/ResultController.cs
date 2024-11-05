public class ResultController : ActionHandler
{
    public int resourceID;
    public int resourceGain;

    public ResultController(Player player, ActionType type, object caller)
    {
        this.player = player;
        this.type = type;
        this.caller = caller;
    }

    public void Handle(bool isSuccess)
    {
        switch (type)
        {
            case ActionType.Gather:
            {
                MapResource resource = (MapResource)caller;

                if (isSuccess)
                {
                    GameManager.Instance.lootGenerator.SetLootTable(resource.lootTableID);
                    (resourceID, resourceGain) = GameManager.Instance.lootGenerator.Generate();
                    resource.UpdateResource();

                    Player.Instance.AddXP(50);
                }

                int skillID = resource.skillID;
                Skill skill = player.skillManager.Get(skillID);

                foreach (SkillTrainingMethod method in skill.methods)
                {
                    method.Update(this, caller, isSuccess);
                }

                AudioController.Instance.PlayGatherResultSFX(isSuccess);
                
                break;
            }
            default:
                break;
        }
    }
}