using System.Collections.Generic;
using System.Data;

public class QuestModel : Model
{
    public int ID;
    public string name;

    public List<QuestConditionModel> prerequisites = new List<QuestConditionModel>();
    public List<QuestConditionModel> steps = new List<QuestConditionModel>();
    public List<QuestConditionModel> rewards = new List<QuestConditionModel>();

    private string prerequisitesTableName;
    private string stepsTableName;
    private string rewardsTableName;

    public QuestModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "quest";
        prerequisitesTableName = "quest_prerequisite";
        stepsTableName = "quest_step";
        rewardsTableName = "quest_reward";

        primaryKeys.Add("id");

        fieldMap.Add("id", new ModelFieldReference(this, nameof(ID)));
        fieldMap.Add("name", new ModelFieldReference(this, nameof(name)));

        CreateReadQuery();
        CreateWriteQuery();

        ReadRow();

        ReadInfo(prerequisitesTableName, prerequisites);
        ReadInfo(stepsTableName, steps);
        ReadInfo(rewardsTableName, rewards);
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
}