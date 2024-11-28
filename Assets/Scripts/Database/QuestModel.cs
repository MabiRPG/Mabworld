using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

[Serializable]
public class QuestModel : Model
{
    public int ID;
    [NonSerialized]
    public string name;
    [NonSerialized]
    public int typeID;

    [NonSerialized]
    public List<QuestConditionModel> prerequisites = new List<QuestConditionModel>();
    [NonSerialized]
    public List<QuestConditionModel> steps = new List<QuestConditionModel>();
    [NonSerialized]
    public List<QuestConditionModel> rewards = new List<QuestConditionModel>();
    [NonSerialized]
    public List<QuestDialogueModel> dialogues = new List<QuestDialogueModel>();

    public static string prerequisitesTableName;
    public static string stepsTableName;
    public static string rewardsTableName;
    public static string dialogueTableName;

    public QuestModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "quest";
        prerequisitesTableName = "quest_prerequisite";
        stepsTableName = "quest_step";
        rewardsTableName = "quest_reward";
        dialogueTableName = "quest_dialogue";

        primaryKeys.Add("id");

        fieldMap.Add("id", new ModelFieldReference(this, nameof(ID)));
        fieldMap.Add("name", new ModelFieldReference(this, nameof(name)));
        fieldMap.Add("type_id", new ModelFieldReference(this, nameof(typeID)));

        CreateReadQuery();
        CreateWriteQuery();

        ReadRow();

        ReadInfo(prerequisitesTableName, prerequisites);
        ReadInfo(stepsTableName, steps);
        ReadInfo(rewardsTableName, rewards);
        ReadDialogue();
    }

    private void ReadInfo(string tableName, List<QuestConditionModel> appendList)
    {
        string query = @$"SELECT step_id
            FROM {tableName}
            WHERE quest_id = @id
            ORDER BY step_id;";

        DataTable table = database.ReadTable(query, fieldMap);

        foreach (DataRow row in table.Rows)
        {
            int stepID = int.Parse(row["step_id"].ToString());
            QuestConditionModel step = new QuestConditionModel(database, ID, stepID, tableName);
            appendList.Add(step);
        }
    }

    private void ReadDialogue()
    {
        string query = @$"SELECT id, conversation_id
            FROM {dialogueTableName}
            WHERE quest_id = @id;";

        DataTable table = database.ReadTable(query, fieldMap);

        foreach (DataRow row in table.Rows)
        {
            int dialogueID = int.Parse(row["id"].ToString());
            int conversationID = int.Parse(row["conversation_id"].ToString());
            QuestDialogueModel dialogue =
                new QuestDialogueModel(database, dialogueID, conversationID, ID);
            dialogues.Add(dialogue);
        }
    }
}