using System.Collections.Generic;

public class Inventory
{
    public int Width {get; private set;}
    public int Height {get; private set;}
    public int Size {get; private set;}
    public Dictionary<int, Item> items = new Dictionary<int, Item>();
    public EventManager changeEvent = new EventManager();

    public Inventory(int width, int height)
    {
        Width = width;
        Height = height;
        Size = Width * Height;
    }

    public void Add(int itemID, int quantity)
    {
        if (!items.ContainsKey(itemID))
        {
            items.Add(itemID, new Item(itemID));
        }

        items[itemID].quantity += quantity;
        changeEvent.RaiseOnChange();
    }

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
}