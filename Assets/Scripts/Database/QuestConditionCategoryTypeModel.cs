public class QuestConditionCategoryTypeModel : TypeModel<QuestConditionCategoryTypeModel>
{
    public QuestConditionCategoryTypeModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "quest_condition_category_type";
        
        CreateReadQuery();

        ReadRow();
    }
}