using Newtonsoft.Json;

[JsonObject]
public class TrainingMethodModel : Model
{
    public int skillID;
    public string rank;
    public int trainingMethodID;
    public float xpGainEach;
    public int countMax;
    public string name;
    public string param1 = "1";
    public string param2 = "1";

    public TrainingMethodModel(DatabaseManager database, int skillID) : base(database)
    {
        this.skillID = skillID;
        tableName = "training_method";

        primaryKeys.Add("skill_id");
        primaryKeys.Add("training_method_id");
        primaryKeys.Add("rank");

        fieldMap.Add("skill_id", new ModelFieldReference(this, nameof(this.skillID)));
        fieldMap.Add("rank", new ModelFieldReference(this, nameof(rank)));
        fieldMap.Add("training_method_id", new ModelFieldReference(this, nameof(trainingMethodID)));
        fieldMap.Add("xp_gain_each", new ModelFieldReference(this, nameof(xpGainEach)));
        fieldMap.Add("count_max", new ModelFieldReference(this, nameof(countMax)));
        fieldMap.Add("name", new ModelFieldReference(this, nameof(name)));
        fieldMap.Add("param1", new ModelFieldReference(this, nameof(param1)));
        fieldMap.Add("param2", new ModelFieldReference(this, nameof(param2)));

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
        fieldMap.Add("name", new ModelFieldReference(this, nameof(name)));
        fieldMap.Add("param1", new ModelFieldReference(this, nameof(param1)));
        fieldMap.Add("param2", new ModelFieldReference(this, nameof(param2)));

        CreateReadQuery();
        CreateWriteQuery();

        ReadRow();
    }
}