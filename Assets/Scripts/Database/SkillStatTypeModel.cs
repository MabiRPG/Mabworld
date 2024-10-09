public class SkillStatTypeModel : TypeModel<SkillStatTypeModel>
{
    public SkillStatTypeModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "skill_stat_type";
        CreateReadQuery();
        ReadRow();
    }
}
