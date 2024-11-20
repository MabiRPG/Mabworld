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
            GameObject obj = questConditionPrefabFactory.GetFree(condition, transform.Find("Viewport/Content"));
            WindowQuestCondition script = obj.GetComponent<WindowQuestCondition>();
            script.SetCondition(condition);
        }
    }
}