using UnityEngine;

public class WindowQuestRowList : MonoBehaviour
{
    [SerializeField]
    private GameObject questRowPrefab;
    private PrefabFactory questRowPrefabFactory;

    [SerializeField]
    private GameObject mainQuestParent;
    [SerializeField]
    private GameObject sideQuestParent;

    private void Awake()
    {
        questRowPrefabFactory = ScriptableObject.CreateInstance<PrefabFactory>();
        questRowPrefabFactory.SetPrefab(questRowPrefab);
    }

    private void OnEnable()
    {
        questRowPrefabFactory.SetActiveAll(false);

        foreach (Quest quest in Player.Instance.quests.Values)
        {
            GameObject obj = questRowPrefabFactory.GetFree(quest, transform.Find("Viewport/Content"));
            WindowQuestRow row = obj.GetComponent<WindowQuestRow>();
            row.SetQuest(quest);
        }
    }
}