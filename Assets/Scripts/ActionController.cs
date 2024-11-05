using System;
using System.Collections;
using UnityEngine;

public class ActionHandler
{
    protected Player player;
    protected object caller;
    public ActionType type;
    public enum ActionType
    {
        Gather,
        Craft,
        Dialogue,
    };
}

public class ActionController : ActionHandler
{
    public ActionController(Player player, ActionType type, object caller)
    {
        this.player = player;
        this.type = type;
        this.caller = caller;
    }

    public void Handle()
    {
        switch (type)
        {
            case ActionType.Gather:
                MapResource resource = (MapResource)caller;
                int skillID = resource.skillID;
                Skill skill = player.skillManager.Get(skillID);
                Vector3 position = resource.transform.TransformPoint(Vector3.zero);

                ResultController result = new ResultController(player, type, caller);
                IEnumerator task = player.controller.AttemptSkill(position, skill, result);
                player.controller.SetTask(task);

                break;
            default:
                break;
        }
    }
}