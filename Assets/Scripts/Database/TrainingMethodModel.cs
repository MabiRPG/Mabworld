public class TrainingMethodModel : BaseModel
{
    public int skillID;
    public string rank;
    public int trainingMethodID;
    public float xpGainEach;
    public int countMax;

    public TrainingMethodModel(DatabaseManager database, int skillID, int trainingMethodID) 
        : base(database)
    {
        this.skillID = skillID;
        this.trainingMethodID = trainingMethodID;

        fieldMap.Add("skill_id", new ModelFieldReference(this, nameof(skillID)));
        fieldMap.Add("rank", new ModelFieldReference(this, nameof(rank)));
        fieldMap.Add("training_method_id", new ModelFieldReference(this, nameof(trainingMethodID)));
        fieldMap.Add("xp_gain_each", new ModelFieldReference(this, nameof(xpGainEach)));
        fieldMap.Add("count_max", new ModelFieldReference(this, nameof(countMax)));

        readString = @"SELECT * FROM training_method WHERE
            skill_id = @skill_id AND training_method_id = @training_method_id;";

        ReadRow();
    }
}