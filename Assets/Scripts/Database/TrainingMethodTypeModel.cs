public class TrainingMethodTypeModel : TypeModel<TrainingMethodTypeModel>
{
    public TrainingMethodTypeModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;

        readString = $"SELECT * FROM training_method_type WHERE id=@id;";

        ReadRow();
    }
}