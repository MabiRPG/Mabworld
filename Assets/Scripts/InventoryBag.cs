using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
///     Handles the inventory processing per bag.
/// </summary>
public class InventoryBag
{
    public int ID;
    public string name;
    public int width;
    public int height;

    public List<(int row, int column)> excludedSlots = new List<(int, int)>();
    public Dictionary<(int row, int column), InventoryItem> items = new Dictionary<(int row, int column), InventoryItem>();
    public EventManager changeEvent = new EventManager();

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    public InventoryBag()
    {
        excludedSlots.Add((0, 0));
        excludedSlots.Add((3, 3));
        width = 4;
        height = 4;
    }

    /// <summary>
    ///     Cycles through the indices to determine if the space is empty.
    /// </summary>
    /// <param name="row">Starting row index</param>
    /// <param name="column">Starting column index</param>
    /// <param name="width">Width of item</param>
    /// <param name="height">Height of item</param>
    /// <returns>True if empty, False otherwise</returns>
    public bool IsEmpty(int row, int column, int width, int height)
    {
        if (row + height > this.height || column + width > this.width)
        {
            return false;
        }

        for (int i = row; i < row + height; i++)
        {
            for (int j = column; j < column + width; j++)
            {
                if (items.ContainsKey((i, j)) || excludedSlots.Contains((i, j)))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    ///     Adds an item anywhere available in the inventory.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="quantity"></param>
    /// <returns>Quantity of item added.</returns>
    public int PushItem(Item item, int quantity)
    {
        if (quantity <= 0)
        {
            return 0;
        }

        int remainingQuantity = quantity;

        // Add to existing items, if they are less than the maximum stack size limit.
        foreach (InventoryItem inventoryItem in items.Values)
        {
            if (item == inventoryItem.item && inventoryItem.quantity < item.stackSizeLimit)
            {
                int diff = Math.Min(inventoryItem.quantity + remainingQuantity, item.stackSizeLimit);
                diff -= inventoryItem.quantity;

                inventoryItem.quantity += diff;
                remainingQuantity -= diff;
            }

            if (remainingQuantity == 0)
            {
                changeEvent.RaiseOnChange();
                return quantity;
            }
        }

        // Iterate over space, and create a new item if possible.
        for (int i = 0; i < height - item.heightInGrid + 1; i++)
        {
            for (int j = 0; j < width - item.widthInGrid + 1; j++)
            {
                if (IsEmpty(i, j, item.widthInGrid, item.heightInGrid))
                {
                    InventoryItem inventoryItem = new InventoryItem(item, 
                        Math.Min(remainingQuantity, item.stackSizeLimit), i, j);

                    InsertItemAt(inventoryItem, i, j);
                    remainingQuantity -= Math.Min(remainingQuantity, item.stackSizeLimit);

                    if (remainingQuantity == 0)
                    {
                        changeEvent.RaiseOnChange();
                        return quantity;
                    }
                }
            }
        }

        changeEvent.RaiseOnChange();
        return quantity - remainingQuantity;
    }

    /// <summary>
    ///     Inserts an item at a specific row and column.
    /// </summary>
    /// <param name="inventoryItem"></param>
    /// <param name="row">Starting row index</param>
    /// <param name="column">Starting column index</param>
    /// <returns>True if inserted, False otherwise.</returns>
    public bool InsertItemAt(InventoryItem inventoryItem, int row, int column)
    {
        if (!IsEmpty(row, column, inventoryItem.width, inventoryItem.height))
        {
            return false;
        }

        for (int i = row; i < row + inventoryItem.height; i++)
        {
            for (int j = column; j < column + inventoryItem.width; j++)
            {
                items.Add((i, j), inventoryItem);
            }
        }

        inventoryItem.origin = (row, column);
        changeEvent.RaiseOnChange();
        return true;
    }

    /// <summary>
    ///     Removes an item at a specific row and column.
    /// </summary>
    /// <param name="row">Starting row index</param>
    /// <param name="column">Starting column index</param>
    /// <returns>InventoryItem instance found, Null otherwise.</returns>
    public InventoryItem RemoveItemAt(int row, int column)
    {
        if (!items.ContainsKey((row, column)))
        {
            return null;
        }

        InventoryItem inventoryItem = items[(row, column)];

        foreach ((int i, int j) in items.Keys.ToList())
        {
            if (items[(i, j)] == inventoryItem)
            {
                items.Remove((i, j));
            }
        }

        changeEvent.RaiseOnChange();
        return inventoryItem;
    }

    /// <summary>
    ///     Counts the items in a defined space.
    /// </summary>
    /// <param name="row">Starting row index</param>
    /// <param name="column">Starting column index</param>
    /// <param name="width">Width of space</param>
    /// <param name="height">Height of space</param>
    /// <returns>Number of items found.</returns>
    public int CountItemsAt(int row, int column, int width, int height)
    {
        if (row + height > this.height || column + width > this.width)
        {
            return 0;
        }        

        List<InventoryItem> itemsHit = new List<InventoryItem>();

        for (int i = row; i < row + height; i++)
        {
            for (int j = column; j < column + width; j++)
            {
                if (items.ContainsKey((i, j)) && !itemsHit.Contains(items[(i, j)]))
                {
                    itemsHit.Add(items[(i, j)]);
                }
            }
        }

        return itemsHit.Count;
    }

    public InventoryItem FindItemAt(int row, int column)
    {
        if (!items.ContainsKey((row, column)))
        {
            return null;
        }

        return items[(row, column)];
    }

    /// <summary>
    ///     Finds all the items in a defined space.
    /// </summary>
    /// <param name="row1">Starting row index</param>
    /// <param name="column1">Starting column index</param>
    /// <param name="row2">Ending row index</param>
    /// <param name="column2">Ending column index</param>
    /// <returns>List of InventoryItem found.</returns>
    public List<InventoryItem> FindItemsAt(int row1, int column1, int row2, int column2)
    {
        List<InventoryItem> itemsFound = new List<InventoryItem>();

        if (row1 > height || column1 > width)
        {
            return itemsFound;
        }

        for (int i = row1; i < row2; i++)
        {
            for (int j = column1; j < column2; j++)
            {
                if (items.ContainsKey((i, j)))
                {
                    itemsFound.Add(items[(i, j)]);
                }
            }
        }

        return itemsFound;
    }
}