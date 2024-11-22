using UnityEngine;

public class WindowQuest : Window
{
    public static WindowQuest Instance = null;

    private WindowQuestRowList windowQuestRowList;
    private WindowQuestDetailed windowQuestDetailed;
    private GameObject selectedQuestObj;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        windowQuestRowList = GetComponentInChildren<WindowQuestRowList>();
        windowQuestDetailed = GetComponentInChildren<WindowQuestDetailed>();

        selectedQuestObj = body.transform.Find("Selected Quest").gameObject;
        selectedQuestObj.SetActive(false);
    }

    public void SetQuestDetailed(Quest quest)
    {
        selectedQuestObj.SetActive(true);
        windowQuestDetailed.SetQuest(quest);
    }
}
