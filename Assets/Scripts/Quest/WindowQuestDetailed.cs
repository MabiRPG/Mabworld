using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowQuestDetailed : MonoBehaviour
{
    private Quest quest;

    [SerializeField]
    private GameObject questConditionPrefab;
    private PrefabFactory questPrerequisitePrefabFactory;
    private PrefabFactory questStepPrefabFactory;
    private PrefabFactory questRewardPrefabFactory;

    [SerializeField]
    private TMP_Text qName;

    [SerializeField]
    private GameObject prerequisiteParent;

    [SerializeField]
    private GameObject stepParent;

    [SerializeField]
    private GameObject rewardParent;

    private Button claimRewardButton;

    private void Awake()
    {
        questPrerequisitePrefabFactory = ScriptableObject.CreateInstance<PrefabFactory>();
        questPrerequisitePrefabFactory.SetPrefab(questConditionPrefab);

        questStepPrefabFactory = ScriptableObject.CreateInstance<PrefabFactory>();
        questStepPrefabFactory.SetPrefab(questConditionPrefab);

        questRewardPrefabFactory = ScriptableObject.CreateInstance<PrefabFactory>();
        questRewardPrefabFactory.SetPrefab(questConditionPrefab);

        claimRewardButton = transform
            .parent.Find("Interact Parent/Claim Reward Button")
            .GetComponent<Button>();
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

        claimRewardButton.onClick.AddListener(ClaimReward);
    }

    private void Draw()
    {
        questPrerequisitePrefabFactory.SetActiveAll(false);
        questStepPrefabFactory.SetActiveAll(false);
        questRewardPrefabFactory.SetActiveAll(false);

        qName.text = quest.name;

        // if (quest.QuestState == Quest.State.NeedPrerequisite)
        // {
        prerequisiteParent.SetActive(true);

        foreach (QuestCondition condition in quest.prerequisiteStates)
        {
            int categoryID = condition.model.conditionID;
            int conditionCategoryID = QuestConditionTypeModel.GetCategory(categoryID);

            if (QuestConditionCategoryTypeModel.FindByID(conditionCategoryID) == "Dialogue")
            {
                continue;
            }

            GameObject obj = questPrerequisitePrefabFactory.GetFree(
                condition,
                prerequisiteParent.transform
            );
            WindowQuestCondition script = obj.GetComponent<WindowQuestCondition>();
            script.SetCondition(condition);
        }
        // }
        // else
        // {
        //     prerequisiteParent.SetActive(false);
        // }

        // if (quest.QuestState == Quest.State.InProgress)
        // {
        stepParent.SetActive(true);

        foreach (QuestCondition condition in quest.stepStates)
        {
            int categoryID = condition.model.conditionID;
            int conditionCategoryID = QuestConditionTypeModel.GetCategory(categoryID);

            if (QuestConditionCategoryTypeModel.FindByID(conditionCategoryID) == "Dialogue")
            {
                continue;
            }

            GameObject obj = questStepPrefabFactory.GetFree(
                condition,
                stepParent.transform
            );
            WindowQuestCondition script = obj.GetComponent<WindowQuestCondition>();
            script.SetCondition(condition);
        }
        // }
        // else
        // {
        //     stepParent.SetActive(false);
        // }

        foreach (QuestCondition condition in quest.rewardStates)
        {
            GameObject obj = questRewardPrefabFactory.GetFree(condition, rewardParent.transform);
            WindowQuestCondition script = obj.GetComponent<WindowQuestCondition>();
            script.SetCondition(condition);
        }

        // Resets the content size fitter.
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);

        if (quest.QuestState == Quest.State.Complete)
        {
            claimRewardButton.gameObject.SetActive(true);
        }
        else
        {
            claimRewardButton.gameObject.SetActive(false);
        }
    }

    private void ClaimReward()
    {
        if (quest == null)
        {
            return;
        }

        ActionQuestController action = new ActionQuestController(Player.Instance,
            this, quest);
        action.OnSuccess += () =>
        {
            claimRewardButton.gameObject.SetActive(false);
            quest.OnStateChange -= Draw;
        };
        action.Handle();
    }
}
