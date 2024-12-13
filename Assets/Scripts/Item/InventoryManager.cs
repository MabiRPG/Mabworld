using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

/// <summary>
///     Handles processing all inventory bags and the total inventory space.
/// </summary>
[JsonObject]
public class InventoryManager
{
    // Dimensions of a single slot (pixels) in the inventory window.
    public static int slotWidth = 50;
    public static int slotHeight = 50;
    // Dictionary of all items across all bags.
    [JsonProperty]
    private Dictionary<int, List<Item>> AllItems = new Dictionary<int, List<Item>>();
    // List of all bags.
    public List<InventoryBag> Bags = new List<InventoryBag>();

    [JsonIgnore]
    public EventManager changeEvent = new EventManager();

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    public InventoryManager() { }

    /// <summary>
    ///     Adds a bag.
    /// </summary>
    /// <param name="bagID">ID of the bag to determine size</param>
    public void AddBag(int bagID)
    {
        Bags.Add(new InventoryBag());
    }

    public List<Item> GetItem(int itemID)
    {
        if (AllItems.ContainsKey(itemID))
        {
            return AllItems[itemID];
        }

        return null;
    }

    private bool CheckItemEquals(Item item1, Item item2)
    {
        // Checks for equal property values, not reference!
        if (JsonConvert.SerializeObject(item1.model) != JsonConvert.SerializeObject(item2.model))
        {
            // Debug.Log("items are not the same");
            return false;
        }

        if (item1.stats.Count == 0 && item2.stats.Count == 0)
        {
            // Debug.Log("number of stats are not the same");
            return true;
        }

        foreach ((int statID, float roll) in item1.stats)
        {
            if (!item2.stats.ContainsKey(statID) ||
                item2.stats[statID] != roll)
            {
                // Debug.Log("rolls are not the same");
                return false;
            }
        }

        // Debug.Log("item is the same");
        return true;
    }

    /// <summary>
    ///     Adds an item anywhere possible.
    /// </summary>
    /// <param name="itemID">Item ID in database</param>
    /// <param name="quantity">Quantity of item to add</param>
    public int AddItem(int itemID, int quantity)
    {
        Item item = new Item(itemID);

        // Check our item cache if the item instance exists, otherwise creates it
        if (AllItems.ContainsKey(itemID))
        {
            if (item.model.stats.Count == 0)
            {
                item = AllItems[itemID].First();
            }
            // Searches for the exact same item with same rolls to add quantity.
            else
            {
                bool found = false;

                foreach (Item bagItem in AllItems[itemID])
                {
                    if (CheckItemEquals(item, bagItem))
                    {
                        item = bagItem;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    AllItems[itemID].Add(item);
                }
            }
        }
        else
        {
            AllItems.Add(itemID, new List<Item> { item });
        }

        // Begin iterating through bags, pushing items wherever possible.
        int remainingQuantity = quantity;

        foreach (InventoryBag bag in Bags)
        {
            int addedQuantity = bag.PushItem(item, remainingQuantity);
            remainingQuantity -= addedQuantity;

            if (remainingQuantity < 0)
            {
                break;
            }
        }

        item.quantity += quantity - remainingQuantity;
        changeEvent.RaiseOnChange();

        // TODO : Handle overflow inventory
        return item.quantity;
    }

    /// <summary>
    ///     Removes an item from anywhere.
    /// </summary>
    /// <param name="item"></param>
    public int RemoveItem(int itemID, int quantity)
    {
        if (!AllItems.ContainsKey(itemID))
        {
            return 0;
        }

        int remainingQuantity = quantity;
        Item item = AllItems[itemID].First();

        foreach (InventoryBag bag in Bags)
        {
            int removedQuantity = bag.PopItem(item, remainingQuantity);
            remainingQuantity -= removedQuantity;

            if (remainingQuantity == 0)
            {
                break;
            }
        }

        item.quantity -= quantity - remainingQuantity;
        changeEvent.RaiseOnChange();

        return item.quantity;
    }

    /// <summary>
    ///     Adds an item into the overflow storage.
    /// </summary>
    /// <param name="item"></param>
    public void AddOverflowItem(Item item)
    {
    }

    public int GetQuantity(Item item)
    {
        if (!AllItems.ContainsKey(item.model.ID))
        {
            return 0;
        }

        return AllItems[item.model.ID].Sum(v => v.quantity);
    }

    public int GetQuantity(ItemModel model)
    {
        if (!AllItems.ContainsKey(model.ID))
        {
            return 0;
        }

        return AllItems[model.ID].Sum(v => v.quantity);
    }

    public int GetQuantity(int itemID)
    {
        if (!AllItems.ContainsKey(itemID))
        {
            return 0;
        }

        return AllItems[itemID].Sum(v => v.quantity);
    }
}