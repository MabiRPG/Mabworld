public class CultivationStageModel : TypeModel<CultivationStageModel>
{
    public CultivationStageModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "cultivation_stage";
        CreateReadQuery();
        ReadRow();
    }
}