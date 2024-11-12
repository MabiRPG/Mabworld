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

    public void Update<T>(T result)
    {
        if (prerequisites.Count > 0)
        {
            foreach (QuestConditionModel condition in prerequisites.ToList())
            {
                UpdateCondition(result, condition, prerequisites);
            }
        }
        else if (steps.Count > 0)
        {
            UpdateCondition(result, steps[0], steps);
            // Debug.Log(steps.Count);
        }
        else
        {
            Debug.Log("reward");
        }
    }

    private void UpdateCondition<T>(T result, QuestConditionModel condition, 
        List<QuestConditionModel> conditions)
    {
        int categoryID = condition.conditionID;
        int conditionCategoryID = QuestConditionTypeModel.GetCategory(categoryID);

        switch (QuestConditionCategoryTypeModel.FindByID(conditionCategoryID))
        {
            case "Skill":
                if (result.GetType() == typeof(ResultSkillController))
                {
                    HandleSkill(result as ResultSkillController, condition, conditions);
                }

                break;
            case "Item":
                if (result.GetType() == typeof(ResultItemController))
                {
                    HandleItem(result as ResultItemController, condition, conditions);
                }

                break;
            case "Cultivation stage":
                if (result.GetType() == typeof(Actor) || result.GetType() == typeof(Player))
                {
                    HandleCultivation(result as Actor, condition, conditions);
                }

                break;
            case "NPC":
                if (result.GetType() == typeof(ResultNPCInteractController))
                {
                    HandleNPC(result as ResultNPCInteractController, condition, conditions);
                }

                break;
            case "Dialogue":
                GameObject dialogueBox = GameObject.Instantiate(
                        GameManager.Instance.dialogueBoxPrefab, 
                        GameManager.Instance.canvas.transform);

                UI_DialogueBox script = dialogueBox.GetComponent<UI_DialogueBox>();

                script.SetDialogue(dialogues
                    .Where(v => v.conversationID == int.Parse(condition.param1))
                    .OrderBy(v => v.ID)
                    .ToList());

                conditions.Remove(condition);

                break;
            default:
                break;
        }
    }

    private void HandleSkill(ResultSkillController result, QuestConditionModel condition, 
        List<QuestConditionModel> conditions)
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
                    conditions.Remove(condition);
                }

                break;
            case "Rank up skill":
                if (actionType == ActionSkillController.ActionType.RankUp &&
                    skill.IsRankOrGreater(condition.param2))
                {
                    conditions.Remove(condition);
                }

                break;
            default:
                break;
        }
    }

    private void HandleItem(ResultItemController result, QuestConditionModel condition, 
        List<QuestConditionModel> conditions)
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
                    conditions.Remove(condition);
                }

                break;
            default:
                break;
        }
    }

    private void HandleCultivation(Actor actor, QuestConditionModel condition, 
        List<QuestConditionModel> conditions)
    {
        if (int.Parse(condition.param1) == actor.actorStage.Value &&
            int.Parse(condition.param2) <= actor.actorSubstage.Value)
        {
            conditions.Remove(condition);
        }
    }

    private void HandleNPC(ResultNPCInteractController result, QuestConditionModel condition, 
        List<QuestConditionModel> conditions)
    {
        if (int.Parse(condition.param1) == result.action.NPCID)
        {
            conditions.Remove(condition);
            UpdateCondition(result, steps[0], conditions);
        }
    }
}