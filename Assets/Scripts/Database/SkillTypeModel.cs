public class SkillTypeModel : TypeModel<SkillTypeModel>
{
    public SkillTypeModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "skill_category_type";
        CreateReadQuery();
        ReadRow();
    }
}