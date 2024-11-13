using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class QuestCondition
{
    public readonly QuestConditionModel model;
    private bool state;

    public QuestCondition(QuestConditionModel model)
    {
        this.model = model;
        State = false;
    }

    public bool State { get => state; set => state = value; }
}

public class Quest : QuestModel
{
    private List<QuestCondition> prerequisiteStates = new List<QuestCondition>();
    private List<QuestCondition> stepStates = new List<QuestCondition>();
    private enum QuestState
    {
        NeedPrerequisite,
        InProgress,
        Complete
    }
    private QuestState questState;
    private int stepCounter = 0;

    public Quest(int ID) : base(GameManager.Instance.Database, ID)
    {
        foreach (QuestConditionModel model in prerequisites)
        {
            prerequisiteStates.Add(new QuestCondition(model));
        }

        foreach (QuestConditionModel model in steps)
        {
            stepStates.Add(new QuestCondition(model));
        }

        questState = QuestState.NeedPrerequisite;
    }

    public void Update<T>(T result)
    {
        if (questState == QuestState.NeedPrerequisite)
        {
            foreach (QuestCondition condition in prerequisiteStates)
            {
                UpdateCondition(result, condition, prerequisiteStates);
            }

            if (prerequisiteStates.All(v => v.State))
            {
                questState = QuestState.InProgress;
            }
        }

        foreach (QuestCondition condition in stepStates)
        {
            UpdateCondition(result, condition, stepStates);
        }

        if (questState == QuestState.InProgress)
        {
            stepCounter = 0;

            foreach (QuestCondition condition in stepStates)
            {
                if (condition.State)
                {
                    stepCounter++;
                }
                else
                {
                    break;
                }
            }
        }

        Debug.Log(questState);
        Debug.Log($"Prereq: {prerequisiteStates.Where(v => v.State).Count()}/{prerequisiteStates.Count}");
        Debug.Log($"Step: {stepStates.Where(v => v.State).Count()}/{stepStates.Count}");
        Debug.Log($"Step Counter: {stepCounter}");
    }

    private void UpdateCondition<T>(T result, QuestCondition condition, List<QuestCondition> conditions)
    {
        int categoryID = condition.model.conditionID;
        int conditionCategoryID = QuestConditionTypeModel.GetCategory(categoryID);

        switch (QuestConditionCategoryTypeModel.FindByID(conditionCategoryID))
        {
            case "Skill":
                if (result.GetType() == typeof(ResultSkillController))
                {
                    HandleSkill(result as ResultSkillController, condition);
                }

                break;
            case "Item":
                if (result.GetType() == typeof(ResultItemController))
                {
                    HandleItem(result as ResultItemController, condition);
                }

                break;
            case "Cultivation stage":
                if (result.GetType() == typeof(Actor) || result.GetType() == typeof(Player))
                {
                    HandleCultivation(result as Actor, condition);
                }

                break;
            case "NPC":
                if (result.GetType() == typeof(ResultNPCInteractController))
                {
                    HandleNPC(result as ResultNPCInteractController, condition, conditions);
                }

                break;
            default:
                break;
        }
    }

    private void HandleSkill(ResultSkillController result, QuestCondition condition)
    {
        Skill skill = result.skill;
        ActionSkillController.ActionType actionType = result.action.type;

        if (int.Parse(condition.model.param1) != skill.ID)
        {
            return;
        }

        switch (QuestConditionTypeModel.FindByID(condition.model.conditionID))
        {
            case "Learn skill":
                if (actionType == ActionSkillController.ActionType.Learn &&
                    result.player.skillManager.IsLearned(skill.ID))
                {
                    condition.State = true;
                }
                else
                {
                    condition.State = false;
                }

                break;
            case "Rank up skill":
                if (actionType == ActionSkillController.ActionType.RankUp &&
                    skill.IsRankOrGreater(condition.model.param2))
                {
                    condition.State = true;
                }
                else
                {
                    condition.State = false;
                }

                break;
            default:
                break;
        }
    }

    private void HandleItem(ResultItemController result, QuestCondition condition)
    {
        ActionItemController.ActionType actionType = result.action.type;

        if (int.Parse(condition.model.param1) != result.action.itemID)
        {
            return;
        }

        switch (QuestConditionTypeModel.FindByID(condition.model.conditionID))
        {
            case "Get item":
                if (result.action.itemCurrentQuantity >= int.Parse(condition.model.param2))
                {
                    condition.State = true;
                }
                else
                {
                    condition.State = false;
                }

                break;
            default:
                break;
        }
    }

    private void HandleCultivation(Actor actor, QuestCondition condition)
    {
        if (int.Parse(condition.model.param1) == actor.actorStage.Value &&
            int.Parse(condition.model.param2) <= actor.actorSubstage.Value)
        {
            condition.State = true;
        }
        else
        {
            condition.State = false;
        }
    }

    private void HandleNPC(ResultNPCInteractController result, QuestCondition condition,
        List<QuestCondition> conditions)
    {
        if (int.Parse(condition.model.param1) == result.action.NPCID)
        {
            condition.State = true;

            QuestCondition nextCondition = conditions
                .Where(v => v.model.stepID == condition.model.stepID + 1)
                .FirstOrDefault();

            if (nextCondition == null)
            {
                return;
            }

            int nextCategoryID = QuestConditionTypeModel.GetCategory(nextCondition.model.conditionID);
            string nextCategory = QuestConditionCategoryTypeModel.FindByID(nextCategoryID);

            if (nextCategory != "Dialogue")
            {
                return;
            }

            if (conditions.Where(v => v.model.stepID < nextCondition.model.stepID).All(v => v.State))
            {
                GameObject dialogueBox = GameObject.Instantiate(
                        GameManager.Instance.dialogueBoxPrefab,
                        GameManager.Instance.canvas.transform);

                UI_DialogueBox script = dialogueBox.GetComponent<UI_DialogueBox>();

                script.SetDialogue(dialogues
                    .Where(v => v.conversationID == int.Parse(condition.model.param1))
                    .OrderBy(v => v.ID)
                    .ToList());

                nextCondition.State = true;
            }
        }
        else
        {
            condition.State = false;
        }
    }

    // private void HandleDialogue(QuestCondition condition)
    // {
    //     int stepID = condition.model.stepID;
    //     QuestCondition prevCondition = conditions
    //         .Where(v => v.model.stepID == stepID - 1)
    //         .FirstOrDefault();

    //     if (prevCondition != null)
    //     {
    //         int prevCategoryID = QuestConditionTypeModel.GetCategory(
    //             prevCondition.model.conditionID);
    //         string prevCategory = QuestConditionCategoryTypeModel.FindByID(prevCategoryID);

    //         if (conditions.Where(v => v.model.stepID < stepID).All(v => v.State) &&
    //             prevCategory == "NPC")
    //         {
    //             GameObject dialogueBox = GameObject.Instantiate(
    //                     GameManager.Instance.dialogueBoxPrefab,
    //                     GameManager.Instance.canvas.transform);

    //             UI_DialogueBox script = dialogueBox.GetComponent<UI_DialogueBox>();

    //             script.SetDialogue(dialogues
    //                 .Where(v => v.conversationID == int.Parse(condition.model.param1))
    //                 .OrderBy(v => v.ID)
    //                 .ToList());

    //             condition.State = true;
    //         }
    //     }
    // }
}