using System.Collections.Generic;

public class InventoryItem
{
    public Item item;
    public int quantity;
    public int width;
    public int height;
    public (int row, int column) origin;

    public InventoryItem(Item item, int quantity, int row, int column)
    {
        this.item = item;
        this.quantity = quantity;
        width = item.widthInGrid;
        height = item.heightInGrid;
        origin = (row, column);
    }
}