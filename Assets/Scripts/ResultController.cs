using System;
using UnityEngine;

public abstract class ResultHandler
{
    public Player player;
    public object caller;

    public ResultHandler(Player player, object caller)
    {
        this.player = player;
        this.caller = caller;
    }

    public abstract void Handle(bool isSuccess);
}

public class ResultGatherController : ResultHandler
{
    public int resourceID;
    public int resourceGain;

    public ResultGatherController(Player player, object caller) : base(player, caller)
    {
    }

    public override void Handle(bool isSuccess)
    {
        MapResource resource = (MapResource)caller;

        if (isSuccess)
        {
            // GameManager.Instance.lootGenerator.SetLootTable(resource.lootTableID);
            // (resourceID, resourceGain) = GameManager.Instance.lootGenerator.Generate();
            ActionItemController action = new ActionItemController(player, caller,
                resource.lootTableID);
            action.Handle();
            resource.UpdateResource();
            Player.Instance.AddXP(100);
        }

        int skillID = resource.skillID;
        Skill skill = player.skillManager.Get(skillID);

        foreach (SkillTrainingMethod method in skill.methods)
        {
            method.Update(this, caller, isSuccess);
        }

        AudioController.Instance.PlayGatherResultSFX(isSuccess);
    }
}

public class ResultSkillController : ResultHandler
{
    public Skill skill;
    public ActionSkillController action;

    public ResultSkillController(Player player, object caller, Skill skill, 
        ActionSkillController action) : base(player, caller)
    {
        this.skill = skill;
        this.action = action;
    }

    public override void Handle(bool isSuccess)
    {
        foreach (Quest quest in player.quests.Values)
        {
            quest.CheckPrereq(this);
        }
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

    public override void Handle(bool isSuccess)
    {
        foreach (Quest quest in player.quests.Values)
        {
            quest.CheckPrereq(this);
        }
    }
}