using System.Collections.Generic;

public class SkillStatModel : Model
{
    public int skillID;
    public int statID;
    public List<float> values = new List<float>(SkillModel.ranks.Count);

    public SkillStatModel(DatabaseManager database, int skillID) : base(database)
    {
        this.skillID = skillID;
        tableName = "skill_stat";

        primaryKeys.Add("skill_id");
        primaryKeys.Add("skill_stat_id");

        fieldMap.Add("skill_id", new ModelFieldReference(this, nameof(this.skillID)));
        fieldMap.Add("skill_stat_id", new ModelFieldReference(this, nameof(statID)));

        for (int i = 1; i <= values.Capacity; i++)
        {
            string hex = i.ToString("X");
            int valueIndex = values.Capacity - i;

            fieldMap.Add("r" + hex, new ModelFieldReference(this, nameof(values), valueIndex));
            values.Add(0);
        }

        CreateReadQuery();
        CreateWriteQuery();
    }

    public SkillStatModel(DatabaseManager database, int skillID, int statID) : base(database)
    {
        this.skillID = skillID;
        this.statID = statID;
        tableName = "skill_stat";

        primaryKeys.Add("skill_id");
        primaryKeys.Add("skill_stat_id");

        fieldMap.Add("skill_id", new ModelFieldReference(this, nameof(skillID)));
        fieldMap.Add("skill_stat_id", new ModelFieldReference(this, nameof(statID)));

        for (int i = 1; i <= values.Capacity; i++)
        {
            string hex = i.ToString("X");
            int valueIndex = values.Capacity - i;

            fieldMap.Add("r" + hex, new ModelFieldReference(this, nameof(values), valueIndex));
            values.Add(0);
        }

        CreateReadQuery();
        CreateWriteQuery();

        ReadRow();
    }
}