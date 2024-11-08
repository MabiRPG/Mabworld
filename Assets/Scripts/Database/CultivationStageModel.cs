using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CultivationStageModel : Model
{
    public int ID;
    public int substageID;
    public string name;
    public string substageName;
    public int hp;
    public int mp;
    public int strength;
    public int intelligence;
    public int dexterity;
    public int luck;

    public static Dictionary<(int, int), CultivationStageModel> stages =
        new Dictionary<(int, int), CultivationStageModel>();

    public CultivationStageModel(DatabaseManager database, int ID, int substageID) 
        : base(database)
    {
        this.ID = ID;
        this.substageID = substageID;
        tableName = "cultivation_stage";

        primaryKeys.Add("id");
        primaryKeys.Add("substage_id");

        fieldMap.Add("id", new ModelFieldReference(this, nameof(this.ID)));       
        fieldMap.Add("substage_id", new ModelFieldReference(this, nameof(this.substageID)));
        fieldMap.Add("name", new ModelFieldReference(this, nameof(name)));       
        fieldMap.Add("substage_name", new ModelFieldReference(this, nameof(substageName))); 
        fieldMap.Add("hp", new ModelFieldReference(this, nameof(hp)));    
        fieldMap.Add("mp", new ModelFieldReference(this, nameof(mp)));
        fieldMap.Add("str", new ModelFieldReference(this, nameof(strength))); 
        fieldMap.Add("int", new ModelFieldReference(this, nameof(intelligence)));
        fieldMap.Add("dex", new ModelFieldReference(this, nameof(dexterity))); 
        fieldMap.Add("luck", new ModelFieldReference(this, nameof(luck)));                           

        CreateReadQuery();
        ReadRow();

        if (!stages.ContainsKey((ID, substageID)))
        {
            stages.Add((ID, substageID), this);
        }
    }

    public static List<CultivationStageModel> FindByStageName(string name)
    {
        return stages.Where(v => v.Value.name == name).Select(v => v.Value).ToList();
    }

    public static CultivationStageModel FindBySubstageName(string name)
    {
        return stages.Where(v => v.Value.substageName == name).Select(v => v.Value).First();
    }
}