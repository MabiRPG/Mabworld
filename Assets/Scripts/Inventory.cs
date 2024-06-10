using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Handles the player inventory.
/// </summary>
public class Inventory
{
    // Width and height of inventory in cell size.
    public int Width {get; private set;}
    public int Height {get; private set;}
    public int Size {get; private set;}
    public Dictionary<int, Item> items = new Dictionary<int, Item>();
    public EventManager changeEvent = new EventManager();

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    /// <param name="width">Starting width in cell size.</param>
    /// <param name="height">Starting height in cell size.</param>
    public Inventory(int width, int height)
    {
        Width = width;
        Height = height;
        Size = Width * Height;
        changeEvent.OnChange += Print;
    }

    /// <summary>
    ///     Adds an item to player inventory.
    /// </summary>
    /// <param name="itemID">Item ID in database.</param>
    /// <param name="quantity">Quantity of item to add.</param>
    public void Add(int itemID, int quantity)
    {
        if (!items.ContainsKey(itemID))
        {
            items.Add(itemID, new Item(itemID));
        }

        items[itemID].quantity += quantity;
        changeEvent.RaiseOnChange();
    }

    /// <summary>
    ///     Removes an item from player inventory.
    /// </summary>
    /// <param name="itemID">Item ID in database.</param>
    /// <param name="quantity">Quantity of item to remove.</param>
    public void Remove(int itemID, int quantity)
    {
        if (!items.ContainsKey(itemID))
        {
            return;
        }

        items[itemID].quantity -= quantity;

        if (items[itemID].quantity <= 0)
        {
            items.Remove(itemID);
        }

        changeEvent.RaiseOnChange();
    }

    public void Print()
    {
        Debug.Log("\n");

        foreach (KeyValuePair<int, Item> pair in items)
        {
            Debug.Log($"{pair.Value.name} {pair.Value.quantity}");
        }
    }
}