public class SkillStatTypeModel : TypeModel<SkillStatTypeModel>
{
    public SkillStatTypeModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;

        readString = $"SELECT * FROM skill_stat_type WHERE id=@id;";

        ReadRow();
    }
}
