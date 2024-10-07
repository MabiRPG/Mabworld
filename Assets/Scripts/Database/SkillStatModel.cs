using System.Collections.Generic;
using System.Data;
using System.Linq;

public class SkillStatModel : BaseModel
{
    public int skillID;
    public int statID;
    public List<float> values = new List<float>(SkillModel.ranks.Count);

    public SkillStatModel(DatabaseManager database, int skillID, int statID) : base(database)
    {
        this.skillID = skillID;
        this.statID = statID;

        fieldMap.Add("skill_id", new ModelFieldReference(this, nameof(skillID)));
        fieldMap.Add("skill_stat_id", new ModelFieldReference(this, nameof(statID)));

        readString = @"SELECT skill_stat.*, skill_stat_type.name
            FROM skill
            JOIN skill_stat
            ON skill.id = skill_stat.skill_id
            JOIN skill_stat_type
            ON skill_stat.skill_stat_id = skill_stat_type.id
            WHERE skill.id = @skill_id AND skill_stat_id = @skill_stat_id";

        DataRow row = ReadRow();

        // Slice the row by length of ranks
        // converting to string then float and back to array for the value.
        values.AddRange(
            row.ItemArray
                .Skip(2)
                .Take(SkillModel.ranks.Count)
                .Select(x => float.Parse(x.ToString()))
                .ToArray());        
    }
}