using System.Collections.Generic;

public class ItemStatTypeModel : TypeModel<ItemStatTypeModel>
{
    private bool isRange;
    public static Dictionary<int, bool> range = new Dictionary<int, bool>();

    public ItemStatTypeModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "item_stat_type";

        fieldMap.Add("is_range", new ModelFieldReference(this, nameof(isRange)));

        CreateReadQuery();
        ReadRow();

        if (!range.ContainsKey(ID))
        {
            range.Add(ID, isRange);
        }
    }

    public static bool IsRange(int ID)
    {
        if (range.ContainsKey(ID))
        {
            return range[ID];
        }

        return false;
    }
}