using UnityEngine;

public class WindowQuestDetailed : MonoBehaviour
{
    [SerializeField]
    private GameObject questConditionPrefab;
    private PrefabFactory questConditionPrefabFactory;

    private void Awake()
    {
        questConditionPrefabFactory = ScriptableObject.CreateInstance<PrefabFactory>();
        questConditionPrefabFactory.SetPrefab(questConditionPrefab);
    }

    public void SetQuest(Quest quest)
    {
        foreach (QuestCondition condition in quest.prerequisiteStates)
        {
            int categoryID = condition.model.conditionID;
            int conditionCategoryID = QuestConditionTypeModel.GetCategory(categoryID);

            if (QuestConditionCategoryTypeModel.FindByID(conditionCategoryID) == "Dialogue")
            {
                continue;
            }

            GameObject obj = questConditionPrefabFactory.GetFree(condition, transform.Find("Viewport/Content"));
            WindowQuestCondition script = obj.GetComponent<WindowQuestCondition>();
            script.SetCondition(condition);
        }

        foreach (QuestCondition condition in quest.stepStates)
        {
            int categoryID = condition.model.conditionID;
            int conditionCategoryID = QuestConditionTypeModel.GetCategory(categoryID);

            if (QuestConditionCategoryTypeModel.FindByID(conditionCategoryID) == "Dialogue")
            {
                continue;
            }

            GameObject obj = questConditionPrefabFactory.GetFree(condition, transform.Find("Viewport/Content"));
            WindowQuestCondition script = obj.GetComponent<WindowQuestCondition>();
            script.SetCondition(condition);
        }
    }
}