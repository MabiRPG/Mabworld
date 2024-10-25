using System.Collections.Generic;

public class CultivationSubstageModel : TypeModel<CultivationSubstageModel>
{
    private int stageID;
    public static Dictionary<(int, int), string> substages = new Dictionary<(int, int), string>();

    public CultivationSubstageModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "cultivation_substage";

        fieldMap.Add("stage_id", new ModelFieldReference(this, nameof(stageID)));

        CreateReadQuery();
        ReadRow();

        if (!substages.ContainsKey((stageID, ID)))
        {
            substages.Add((stageID, ID), name);
        }
    }
}