using System;
using System.Collections.Generic;
using System.Data;

// public enum QuestState
// {
//     Hidden,
//     Available,
//     InProgress,
//     Completed
// }

public abstract class QuestRequirement
{
    public QuestRequirement()
    {
        SetListeners();
    }

    public abstract void SetListeners();
    public abstract bool IsCompleted();
}

public abstract class QuestTask
{
    public QuestTask()
    {
        SetListeners();
    }

    public abstract void SetListeners();
    public abstract bool IsCompleted();
}

public class QuestState
{
    public List<QuestTask> tasks = new List<QuestTask>();

    public bool IsCompleted()
    {
        foreach (QuestTask task in tasks)
        {
            if (!task.IsCompleted())
            {
                return false;
            }
        }

        return true;
    }
}

public abstract class QuestReward
{
    public abstract void GiveReward();
}

public class Quest
{
    public int ID;
    public string name;
    public int categoryID;
    public int stateID = -1;

    private const string questQuery = @"SELECT * FROM quest WHERE id = @id LIMIT 1;";

    public Quest(int ID)
    {
        this.ID = ID;
        LoadQuestInfo();
    }

    private void LoadQuestInfo()
    {
        DataTable dt = GameManager.Instance.QueryDatabase(questQuery, ("@id", ID));
        DataRow row = dt.Rows[0];
        GameManager.Instance.ParseDatabaseRow(row, this, ("category_id", "categoryID"));
        stateID = 0;

        UI_DialogueBox.Instance.SetDialogue(ID);
    }

    public void Advance(int newStateID)
    {
        if (newStateID != stateID + 1)
        {
            return;
        }

        stateID++;
        UI_DialogueBox.Instance.SetDialogue(ID);
    }
}