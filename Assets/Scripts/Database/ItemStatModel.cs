public class ItemStatModel : Model
{
    public int itemID;
    public int statID;
    public float min;
    public float max;

    public ItemStatModel(DatabaseManager database, int itemID, int statID) : base(database)
    {
        this.itemID = itemID;
        this.statID = statID;

        primaryKeys.Add("item_id");
        primaryKeys.Add("stat_id");

        fieldMap.Add("item_id", new ModelFieldReference(this, nameof(itemID)));
        fieldMap.Add("stat_id", new ModelFieldReference(this, nameof(statID)));
        fieldMap.Add("min", new ModelFieldReference(this, nameof(min)));
        fieldMap.Add("max", new ModelFieldReference(this, nameof(max)));

        CreateReadQuery();
        CreateWriteQuery();

        ReadRow();
    }
}