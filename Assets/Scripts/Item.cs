using System.Data;
using UnityEngine;

public class Item
{
    public int ID;
    public string name;
    public string description;
    public Sprite icon;
    public int stackSizeLimit;
    public int widthInGrid = 2;
    public int heightInGrid = 2;

    public int quantity;
    
    private const string itemQuery = @"SELECT * FROM item WHERE id = @id LIMIT 1;";

    public Item(int ID)
    {
        this.ID = ID;

        DataTable dt = GameManager.Instance.QueryDatabase(itemQuery, ("@id", ID));
        DataRow row = dt.Rows[0];
        GameManager.Instance.ParseDatabaseRow(row, this);
    }
}