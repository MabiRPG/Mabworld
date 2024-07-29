/// <summary>
///     Handles the inventory item occupying the bag space
/// </summary>
public class InventoryItem
{
    public Item item;
    public int quantity;
    // Dimensions of item sprite
    public int width;
    public int height;
    // Origin in the bag space (top left corner)
    public (int row, int column) origin;

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
        width = item.widthInGrid;
        height = item.heightInGrid;
        origin = (row, column);
    }
}