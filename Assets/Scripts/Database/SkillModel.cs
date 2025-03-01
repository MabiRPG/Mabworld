using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class SkillModel : Model
{
    // Primary key of skill
    [JsonProperty]
    private int id;
    // Name of skill and category
    [JsonProperty]
    private string name;
    [JsonProperty]
    private int cultivationStageID;
    // Skill description, details, skill icon, and sound effect when using
    [JsonProperty]
    private string description;
    [JsonProperty]
    private string details;
    [JsonProperty]
    private string icon;
    [JsonProperty]
    private string sfx;
    [JsonProperty]
    private string animationClip;
    // Starting, first and last ranks that can be reached
    [JsonProperty]
    private string startingRank;
    [JsonProperty]
    private string firstAvailableRank;
    [JsonProperty]
    private string lastAvailableRank;
    // Base loading time, use time, and cooldown
    [JsonProperty]
    private float baseLoadTime;
    [JsonProperty]
    private float baseUseTime;
    [JsonProperty]
    private float baseCooldown;
    // Does player start with skill?
    [JsonProperty]
    private bool isStartingWith;
    // Learnable? and learn condition
    [JsonProperty]
    private bool isLearnable;
    [JsonProperty]
    private int learnConditionID;
    // Passive or active
    [JsonProperty]
    private bool isPassive;

    // All ranks in string format
    public static List<string> ranks = new List<string>
        {"F", "E", "D", "C", "B", "A", "9", "8", "7", "6", "5", "4", "3", "2", "1"};

    private string statTableName;
    private string trainingMethodTableName;

    [JsonProperty]
    public Dictionary<int, SkillStatModel> stats = new Dictionary<int, SkillStatModel>();

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

    public int ID { get => id; set => id = value; }
    public string Name { get => name; set => name = value; }
    public int CultivationStageID { get => cultivationStageID; set => cultivationStageID = value; }
    public string Description { get => description; set => description = value; }
    public string Details { get => details; set => details = value; }
    public Sprite Icon
    {
        get => GameManager.Instance.LoadAsset<Sprite>(icon);
#if UNITY_EDITOR
        set => icon = GameManager.Instance.Database.AddToAddressables(value);
#endif
    }
    public AudioClip Sfx
    {
        get => GameManager.Instance.LoadAsset<AudioClip>(sfx);
#if UNITY_EDITOR
        set => sfx = GameManager.Instance.Database.AddToAddressables(value);
#endif
    }
    public AnimationClip AnimationClip
    {
        get => GameManager.Instance.LoadAsset<AnimationClip>(animationClip);
#if UNITY_EDITOR
        set => animationClip = GameManager.Instance.Database.AddToAddressables(value);
#endif
    }
    public string StartingRank { get => startingRank; set => startingRank = value; }
    public string FirstAvailableRank { get => firstAvailableRank; set => firstAvailableRank = value; }
    public string LastAvailableRank { get => lastAvailableRank; set => lastAvailableRank = value; }
    public float BaseLoadTime { get => baseLoadTime; set => baseLoadTime = value; }
    public float BaseUseTime { get => baseUseTime; set => baseUseTime = value; }
    public float BaseCooldown { get => baseCooldown; set => baseCooldown = value; }
    public bool IsStartingWith { get => isStartingWith; set => isStartingWith = value; }
    public bool IsLearnable { get => isLearnable; set => isLearnable = value; }
    public int LearnConditionID { get => learnConditionID; set => learnConditionID = value; }
    public bool IsPassive { get => isPassive; set => isPassive = value; }

    [JsonConstructor]
    public SkillModel() : base(null) { }

    public SkillModel(DatabaseManager database, int ID) : base(database)
    {
        id = ID;
        tableName = "skill";
        statTableName = "skill_stat";
        trainingMethodTableName = "training_method";

        primaryKeys.Add("id");

        fieldMap.Add("id", new ModelFieldReference(this, nameof(id)));
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