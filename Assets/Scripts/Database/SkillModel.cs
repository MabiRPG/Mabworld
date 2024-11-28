using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

[Serializable]
public class SkillModel : Model
{
    // Primary key of skill
    public int ID;
    [NonSerialized]
    // Name of skill and category
    public string name;
    [NonSerialized]
    public int cultivationStageID;
    [NonSerialized]
    // Skill description, details, skill icon, and sound effect when using
    public string description;
    [NonSerialized]
    public string details;
    [NonSerialized]
    public Sprite icon;
    [NonSerialized]
    public AudioClip sfx;
    [NonSerialized]
    public AnimationClip animationClip;
    [NonSerialized]
    // Starting, first and last ranks that can be reached
    public string startingRank;
    [NonSerialized]
    public string firstAvailableRank;
    [NonSerialized]
    public string lastAvailableRank;
    [NonSerialized]
    // Base loading time, use time, and cooldown
    public float baseLoadTime;
    [NonSerialized]
    public float baseUseTime;
    [NonSerialized]
    public float baseCooldown;
    [NonSerialized]
    // Does player start with skill?
    public bool isStartingWith;
    [NonSerialized]
    // Learnable? and learn condition
    public bool isLearnable;
    [NonSerialized]
    public int learnConditionID;
    [NonSerialized]
    // Passive or active
    public bool isPassive;

    // All ranks in string format
    public static List<string> ranks = new List<string>
        {"F", "E", "D", "C", "B", "A", "9", "8", "7", "6", "5", "4", "3", "2", "1"};

    private string statTableName;
    private string trainingMethodTableName;

    public Dictionary<int, SkillStatModel> stats = new Dictionary<int, SkillStatModel>();
    public Dictionary<(int, string), TrainingMethodModel> trainingMethods =
        new Dictionary<(int, string), TrainingMethodModel>();

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
        string trainingQuery = @$"SELECT training_method_id, rank
            FROM {trainingMethodTableName}
            WHERE skill_id = @id;";

        DataTable table = database.ReadTable(trainingQuery, fieldMap);

        foreach (DataRow row in table.Rows)
        {
            int methodID = int.Parse(row["training_method_id"].ToString());
            string rank = row["rank"].ToString();
            TrainingMethodModel method = new TrainingMethodModel(database, ID, methodID, rank);
            trainingMethods.Add((methodID, method.rank), method);
        }
    }
}