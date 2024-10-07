public class SkillTypeModel : TypeModel<SkillTypeModel>
{
    public SkillTypeModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;

        fieldMap.Add("id", new ModelFieldReference(this, nameof(ID)));
        fieldMap.Add("name", new ModelFieldReference(this, nameof(name)));

        readString = $"SELECT * FROM skill_category_type WHERE id=@id;";

        ReadRow();
    }
}