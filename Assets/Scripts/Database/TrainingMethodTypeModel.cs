public class TrainingMethodTypeModel : TypeModel<TrainingMethodTypeModel>
{
    public TrainingMethodTypeModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "training_method_type";
        CreateReadQuery();
        ReadRow();
    }
}