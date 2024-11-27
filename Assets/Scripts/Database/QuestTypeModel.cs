public class QuestTypeModel : TypeModel<QuestTypeModel>
{
    public QuestTypeModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "quest_type";
        CreateReadQuery();
        ReadRow();
    }
}