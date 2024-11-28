using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

[Serializable]
public class QuestCondition
{
    [SerializeField]
    public QuestConditionModel model;
    [SerializeField]
    public BoolManager state = new BoolManager();

    public QuestCondition(QuestConditionModel model)
    {
        this.model = model;
        state.Value = false;
    }
}

[Serializable]
public class Quest : QuestModel
{
    public List<QuestCondition> prerequisiteStates = new List<QuestCondition>();
    public List<QuestCondition> stepStates = new List<QuestCondition>();
    public List<QuestCondition> rewardStates = new List<QuestCondition>();

    public enum State
    {
        NeedPrerequisite,
        InProgress,
        Complete,
        RewardObtained
    }

    private State _questState;
    public State QuestState
    {
        get { return _questState; }
        set
        {
            _questState = value;
            OnStateChange?.Invoke();
        }
    }
    public event Action OnStateChange;

    [SerializeField]
    private int stepCounter = 0;

    public Quest(int ID)
        : base(GameManager.Instance.Database, ID)
    {
        foreach (QuestConditionModel model in prerequisites)
        {
            prerequisiteStates.Add(new QuestCondition(model));
        }

        foreach (QuestConditionModel model in steps)
        {
            stepStates.Add(new QuestCondition(model));
        }

        foreach (QuestConditionModel model in rewards)
        {
            rewardStates.Add(new QuestCondition(model));
        }

        QuestState = State.NeedPrerequisite;
    }

    public void Update<T>(T result)
    {
        if (QuestState == State.RewardObtained)
        {
            return;
        }

        if (QuestState == State.NeedPrerequisite)
        {
            foreach (QuestCondition condition in prerequisiteStates)
            {
                UpdateCondition(result, condition, prerequisiteStates);
            }

            if (prerequisiteStates.All(v => v.state.Value))
            {
                QuestState = State.InProgress;
            }
        }

        if (QuestState != State.Complete)
        {
            foreach (QuestCondition condition in stepStates)
            {
                UpdateCondition(result, condition, stepStates);
            }

            stepCounter = 0;

            foreach (QuestCondition condition in stepStates)
            {
                if (condition.state.Value)
                {
                    stepCounter++;
                }
                else
                {
                    break;
                }
            }

            if (stepCounter == stepStates.Count)
            {
                QuestState = State.Complete;
            }
        }

        // Debug.Log(questState);
        // Debug.Log($"Prereq: {prerequisiteStates.Where(v => v.State).Count()}/{prerequisiteStates.Count}");
        // Debug.Log($"Step: {stepStates.Where(v => v.State).Count()}/{stepStates.Count}");
        // Debug.Log($"Step Counter: {stepCounter}");
    }

