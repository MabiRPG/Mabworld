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

    public void CheckPrereq<T>(T result) where T : ResultHandler
    {
        foreach (QuestConditionModel condition in prerequisites.ToList())
        {
            int categoryID = condition.conditionID;
            int conditionCategoryID = QuestConditionTypeModel.GetCategory(categoryID);

            switch (QuestConditionCategoryTypeModel.FindByID(conditionCategoryID))
            {
                case "Skill":
                {
                    HandleSkill(result as ResultSkillController, condition);
                    break;
                }
                default:
                    break;
            }
        }
    }

    private void HandleSkill(ResultSkillController result, QuestConditionModel condition)
    {
        Skill skill = result.skill;
        ActionSkillController.ActionType actionType = result.type;

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
}