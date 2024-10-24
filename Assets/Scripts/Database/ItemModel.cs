using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class ItemModel : Model
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

    private string statTableName;

    public Dictionary<int, ItemStatModel> stats = new Dictionary<int, ItemStatModel>();

    public ItemModel(DatabaseManager database) : base(database)
    {
        tableName = "item";
        statTableName = "item_stat";

        primaryKeys.Add("id");

        fieldMap.Add("id", new ModelFieldReference(this, nameof(this.ID)));
        fieldMap.Add("name", new ModelFieldReference(this, nameof(name)));
        fieldMap.Add("category_id", new ModelFieldReference(this, nameof(categoryID)));
        fieldMap.Add("description", new ModelFieldReference(this, nameof(description)));
        fieldMap.Add("icon", new ModelFieldReference(this, nameof(icon)));
        fieldMap.Add("stack_size_limit", new ModelFieldReference(this, nameof(stackSizeLimit)));
        fieldMap.Add("width_in_grid", new ModelFieldReference(this, nameof(widthInGrid)));
        fieldMap.Add("height_in_grid", new ModelFieldReference(this, nameof(heightInGrid)));
    }

    public ItemModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "item";
        statTableName = "item_stat";

        primaryKeys.Add("id");

        fieldMap.Add("id", new ModelFieldReference(this, nameof(this.ID)));
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
        ReadStats();
    }

    private void ReadStats()
    {
        string statQuery = @$"SELECT stat_id
            FROM {statTableName}
            WHERE item_id = @id;";

        DataTable table = database.ReadTable(statQuery, fieldMap);

        foreach (DataRow row in table.Rows)
        {
            int statID = int.Parse(row["stat_id"].ToString());
            ItemStatModel stat = new ItemStatModel(database, ID, statID);
            stats.Add(statID, stat);
        }
    }

}