using UnityEngine;

public class ItemModel : BaseModel
{
    // Internal ID number
    public int ID;
    public string name;
    public int categoryID;
    public string description;
    public Sprite icon;
    // Stack size limits inside an inventory
    public int stackSizeLimit;
    public int widthInGrid;
    public int heightInGrid;

    public ItemModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "item";

        primaryKeys.Add("id");

        fieldMap.Add("id", new ModelFieldReference(this, nameof(ID)));
        fieldMap.Add("name", new ModelFieldReference(this, nameof(name)));
        fieldMap.Add("category_id", new ModelFieldReference(this, nameof(categoryID)));
        fieldMap.Add("description", new ModelFieldReference(this, nameof(description)));
        fieldMap.Add("icon", new ModelFieldReference(this, nameof(icon)));
        fieldMap.Add("stack_size_limit", new ModelFieldReference(this, nameof(stackSizeLimit)));
        fieldMap.Add("width_in_grid", new ModelFieldReference(this, nameof(widthInGrid)));
        fieldMap.Add("height_in_grid", new ModelFieldReference(this, nameof(heightInGrid)));

        CreateReadQuery();
        CreateWriteQuery();
        
        ReadRow();
    }
}