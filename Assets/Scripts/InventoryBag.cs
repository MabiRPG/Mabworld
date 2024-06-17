using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryBag
{
    public int ID;
    public string name;
    public int width;
    public int height;

    public enum State
    {
        Null,
        Empty,
        Full
    }
    public static Dictionary<int, State> StateMap = new Dictionary<int, State>
    {
        {-1, State.Null},
        {0, State.Empty},
        {1, State.Full}
    };

    public List<List<int>> Grid;
    public Dictionary<(int startingRow, int startingColumn), (Item item, int quantity)> Items = 
        new Dictionary<(int, int), (Item, int)>();
    public Dictionary<(int row, int column), (int startingRow, int startingColumn)> ItemSlotsUsed = 
        new Dictionary<(int row, int column), (int startingRow, int startingColumn)>();
    public EventManager changeEvent = new EventManager();

    public InventoryBag(int ID)
    {
        this.ID = ID;
        CreateGrid(ID);

        changeEvent.OnChange += Print;
    }

    public void CreateGrid(int ID)
    {
        switch(ID)
        {
            case 1:
                Grid = new List<List<int>> {
                    new List<int>{-1, 0, 0, 0 },
                    new List<int>{0, 0, 0, 0 },
                    new List<int>{0, 0, 0, 0 },
                    new List<int>{0, 0, 0, -1 }
                };

                break;
            case 2:
                Grid = new List<List<int>> {
                    new List<int>{-1, 0, 0, 0, 0, 0, -1 },
                    new List<int>{-1, -1, 0, 0, 0, -1, -1 },
                    new List<int>{ 0, 0, 0, 0, 0, 0, 0 }
                };
                break;
        }

        width = Grid[0].Count;
        height = Grid.Count;
    }

    public (int, int) GetStartingPos(int row, int column)
    {
        if (!ItemSlotsUsed.ContainsKey((row, column)))
        {
            return (-1, -1);
        }

        return ItemSlotsUsed[(row, column)];
    }

    public (Item, int) GetItemAtPos(int row, int column)
    {
        if (!Items.ContainsKey((row, column)))
        {
            return (null, 0);
        }

        return Items[GetStartingPos(row, column)];
    }

    private (int, int) FindFreePosition(int itemWidth, int itemHeight)
    {
        if (Grid.Count == 0 || itemWidth <= 0 || itemWidth > Grid[0].Count || itemHeight <= 0 || itemHeight > Grid.Count)
        {
            return (-1, -1);
        }

        for (int row = 0; row <= Grid.Count - itemHeight; row++)
        {
            for (int column = 0; column <= Grid[row].Count - itemWidth; column++)
            {
                if (IsEmpty(row, column, itemWidth, itemHeight))
                {
                    return (row, column);
                }
            }
        }

        return (-1, -1);
    }

    public bool IsEmpty(int startingRow, int startingColumn, int itemWidth, int itemHeight)
    {
        for (int row = startingRow; row < startingRow + itemHeight; row++)
        {
            for (int column = startingColumn; column < startingColumn + itemWidth; column++)
            {
                if (StateMap[Grid[row][column]] != State.Empty)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void SetState(int startingRow, int startingColumn, int width, int height, int state)
    {
        if (!StateMap.ContainsKey(state))
        {
            return;
        }

        for (int row = startingRow; row < startingRow + height; row++)
        {
            for (int column = startingColumn; column < startingColumn + width; column++)
            {
                Grid[row][column] = state;

                if (ItemSlotsUsed.ContainsKey((row, column)) && StateMap[state] == State.Empty)
                {
                    ItemSlotsUsed.Remove((row, column));
                }
                else if (!ItemSlotsUsed.ContainsKey((row, column)) && StateMap[state] == State.Full)
                {
                    ItemSlotsUsed.Add((row, column), (startingRow, startingColumn));
                }
            }
        }
    }

    /// <summary>
    ///     Adds an item to the inventory bag, if possible.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="quantity"></param>
    /// <returns>Number of items successfully added.</returns>
    public int AddItem(Item item, int quantity)
    {
        if (quantity <= 0)
        {
            return 0;
        }

        int remainingQuantity = quantity;

        foreach (var key in Items.Keys.ToList())
        {
            (Item slotItem, int slotQuantity) = Items[key];

            if (item == slotItem && slotQuantity < item.stackSizeLimit)
            {
                slotQuantity = Math.Min(slotQuantity + remainingQuantity, item.stackSizeLimit);
                Items[key] = (item, slotQuantity);
                remainingQuantity -= slotQuantity;
            }

            if (remainingQuantity == 0)
            {
                break;
            }
        }

        while (remainingQuantity > 0)
        {
            (int startingRow, int startingColumn) = FindFreePosition(item.widthInGrid, item.heightInGrid);

            if (startingRow == -1 || startingColumn == -1)
            {
                break;
            }

            SetState(startingRow, startingColumn, item.widthInGrid, item.heightInGrid, 1);
            int slotQuantity = Math.Min(remainingQuantity, item.stackSizeLimit);
            Items.Add((startingRow, startingColumn), (item, slotQuantity));
            remainingQuantity -= slotQuantity;
        }

        changeEvent.RaiseOnChange();
        return quantity - remainingQuantity;
    }

    public void AddItem(Item item, int quantity, int startingRow, int startingColumn)
    {
        SetState(startingRow, startingColumn, item.widthInGrid, item.heightInGrid, 1);
        Items.Add((startingRow, startingColumn), (item, quantity));
    }

    public bool RemoveItem(int quantity, int startingRow, int startingColumn)
    {
        if (startingRow == -1 || startingColumn == -1 || quantity <= 0 || !Items.ContainsKey((startingRow, startingColumn)))
        {
            return false;
        }

        (Item slotItem, int slotQuantity) = Items[(startingRow, startingColumn)];
        slotQuantity -= Math.Max(quantity, 0);

        if (slotQuantity == 0)
        {
            SetState(startingRow, startingColumn, slotItem.widthInGrid, slotItem.heightInGrid, 0);
            Items.Remove((startingRow, startingColumn));
        }
        else
        {
            Items[(startingRow, startingColumn)] = (slotItem, slotQuantity);
        }

        changeEvent.RaiseOnChange();
        return true;
    }

    public bool ShiftItem(int startingRow, int startingColumn, int endingRow, int endingColumn)
    {
        if (!Items.ContainsKey((startingRow, startingColumn)) || Items.ContainsKey((endingRow, endingColumn)))
        {
            return false;
        }

        (Item slotItem, int slotQuantity) = Items[(startingRow, startingColumn)];
        Items.Remove((startingRow, startingColumn));
        Items.Add((endingRow, endingColumn), (slotItem, slotQuantity));

        foreach ((int row, int column) in ItemSlotsUsed.Keys.ToList())
        {
            if (ItemSlotsUsed[(row, column)] == (startingRow, startingColumn))
            {
                
            }
        }

        return true;
    }

    private void Print()
    {
        foreach (var pair in Items)
        {
            Debug.Log($"({pair.Key.startingRow},{pair.Key.startingColumn}) {pair.Value.item.name}:{pair.Value.quantity}");
        }
    }
}