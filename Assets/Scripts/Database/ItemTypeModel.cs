public class ItemTypeModel : TypeModel<ItemTypeModel>
{
    public ItemTypeModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "item_category_type";
        CreateReadQuery();
        ReadRow();
    }
}