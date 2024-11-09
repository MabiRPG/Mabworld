using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class Quest : QuestModel
{
    public Quest(int ID) : base(GameManager.Instance.Database, ID)
    {
    }

    public void CheckPrereq<T>(T result)
    {
        foreach (QuestConditionModel condition in prerequisites.ToList())
        {
            int categoryID = condition.conditionID;
            int conditionCategoryID = QuestConditionTypeModel.GetCategory(categoryID);

            switch (QuestConditionCategoryTypeModel.FindByID(conditionCategoryID))
            {
                case "Skill":
                    if (typeof(T) == typeof(ResultSkillController))
                    {
                        HandleSkill(result as ResultSkillController, condition);
                    }

                    break;
                case "Item":
                    if (typeof(T) == typeof(ResultItemController))
                    {
                        HandleItem(result as ResultItemController, condition);
                    }

                    break;
                case "Cultivation stage":
                    if (typeof(T) == typeof(Actor) || typeof(T) == typeof(Player))
                    {
                        HandleCultivation(result as Actor, condition);
                    }

                    break;
                case "NPC":
                    if (typeof(T) == typeof(ResultNPCInteractController))
                    {
                        HandleNPC(result as ResultNPCInteractController, condition);
                    }

                    break;
                default:
                    break;
            }
        }
    }

    private void HandleSkill(ResultSkillController result, QuestConditionModel condition)
    {
        Skill skill = result.skill;
        ActionSkillController.ActionType actionType = result.action.type;

        if (int.Parse(condition.param1) != skill.ID)
        {
            return;
        }

        switch (QuestConditionTypeModel.FindByID(condition.conditionID))
        {
            case "Learn skill":
                if (actionType == ActionSkillController.ActionType.Learn &&
                    result.player.skillManager.IsLearned(skill.ID))
                {
                    prerequisites.Remove(condition);
                }

                break;
            case "Rank up skill":
                if (actionType == ActionSkillController.ActionType.RankUp &&
                    skill.IsRankOrGreater(condition.param2))
                {
                    prerequisites.Remove(condition);
                }

                break;
            default:
                break;
        }
    }

    private void HandleItem(ResultItemController result, QuestConditionModel condition)
    {
        ActionItemController.ActionType actionType = result.action.type;

        if (int.Parse(condition.param1) != result.action.itemID)
        {
            return;
        }

        switch (QuestConditionTypeModel.FindByID(condition.conditionID))
        {
            case "Get item":
                if (actionType == ActionItemController.ActionType.ItemAdd &&
                    result.action.itemCurrentQuantity >= int.Parse(condition.param2))
                {
                    prerequisites.Remove(condition);
                }

                break;
            default:
                break;
        }
    }

    private void HandleCultivation(Actor actor, QuestConditionModel condition)
    {
        if (int.Parse(condition.param1) == actor.actorStage.Value &&
            int.Parse(condition.param2) <= actor.actorSubstage.Value)
        {
            prerequisites.Remove(condition);
        }
    }

    private void HandleNPC(ResultNPCInteractController result, QuestConditionModel condition)
    {
        if (int.Parse(condition.param1) == result.action.NPCID)
        {
            prerequisites.Remove(condition);
            Debug.Log("hit");
        }
    }
}