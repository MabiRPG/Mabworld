using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class InventoryManagerAllItemsConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    public override bool CanWrite
    {
        get { return false; }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new Exception("Is not implemented");
    }

    public override bool CanRead
    {
        get { return true; }
    }

    public override object ReadJson(JsonReader reader, Type objectType,
        object existingValue, JsonSerializer serializer)
    {
        JToken obj = JToken.Load(reader);

        Dictionary<int, Item> AllItems = new Dictionary<int, Item>();

        foreach (JToken token in obj)
        {
            int ID = (int)token.First["model"]["ID"];
            Item item = new Item(ID);
            JsonConvert.PopulateObject(token.First.ToString(), item);
            AllItems.Add(ID, item);
        }

        return AllItems;
    }
}

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
    [JsonConverter(typeof(InventoryManagerAllItemsConverter))]
    private Dictionary<int, Item> AllItems = new Dictionary<int, Item>();
    // List of all bags.
    public List<InventoryBag> Bags = new List<InventoryBag>();

    [JsonIgnore]
    public EventManager changeEvent = new EventManager();

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    public InventoryManager()
    {
    }

    /// <summary>
    ///     Adds a bag.
    /// </summary>
    /// <param name="bagID">ID of the bag to determine size</param>
    public void AddBag(int bagID)
    {
        Bags.Add(new InventoryBag());
    }

    public Item GetItem(int itemID)
    {
        if (AllItems.ContainsKey(itemID))
        {
            return AllItems[itemID];
        }

        return null;
    }

    /// <summary>
    ///     Adds an item anywhere possible.
    /// </summary>
    /// <param name="itemID">Item ID in database</param>
    /// <param name="quantity">Quantity of item to add</param>
    public int AddItem(int itemID, int quantity)
    {
        Item item;

        // Check our item cache if the item instance exists, otherwise creates it
        if (AllItems.ContainsKey(itemID))
        {
            item = AllItems[itemID];
        }
        else
        {
            item = new Item(itemID);
            AllItems.Add(itemID, item);
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
        Item item = AllItems[itemID];

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

        return AllItems[item.model.ID].quantity;
    }

    public int GetQuantity(ItemModel item)
    {
        if (!AllItems.ContainsKey(item.ID))
        {
            return 0;
        }

        return AllItems[item.ID].quantity;
    }

    public int GetQuantity(int itemID)
    {
        if (!AllItems.ContainsKey(itemID))
        {
            return 0;
        }

        return AllItems[itemID].quantity;
    }
}