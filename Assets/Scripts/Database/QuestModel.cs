using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using UnityEngine;

public class QuestModel : Model
{
    public int ID;
    public string name;
    public int typeID = 1;

    public List<QuestConditionModel> prerequisites;
    public List<QuestConditionModel> steps;
    public List<QuestConditionModel> rewards;
    public List<QuestDialogueModel> dialogues;

    public static string prerequisitesTableName;
    public static string stepsTableName;
    public static string rewardsTableName;
    public static string dialogueTableName;

    [JsonConstructor]
    public QuestModel() : base(null) { }

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

        prerequisites = new List<QuestConditionModel>();
        steps = new List<QuestConditionModel>();
        rewards = new List<QuestConditionModel>();
        dialogues = new List<QuestDialogueModel>();

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
        string query = @$"SELECT quest_id, conversation_id, id 
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