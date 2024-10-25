public class QuestConditionModel : Model
{
    public int questID;
    public int stepID;
    public int conditionID;
    public int param1;

    public QuestConditionModel(DatabaseManager database, int questID, int stepID) : base(database)
    {
        this.questID = questID;
        this.stepID = stepID;
        tableName = "quest_step";

        primaryKeys.Add("questID");
        primaryKeys.Add("stepID");

        fieldMap.Add("quest_id", new ModelFieldReference(this, nameof(this.questID)));
        fieldMap.Add("step_id", new ModelFieldReference(this, nameof(this.stepID)));
        fieldMap.Add("condition_id", new ModelFieldReference(this, nameof(conditionID)));
        fieldMap.Add("param1", new ModelFieldReference(this, nameof(param1)));

        CreateReadQuery();
        CreateWriteQuery();

        ReadRow();
    }
}