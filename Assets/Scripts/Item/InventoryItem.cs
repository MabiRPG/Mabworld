using System.Runtime.Serialization;
using Newtonsoft.Json;

/// <summary>
///     Handles the inventory item occupying the bag space
/// </summary>
[JsonObject]
public class InventoryItem
{
    public Item item;
    public int quantity;
    // Dimensions of item sprite
    public int width;
    public int height;
    // Origin in the bag space (top left corner)
    [JsonIgnore]
    public (int row, int column) origin;

    // Serialization
    [JsonProperty]
    private int _row;
    [JsonProperty]
    private int _column;

    [JsonConstructor]
    public InventoryItem() { }

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="quantity">Quantity of the item</param>
    /// <param name="row">Starting row of the item in bag</param>
    /// <param name="column">Starting column of the item in bag</param>
    public InventoryItem(Item item, int quantity, int row, int column)
    {
        this.item = item;
        this.quantity = quantity;

        width = item.model.widthInGrid;
        height = item.model.heightInGrid;
        origin = (row, column);
    }

    [OnSerializing]
    internal void OnSerializing(StreamingContext context)
    {
        _row = origin.row;
        _column = origin.column;
    }

    [OnDeserialized]
    internal void OnDeserialized(StreamingContext context)
    {
        origin = (_row, _column);
    }
}