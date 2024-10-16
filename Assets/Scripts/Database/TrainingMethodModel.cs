public class TrainingMethodModel : Model
{
    public int skillID;
    public string rank;
    public int trainingMethodID;
    public float xpGainEach;
    public int countMax;

    public TrainingMethodModel(DatabaseManager database, int skillID) : base(database)
    {
        this.skillID = skillID;
        tableName = "training_method";

        primaryKeys.Add("skill_id");
        primaryKeys.Add("training_method_id");
        primaryKeys.Add("rank");

        fieldMap.Add("skill_id", new ModelFieldReference(this, nameof(skillID)));
        fieldMap.Add("rank", new ModelFieldReference(this, nameof(rank)));
        fieldMap.Add("training_method_id", new ModelFieldReference(this, nameof(trainingMethodID)));
        fieldMap.Add("xp_gain_each", new ModelFieldReference(this, nameof(xpGainEach)));
        fieldMap.Add("count_max", new ModelFieldReference(this, nameof(countMax)));

        CreateReadQuery();
        CreateWriteQuery();
    }

    public TrainingMethodModel(DatabaseManager database, int skillID, int trainingMethodID, string rank) 
        : base(database)
    {
        this.skillID = skillID;
        this.trainingMethodID = trainingMethodID;
        this.rank = rank;
        tableName = "training_method";

        primaryKeys.Add("skill_id");
        primaryKeys.Add("training_method_id");
        primaryKeys.Add("rank");

        fieldMap.Add("skill_id", new ModelFieldReference(this, nameof(skillID)));
        fieldMap.Add("rank", new ModelFieldReference(this, nameof(rank)));
        fieldMap.Add("training_method_id", new ModelFieldReference(this, nameof(trainingMethodID)));
        fieldMap.Add("xp_gain_each", new ModelFieldReference(this, nameof(xpGainEach)));
        fieldMap.Add("count_max", new ModelFieldReference(this, nameof(countMax)));

        CreateReadQuery();
        CreateWriteQuery();

        ReadRow();
    }
}