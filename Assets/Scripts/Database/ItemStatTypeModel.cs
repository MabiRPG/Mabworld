public class ItemStatTypeModel : TypeModel<ItemStatTypeModel>
{
    public ItemStatTypeModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "item_stat_type";
        CreateReadQuery();
        ReadRow();
    }
}