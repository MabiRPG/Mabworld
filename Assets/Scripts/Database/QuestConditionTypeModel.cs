public class QuestConditionTypeModel : TypeModel<QuestConditionTypeModel>
{
    public int categoryID;

    public QuestConditionTypeModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "quest_condition_type";

        fieldMap.Add("category_id", new ModelFieldReference(this, nameof(categoryID)));

        CreateReadQuery();

        ReadRow();
    }
}