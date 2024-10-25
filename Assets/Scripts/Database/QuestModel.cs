using System.Collections.Generic;
using System.Data;

public class QuestModel : Model
{
    public int ID;
    public string name;

    public List<QuestConditionModel> steps = new List<QuestConditionModel>();

    private string stepsTableName;

    public QuestModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "quest";
        stepsTableName = "quest_step";

        primaryKeys.Add("id");

        fieldMap.Add("id", new ModelFieldReference(this, nameof(ID)));
        fieldMap.Add("name", new ModelFieldReference(this, nameof(name)));

        CreateReadQuery();
        CreateWriteQuery();

        ReadRow();
        ReadSteps();
    }

    private void ReadSteps()
    {
        string stepQuery = @$"SELECT step_id
            FROM {stepsTableName}
            WHERE quest_id = @id
            ORDER BY step_id;";

        DataTable table = database.ReadTable(stepQuery, fieldMap);

        foreach (DataRow row in table.Rows)
        {
            int stepID = int.Parse(row["step_id"].ToString());
            QuestConditionModel step = new QuestConditionModel(database, ID, stepID);
            steps.Add(step);
        }            
    }
}