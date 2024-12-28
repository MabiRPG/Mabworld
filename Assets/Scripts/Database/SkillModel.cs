using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject]
public class SkillModel : Model
{
    // Primary key of skill
    public int ID;
    // Name of skill and category
    public string name;
    public int cultivationStageID;
    // Skill description, details, skill icon, and sound effect when using
    public string description;
    public string details;
    [JsonIgnore]
    public Sprite icon;
    [JsonIgnore]
    public AudioClip sfx;
    [JsonIgnore]
    public AnimationClip animationClip;
    // Starting, first and last ranks that can be reached
    public string startingRank;
    public string firstAvailableRank;
    public string lastAvailableRank;
    // Base loading time, use time, and cooldown
    public float baseLoadTime;
    public float baseUseTime;
    public float baseCooldown;
    // Does player start with skill?
    public bool isStartingWith;
    // Learnable? and learn condition
    public bool isLearnable;
    public int learnConditionID;
    // Passive or active
    public bool isPassive;

    // Serialization info
    [JsonProperty]
    private string _iconName;
    [JsonProperty]
    private string _sfxName;
    [JsonProperty]
    private string _animationClipName;

    // All ranks in string format
    [JsonIgnore]
    public static List<string> ranks = new List<string>
        {"F", "E", "D", "C", "B", "A", "9", "8", "7", "6", "5", "4", "3", "2", "1"};

    private string statTableName;
    private string trainingMethodTableName;

    public Dictionary<int, SkillStatModel> stats = new Dictionary<int, SkillStatModel>();
    [JsonIgnore]
    public Dictionary<(int, string, string, string), TrainingMethodModel> trainingMethods =
        new Dictionary<(int, string, string, string), TrainingMethodModel>();

    [JsonProperty]
    private List<int> _methodID;
    [JsonProperty]
    private List<string> _rank;
    [JsonProperty]
    private List<string> _param1;
    [JsonProperty]
    private List<string> _param2;
    [JsonProperty]
    private List<TrainingMethodModel> _methods;

    [JsonConstructor]
    public SkillModel() : base(null) { }

    public SkillModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "skill";
        statTableName = "skill_stat";
        trainingMethodTableName = "training_method";

        primaryKeys.Add("id");

        fieldMap.Add("id", new ModelFieldReference(this, nameof(this.ID)));
        fieldMap.Add("name", new ModelFieldReference(this, nameof(name)));
        fieldMap.Add("cultivation_stage_id", new ModelFieldReference(this, nameof(cultivationStageID)));
        fieldMap.Add("description", new ModelFieldReference(this, nameof(description)));
        fieldMap.Add("details", new ModelFieldReference(this, nameof(details)));
        fieldMap.Add("icon", new ModelFieldReference(this, nameof(icon)));
        fieldMap.Add("sfx", new ModelFieldReference(this, nameof(sfx)));
        fieldMap.Add("starting_rank", new ModelFieldReference(this, nameof(startingRank)));
        fieldMap.Add("first_available_rank", new ModelFieldReference(this, nameof(firstAvailableRank)));
        fieldMap.Add("last_available_rank", new ModelFieldReference(this, nameof(lastAvailableRank)));
        fieldMap.Add("base_load_time", new ModelFieldReference(this, nameof(baseLoadTime)));
        fieldMap.Add("base_use_time", new ModelFieldReference(this, nameof(baseUseTime)));
        fieldMap.Add("base_cooldown", new ModelFieldReference(this, nameof(baseCooldown)));
        fieldMap.Add("is_starting_with", new ModelFieldReference(this, nameof(isStartingWith)));
        fieldMap.Add("is_learnable", new ModelFieldReference(this, nameof(isLearnable)));
        fieldMap.Add("is_passive", new ModelFieldReference(this, nameof(isPassive)));

        CreateReadQuery();
        CreateWriteQuery();

        ReadRow();
        ReadStats();
        ReadTrainingMethods();
    }

    private void ReadStats()
    {
        string statQuery = @$"SELECT skill_stat_id
            FROM {statTableName}
            WHERE skill_id = @id;";

        DataTable table = database.ReadTable(statQuery, fieldMap);

        foreach (DataRow row in table.Rows)
        {
            int statID = int.Parse(row["skill_stat_id"].ToString());
            SkillStatModel stat = new SkillStatModel(database, ID, statID);
            stats.Add(statID, stat);
        }
    }

    private void ReadTrainingMethods()
    {
        string trainingQuery = @$"SELECT training_method_id, rank, param1, param2
            FROM {trainingMethodTableName}
            WHERE skill_id = @id;";

        DataTable table = database.ReadTable(trainingQuery, fieldMap);

        foreach (DataRow row in table.Rows)
        {
            int methodID = int.Parse(row["training_method_id"].ToString());
            string rank = row["rank"].ToString();
            string param1 = row["param1"].ToString();
            string param2 = row["param2"].ToString();

            TrainingMethodModel method = new TrainingMethodModel(database, ID,
                methodID, rank, param1, param2);

            trainingMethods.Add((methodID, rank, param1, param2), method);
        }
    }

    [OnSerializing]
    internal void OnSerializing(StreamingContext context)
    {
        // Get the addressable names for save. Does not actually add to addressable
        _iconName = GameManager.Instance.Database.AddToAddressables(icon);
        _sfxName = GameManager.Instance.Database.AddToAddressables(sfx);
        _animationClipName = GameManager.Instance.Database.AddToAddressables(animationClip);

        _methodID = new List<int>();
        _rank = new List<string>();
        _param1 = new List<string>();
        _param2 = new List<string>();
        _methods = new List<TrainingMethodModel>();

        foreach ((int methodID, string rank, string param1, string param2) in trainingMethods.Keys)
        {
            _methodID.Add(methodID);
            _rank.Add(rank);
            _param1.Add(param1);
            _param2.Add(param2);
            _methods.Add(trainingMethods[(methodID, rank, param1, param2)]);
        }
    }

    [OnDeserialized]
    internal void OnDeserialized(StreamingContext context)
    {
        icon = GameManager.Instance.Database.LoadAsset<Sprite>(_iconName);
        sfx = GameManager.Instance.Database.LoadAsset<AudioClip>(_sfxName);
        animationClip = GameManager.Instance.Database.LoadAsset<AnimationClip>(_animationClipName);

        for (int i = 0; i < _methodID.Count; i++)
        {
            int methodID = _methodID[i];
            string rank = _rank[i];
            string param1 = _param1[i];
            string param2 = _param2[i];
            TrainingMethodModel method = _methods[i];

            trainingMethods.Add((methodID, rank, param1, param2), method);
        }

        _methodID.Clear();
        _rank.Clear();
        _methods.Clear();
    }
}