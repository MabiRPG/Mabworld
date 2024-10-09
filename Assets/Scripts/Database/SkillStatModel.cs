using System.Collections.Generic;
using System.Data;
using System.Linq;

public class SkillStatModel : BaseModel
{
    public int skillID;
    public int statID;
    public List<float> values = new List<float>(SkillModel.ranks.Count);

    public SkillStatModel(DatabaseManager database, int skillID) : base(database)
    {
        this.skillID = skillID;
        
        for (int i = 0; i < values.Capacity; i++)
        {
            values.Add(0);
        }
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

        // readString = @"SELECT * FROM skill_stat 
        //     WHERE skill_id = @skill_id AND skill_stat_id = @skill_stat_id;";

        DataRow row = ReadRow();

        // Slice the row by length of ranks
        // converting to string then float and back to array for the value.
        // values.AddRange(
        //     row.ItemArray
        //         .Skip(2)
        //         .Take(SkillModel.ranks.Count)
        //         .Select(x => float.Parse(x.ToString()))
        //         .ToArray());
    }
}