using System.Data;
using UnityEngine;

/// <summary>
///     Handles the individual item processing.
/// </summary>
public class Item
{
    // Internal ID number
    public int ID;
    public string name;
    public string description;
    public Sprite icon;
    // Stack size limits inside an inventory
    public int stackSizeLimit;
    public int widthInGrid;
    public int heightInGrid;

    public int quantity;
    
    private const string itemQuery = @"SELECT * FROM item WHERE id = @id LIMIT 1;";

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    /// <param name="ID">Item ID in database.</param>
    public Item(int ID)
    {
        this.ID = ID;

        DataTable dt = GameManager.Instance.QueryDatabase(itemQuery, ("@id", ID));
        DataRow row = dt.Rows[0];
        GameManager.Instance.ParseDatabaseRow(row, this);
    }
}