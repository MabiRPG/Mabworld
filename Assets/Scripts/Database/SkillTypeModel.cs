public class SkillTypeModel : TypeModel<SkillTypeModel>
{
    public SkillTypeModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;

        readString = $"SELECT * FROM skill_category_type WHERE id=@id;";

        ReadRow();
    }
}