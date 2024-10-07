using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class SkillModel : BaseModel
{
    // Primary key of skill
    public int ID;
    // Name of skill and category
    public string name;
    public int categoryID;
    // Skill description, details, skill icon, and sound effect when using
    public string description;
    public string details;
    public Sprite icon;
    public AudioClip sfx;
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

    // All ranks in string format
    public static List<string> ranks = new List<string>
        {"F", "E", "D", "C", "B", "A", "9", "8", "7", "6", "5", "4", "3", "2", "1"};

    public List<SkillStatModel> stats = new List<SkillStatModel>();
    public List<TrainingMethodModel> trainingMethods = new List<TrainingMethodModel>();

    public SkillModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;

        fieldMap.Add("id", new ModelFieldReference(this, nameof(ID)));
        fieldMap.Add("name", new ModelFieldReference(this, nameof(name)));
        fieldMap.Add("category_id", new ModelFieldReference(this, nameof(categoryID)));
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

        readString = $"SELECT * FROM skill WHERE id = @id;";

        ReadRow();
        ReadStats();
        ReadTrainingMethods();
    }

    private void ReadStats()
    {
        string statQuery = @"SELECT skill_stat_id
            FROM skill_stat
            WHERE skill_id = @id;";

        DataTable table = database.ReadTable(statQuery, fieldMap);

        foreach (DataRow row in table.Rows)
        {
            int statID = int.Parse(row["skill_stat_id"].ToString());
            SkillStatModel stat = new SkillStatModel(database, ID, statID);
            stats.Add(stat);
        }
    }

    private void ReadTrainingMethods()
    {
        string trainingQuery = @"SELECT training_method_id
            FROM training_method
            WHERE skill_id = @id;";

        DataTable table = database.ReadTable(trainingQuery, fieldMap);

        foreach (DataRow row in table.Rows)
        {
            int methodID = int.Parse(row["training_method_id"].ToString());
            TrainingMethodModel method = new TrainingMethodModel(database, ID, methodID);
            trainingMethods.Add(method);
        }
    }
}