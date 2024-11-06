using System;
using System.Collections;
using UnityEngine;

public abstract class ActionHandler
{
    protected Player player;
    protected object caller;

    public ActionHandler(Player player, object caller)
    {
        this.player = player;
        this.caller = caller;
    }

    public abstract void Handle();
}

public class ActionGatherController : ActionHandler
{
    public ActionGatherController(Player player, object caller) : base(player, caller)
    {
    }

    public override void Handle()
    {
        MapResource resource = (MapResource)caller;
        int skillID = resource.skillID;
        Skill skill = player.skillManager.Get(skillID);
        Vector3 position = resource.transform.TransformPoint(Vector3.zero);

        ResultGatherController result = new ResultGatherController(player, caller);
        IEnumerator task = player.controller.AttemptSkill(position, skill, result);
        player.controller.SetTask(task);
    }
}

public class ActionSkillController : ActionHandler
{
    private Skill skill;
    private int skillID;
    private ActionType type;
    public enum ActionType
    {
        RankUp,
        RankDown,
        Learn
    }

    public ActionSkillController(Player player, object caller, int skillID, 
        ActionType type = ActionType.Learn) : base(player, caller)
    {
        this.skillID = skillID;
        this.type = type;
    }

    public ActionSkillController(Player player, object caller, Skill skill, ActionType type) 
        : base(player, caller)
    {
        this.skill = skill;
        this.type = type;
    }

    public override void Handle()
    {
        ResultSkillController result = new ResultSkillController(player, caller, skill, type);

        switch (type)
        {
            case ActionType.Learn:
                player.skillManager.Learn(skillID);
                skill = player.skillManager.Get(skillID);
                result.skill = skill;
                result.Handle(true);

                break;
            case ActionType.RankUp:
                if (!skill.CanRankUp() || skill.xp.Value < 100 
                    || !player.skillManager.IsLearned(skill))
                {
                    result.Handle(false);
                }
                else
                {
                    skill.RankUp();
                    result.Handle(true);
                }

                break;
            default:
                break;
        }
    }
}