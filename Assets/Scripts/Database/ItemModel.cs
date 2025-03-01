using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class ItemModel : Model
{
    // Internal ID number
    [JsonProperty]
    private int id;
    [JsonProperty]
    private string name;
    [JsonProperty]
    private int categoryID;
    [JsonProperty]
    private string description;
    [JsonProperty]
    private string icon;
    // Stack size limits inside an inventory
    [JsonProperty]
    private int stackSizeLimit;
    [JsonProperty]
    private int widthInGrid;
    [JsonProperty]
    private int heightInGrid;

    private string statTableName;

    [JsonProperty]
    public Dictionary<int, ItemStatModel> stats = new Dictionary<int, ItemStatModel>();

    public int ID { get => id; set => id = value; }
    public string Name { get => name; set => name = value; }
    public int CategoryID { get => categoryID; set => categoryID = value; }
    public string Description { get => description; set => description = value; }
    public Sprite Icon
    {
        get => GameManager.Instance.LoadAsset<Sprite>(icon);
#if UNITY_EDITOR
        set => icon = GameManager.Instance.Database.AddToAddressables(value);
#endif
    }
    public int StackSizeLimit { get => stackSizeLimit; set => stackSizeLimit = value; }
    public int WidthInGrid { get => widthInGrid; set => widthInGrid = value; }
    public int HeightInGrid { get => heightInGrid; set => heightInGrid = value; }

    [JsonConstructor]
    public ItemModel() : base(null) { }

    public ItemModel(DatabaseManager database) : base(database)
    {
        tableName = "item";
        statTableName = "item_stat";

        primaryKeys.Add("id");

        fieldMap.Add("id", new ModelFieldReference(this, nameof(id)));
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
        id = ID;
        tableName = "item";
        statTableName = "item_stat";

        primaryKeys.Add("id");

        fieldMap.Add("id", new ModelFieldReference(this, nameof(id)));
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