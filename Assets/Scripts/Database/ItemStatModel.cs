using UnityEngine;

public class ItemStatModel : Model
{
    public int itemID;
    public int statID;
    public float value;

    public ItemStatModel(DatabaseManager database, int itemID, int statID) : base(database)
    {
        this.itemID = itemID;
        this.statID = statID;
        tableName = "item_stat";

        primaryKeys.Add("item_id");
        primaryKeys.Add("stat_id");

        fieldMap.Add("item_id", new ModelFieldReference(this, nameof(this.itemID)));
        fieldMap.Add("stat_id", new ModelFieldReference(this, nameof(this.statID)));
        fieldMap.Add("value", new ModelFieldReference(this, nameof(value)));

        CreateReadQuery();
        CreateWriteQuery();

        ReadRow();
    }
}