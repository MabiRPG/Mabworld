using System.Collections.Generic;

public class CultivationSubstageModel : TypeModel<CultivationSubstageModel>
{
    private int stageID;
    public static Dictionary<(int, int), string> substages = new Dictionary<(int, int), string>();

    public CultivationSubstageModel(DatabaseManager database, int stageID, int ID) : base(database)
    {
        this.ID = ID;
        this.stageID = stageID;
        tableName = "cultivation_substage";

        primaryKeys.Add("id");
        primaryKeys.Add("stage_id");

        fieldMap.Add("stage_id", new ModelFieldReference(this, nameof(this.stageID)));

        CreateReadQuery();
        ReadRow();

        if (!substages.ContainsKey((stageID, ID)))
        {
            substages.Add((stageID, ID), name);
        }
    }
}