public class SkillStatTypeModel : TypeModel<SkillStatTypeModel>
{
    public SkillStatTypeModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;

        fieldMap.Add("id", new ModelFieldReference(this, nameof(ID)));
        fieldMap.Add("name", new ModelFieldReference(this, nameof(name)));

        readString = $"SELECT * FROM skill_stat_type WHERE id=@id;";

        ReadRow();
    }
}
