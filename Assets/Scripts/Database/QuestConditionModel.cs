public class QuestConditionModel : Model
{
    public int questID;
    public int stepID;
    public int conditionID;
    public string param1 = "1";
    public string param2 = "1";

    public QuestConditionModel(DatabaseManager database, int questID, string tableName) 
        : base(database)
    {
        this.questID = questID;
        this.tableName = tableName;

        primaryKeys.Add("quest_id");
        primaryKeys.Add("step_id");

        fieldMap.Add("quest_id", new ModelFieldReference(this, nameof(this.questID)));
        fieldMap.Add("step_id", new ModelFieldReference(this, nameof(this.stepID)));
        fieldMap.Add("condition_id", new ModelFieldReference(this, nameof(conditionID)));
        fieldMap.Add("param1", new ModelFieldReference(this, nameof(param1)));
        fieldMap.Add("param2", new ModelFieldReference(this, nameof(param2)));

        CreateReadQuery();
        CreateWriteQuery();
    }

    public QuestConditionModel(DatabaseManager database, int questID, int stepID, 
        string tableName) : base(database)
    {
        this.questID = questID;
        this.stepID = stepID;
        this.tableName = tableName;

        primaryKeys.Add("quest_id");
        primaryKeys.Add("step_id");

        fieldMap.Add("quest_id", new ModelFieldReference(this, nameof(this.questID)));
        fieldMap.Add("step_id", new ModelFieldReference(this, nameof(this.stepID)));
        fieldMap.Add("condition_id", new ModelFieldReference(this, nameof(conditionID)));
        fieldMap.Add("param1", new ModelFieldReference(this, nameof(param1)));
        fieldMap.Add("param2", new ModelFieldReference(this, nameof(param2)));

        CreateReadQuery();
        CreateWriteQuery();

        ReadRow();
    }
}