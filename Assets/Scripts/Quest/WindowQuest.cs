public class WindowQuest : Window
{
    public static WindowQuest Instance = null;

    private WindowQuestRowList windowQuestRowList;
    private WindowQuestDetailed windowQuestDetailed;

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
    }

    public void SetQuestDetailed(Quest quest)
    {
        windowQuestDetailed.SetQuest(quest);
    }
}