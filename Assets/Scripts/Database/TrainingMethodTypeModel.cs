public class TrainingMethodTypeModel : TypeModel<TrainingMethodTypeModel>
{
    public TrainingMethodTypeModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;

        fieldMap.Add("id", new ModelFieldReference(this, nameof(ID)));
        fieldMap.Add("name", new ModelFieldReference(this, nameof(name)));

        readString = $"SELECT * FROM training_method_type WHERE id=@id;";

        ReadRow();
    }
}