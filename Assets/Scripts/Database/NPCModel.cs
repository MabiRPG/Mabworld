using UnityEngine;

public class NPCModel : Model
{
    public int ID;
    public string name;
    public Sprite icon;
    public int cultivationStageID;
    public int cultivationSubstageID;

    public NPCModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "npc";

        primaryKeys.Add("id");

        fieldMap.Add("id", new ModelFieldReference(this, nameof(ID)));
        fieldMap.Add("name", new ModelFieldReference(this, nameof(name)));
        fieldMap.Add("icon", new ModelFieldReference(this, nameof(icon)));
        fieldMap.Add("cultivation_stage_id", new ModelFieldReference(this, nameof(cultivationStageID)));
        fieldMap.Add("cultivation_substage_id", new ModelFieldReference(this, nameof(cultivationSubstageID)));

        CreateReadQuery();
        CreateWriteQuery();

        ReadRow();
    }
}