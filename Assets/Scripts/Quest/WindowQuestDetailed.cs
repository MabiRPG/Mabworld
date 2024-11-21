using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowQuestDetailed : MonoBehaviour
{
    private Quest quest;

    [SerializeField]
    private GameObject questConditionPrefab;
    private PrefabFactory questConditionPrefabFactory;

    [SerializeField]
    private TMP_Text qName;

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
        if (this.quest != null)
        {
            this.quest.OnStateChange -= Draw;
        }

        this.quest = quest;
        quest.OnStateChange += Draw;
        Draw();
    }

    private void Draw()
    {
        qName.text = quest.name;

        if (quest.QuestState == Quest.State.NeedPrerequisite)
        {
            prerequisiteParent.SetActive(true);

            foreach (QuestCondition condition in quest.prerequisiteStates)
            {
                int categoryID = condition.model.conditionID;
                int conditionCategoryID = QuestConditionTypeModel.GetCategory(categoryID);

                if (QuestConditionCategoryTypeModel.FindByID(conditionCategoryID) == "Dialogue")
                {
                    continue;
                }

                GameObject obj = questConditionPrefabFactory.GetFree(
                    condition,
                    prerequisiteParent.transform
                );
                WindowQuestCondition script = obj.GetComponent<WindowQuestCondition>();
                script.SetCondition(condition);
            }
        }
        else
        {
            prerequisiteParent.SetActive(false);
        }

        if (quest.QuestState == Quest.State.InProgress)
        {
            stepParent.SetActive(true);

            foreach (QuestCondition condition in quest.stepStates)
            {
                int categoryID = condition.model.conditionID;
                int conditionCategoryID = QuestConditionTypeModel.GetCategory(categoryID);

                if (QuestConditionCategoryTypeModel.FindByID(conditionCategoryID) == "Dialogue")
                {
                    continue;
                }

                GameObject obj = questConditionPrefabFactory.GetFree(
                    condition,
                    stepParent.transform
                );
                WindowQuestCondition script = obj.GetComponent<WindowQuestCondition>();
                script.SetCondition(condition);
            }
        }
        else
        {
            stepParent.SetActive(false);
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
