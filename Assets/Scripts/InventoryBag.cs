using System;
using System.Collections.Generic;
using System.Linq;

public class InventoryBag
{
    public int ID;
    public string name;
    public int width;
    public int height;

    public List<(int row, int column)> excludedSlots = new List<(int, int)>();
    public Dictionary<(int row, int column), InventoryItem> items = new Dictionary<(int row, int column), InventoryItem>();
    public EventManager changeEvent = new EventManager();

    public InventoryBag()
    {
        excludedSlots.Add((0, 0));
        excludedSlots.Add((3, 3));
        width = 4;
        height = 4;
    }

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

    public int PushItem(Item item, int quantity)
    {
        void Add(int quantity, int row, int column)
        {
            InventoryItem inventoryItem = new InventoryItem(item, quantity, row, column);

            for (int i = row; i < row + item.heightInGrid; i++)
            {
                for (int j = column; j < column + item.widthInGrid; j++)
                {
                    items.Add((i, j), inventoryItem);
                }
            }
        }

        if (quantity <= 0)
        {
            return 0;
        }

        int remainingQuantity = quantity;

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

        for (int i = 0; i < height - item.heightInGrid + 1; i++)
        {
            for (int j = 0; j < width - item.widthInGrid + 1; j++)
            {
                if (IsEmpty(i, j, item.widthInGrid, item.heightInGrid))
                {
                    Add(Math.Min(remainingQuantity, item.stackSizeLimit), i, j);
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

    public int CountItemsAt(int row, int column, int width, int height)
    {
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
}