    private void UpdateCondition<T>(
        T result,
        QuestCondition condition,
        List<QuestCondition> conditions
    )
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
            case "Quest":
                if (result.GetType() == typeof(ResultQuestController))
                {
                    HandleQuest(result as ResultQuestController, condition, conditions);
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
                if (
                    actionType == ActionSkillController.ActionType.Learn
                    && result.player.skillManager.IsLearned(skill.ID)
                )
                {
                    condition.state.Value = true;
                }
                else if (!result.player.skillManager.IsLearned(skill.ID))
                {
                    condition.state.Value = false;
                }

                break;
            case "Rank up skill":
                if (
                    actionType == ActionSkillController.ActionType.RankUp
                    && skill.IsRankOrGreater(condition.model.param2)
                )
                {
                    condition.state.Value = true;
                }
                else if (!skill.IsRankOrGreater(condition.model.param2))
                {
                    condition.state.Value = false;
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
                    condition.state.Value = true;
                }
                else
                {
                    condition.state.Value = false;
                }

                break;
            default:
                break;
        }
    }

    private void HandleCultivation(Actor actor, QuestCondition condition)
    {
        if (
            int.Parse(condition.model.param1) <= actor.actorStage.Value
            && int.Parse(condition.model.param2) <= actor.actorSubstage.Value
        )
        {
            condition.state.Value = true;
        }
        else
        {
            condition.state.Value = false;
        }
    }

    private void HandleNPC(
        ResultNPCInteractController result,
        QuestCondition condition,
        List<QuestCondition> conditions
    )
    {
        if (
            int.Parse(condition.model.param1) == result.action.NPCID
            && condition.model.stepID == stepCounter + 1
            && QuestState == State.InProgress
        )
        {
            condition.state.Value = true;

            QuestCondition nextCondition = conditions
                .Where(v => v.model.stepID == condition.model.stepID + 1)
                .FirstOrDefault();

            if (nextCondition == null)
            {
                return;
            }

            int nextCategoryID = QuestConditionTypeModel.GetCategory(
                nextCondition.model.conditionID
            );
            string nextCategory = QuestConditionCategoryTypeModel.FindByID(nextCategoryID);

            if (nextCategory != "Dialogue")
            {
                return;
            }

            GameObject dialogueBox = GameObject.Instantiate(
                GameManager.Instance.dialogueBoxPrefab,
                GameManager.Instance.canvas.transform
            );

            UI_DialogueBox script = dialogueBox.GetComponent<UI_DialogueBox>();

            script.SetDialogue(
                dialogues
                    .Where(v => v.conversationID == int.Parse(condition.model.param1))
                    .OrderBy(v => v.ID)
                    .ToList()
            );

            nextCondition.state.Value = true;
        }
    }

    private void HandleQuest(ResultQuestController result,
        QuestCondition condition, List<QuestCondition> conditions)
    {
        if (result.quest.ID != int.Parse(condition.model.param1))
        {
            return;
        }

        if (result.quest.QuestState == State.RewardObtained)
        {
            condition.state.Value = true;
        }
    }

    public void GiveRewards()
    {
        if (QuestState != State.Complete)
        {
            return;
        }

        foreach (QuestConditionModel condition in rewards)
        {
            int categoryID = condition.conditionID;
            int conditionCategoryID = QuestConditionTypeModel.GetCategory(categoryID);

            switch (QuestConditionCategoryTypeModel.FindByID(conditionCategoryID))
            {
                case "Skill":
                    GiveSkill(condition);

                    break;
                case "Item":
                    GiveItem(condition);

                    break;
                case "Cultivation stage":
                    GiveCultivation(condition);

                    break;
                case "NPC":

                    break;
                default:
                    break;
            }
        }

        QuestState = State.RewardObtained;
    }

    private void GiveSkill(QuestConditionModel condition)
    {
        switch (QuestConditionTypeModel.FindByID(condition.conditionID))
        {
            case "Learn skill":
                {
                    ActionSkillController action;
                    ResultSkillController result;
                    int skillID = int.Parse(condition.param1);
                    string rank = condition.param2;

                    // Learn if needed
                    action = new ActionSkillController(
                        Player.Instance,
                        this,
                        skillID,
                        ActionSkillController.ActionType.Learn
                    );
                    action.Handle();

                    Skill skill = Player.Instance.skillManager.Get(skillID);

                    while (skill.CanRankUp() && !skill.IsRankOrGreater(rank))
                    {
                        // Force it to rank up, despite xp requirement
                        skill.RankUp();

                        // Inform the quest and skill handlers of new updates...
                        action = new ActionSkillController(
                            Player.Instance,
                            this,
                            skillID,
                            ActionSkillController.ActionType.RankUp
                        );
                        result = new ResultSkillController(Player.Instance, this, skill, action);
                        result.Handle(true);
                    }

                    break;
                }
            case "Rank up skill":
                {
                    break;
                }
            default:
                break;
        }
    }

    private void GiveItem(QuestConditionModel condition)
    {
        switch (QuestConditionTypeModel.FindByID(condition.conditionID))
        {
            case "Get item":
                int itemID = int.Parse(condition.param1);
                int itemQuantity = int.Parse(condition.param2);

                ActionItemController action = new ActionItemController(
                    Player.Instance,
                    this,
                    itemID,
                    itemQuantity,
                    ActionItemController.ActionType.ItemAdd
                );
                action.Handle();

                break;
            default:
                break;
        }
    }

    private void GiveCultivation(QuestConditionModel condition)
    {
        if (int.Parse(condition.param1) > Player.Instance.actorStage.Value)
        {
            Player.Instance.actorStage.Value = int.Parse(condition.param1);
        }

        if (int.Parse(condition.param2) > Player.Instance.actorSubstage.Value)
        {
            Player.Instance.actorSubstage.Value = int.Parse(condition.param2);
        }
    }
}
