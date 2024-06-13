using System.Collections.Generic;

public class InventoryManager
{
    public static int slotWidth = 50;
    public static int slotHeight = 50;
    public Dictionary<int, Item> AllItems = new Dictionary<int, Item>();
    public List<InventoryBag> Bags = new List<InventoryBag>();

    public InventoryManager()
    {
        AddBag(1);
    }

    public void AddBag(int bagID)
    {
        Bags.Add(new InventoryBag(bagID));
    }

    public void AddItem(int itemID, int quantity)
    {
        Item item;

        if (AllItems.ContainsKey(itemID))
        {
            item = AllItems[itemID];
        }
        else
        {
            item = new Item(itemID);
            AllItems.Add(itemID, item);
        }

        int remainingQuantity = quantity;

        foreach (InventoryBag bag in Bags)
        {
            int addedQuantity = bag.AddItem(item, remainingQuantity);
            remainingQuantity -= addedQuantity;

            if (remainingQuantity < 0)
            {
                break;
            }
        }

        item.quantity += quantity - remainingQuantity;
    }

    public void RemoveItem(WindowInventoryItem item)
    {
    }

    public void AddOverflowItem(Item item)
    {
    }
}