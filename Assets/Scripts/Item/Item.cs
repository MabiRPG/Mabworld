using Newtonsoft.Json;

/// <summary>
///     Handles the individual item processing.
/// </summary>
[JsonObject]
public class Item
{
    public ItemModel model;
    public int quantity;

    [JsonConstructor]
    public Item() { }

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    /// <param name="ID">Item ID in database.</param>
    public Item(int ID)
    {
        model = new ItemModel(GameManager.Instance.Database, ID);
    }
}