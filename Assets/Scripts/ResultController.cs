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
    }
}

public class ResultSkillController : ResultHandler
{
    public Skill skill;
    public ActionSkillController.ActionType type;

    public ResultSkillController(Player player, object caller, Skill skill, 
        ActionSkillController.ActionType type) : base(player, caller)
    {
        this.skill = skill;
        this.type = type;
    }

    public override void Handle(bool isSuccess)
    {
        foreach (Quest quest in player.quests.Values)
        {
            quest.CheckPrereq(this);
        }
    }    
}