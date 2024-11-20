using UnityEngine;
using UnityEngine.UI;

public class WindowQuestDetailed : MonoBehaviour
{
    [SerializeField]
    private GameObject questConditionPrefab;
    private PrefabFactory questConditionPrefabFactory;

    [SerializeField]
    private GameObject prerequisiteParent;
    [SerializeField]
    private GameObject stepParent;
    [SerializeField]
    private GameObject rewardParent;

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

            GameObject obj = questConditionPrefabFactory.GetFree(condition, prerequisiteParent.transform);
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

            GameObject obj = questConditionPrefabFactory.GetFree(condition, stepParent.transform);
            WindowQuestCondition script = obj.GetComponent<WindowQuestCondition>();
            script.SetCondition(condition);
        }

        foreach (QuestCondition condition in quest.rewardStates)
        {
            GameObject obj = questConditionPrefabFactory.GetFree(condition, rewardParent.transform);
            WindowQuestCondition script = obj.GetComponent<WindowQuestCondition>();
            script.SetCondition(condition);            
        }

        // Resets the content size fitter.
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }
}