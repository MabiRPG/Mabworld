using System;
using UnityEngine;

public abstract class ResultHandler
{
    public Player player;
    public object caller;
    public Skill skill;

    public ResultHandler(Player player, object caller)
    {
        this.player = player;
        this.caller = caller;
    }

    public virtual void Handle(bool isSuccess)
    {
        if (skill != null)
        {
            foreach (SkillTrainingMethod method in skill.methods)
            {
                method.Update(this, caller, isSuccess);
            }
        } 

        foreach (Quest quest in player.quests.Values)
        {
            quest.Update(this);
        }
    }
}

public class ResultGatherController : ResultHandler
{
    public int resourceID;
    public int resourceGain;

    public ResultGatherController(Player player, object caller, Skill skill) : base(player, caller)
    {
        this.skill = skill;
    }

    public override void Handle(bool isSuccess)
    {
        MapResource resource = (MapResource)caller;

        if (isSuccess)
        {
            ActionItemController action = new ActionItemController(player, caller,
                resource.model.lootTableID);
            action.Handle();
            resourceID = action.itemID;
            resourceGain = action.itemCurrentQuantity - action.itemPreviousQuantity;
            resource.UpdateResource();
            Player.Instance.AddXP(100);
        }

        AudioController.Instance.PlayGatherResultSFX(isSuccess);

        base.Handle(isSuccess);
    }
}

public class ResultSkillController : ResultHandler
{
    public ActionSkillController action;

    public ResultSkillController(Player player, object caller, Skill skill, 
        ActionSkillController action) : base(player, caller)
    {
        this.skill = skill;
        this.action = action;
    }
}

public class ResultItemController : ResultHandler
{
    public ActionItemController action;

    public ResultItemController(Player player, object caller, ActionItemController action) 
        : base(player, caller)
    {
        this.action = action;
    }
}

public class ResultNPCInteractController : ResultHandler
{
    public ActionNPCInteractController action;

    public ResultNPCInteractController(Player player, object caller, 
        ActionNPCInteractController action) : base(player, caller)
    {
        this.action = action;
    }
}