using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowInventory : Window
{
    public static WindowInventory Instance = null;

    [SerializeField]
    private GameObject slot;

    private Dictionary<(int, int), GameObject> grid = new Dictionary<(int, int), GameObject>();
    private Dictionary<Item, List<(int, int)>> usedSlots = new Dictionary<Item, List<(int, int)>>();

    protected override void Awake()
    {
        base.Awake();

        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Hides the object at start
        gameObject.SetActive(false);

        for (int i = 0; i < Player.Instance.inventory.Height; i++)
        {
            for (int j = 0; j < Player.Instance.inventory.Width; j++)
            {
                GameObject obj = Instantiate(slot, body.transform);
                grid.Add((i, j), obj);          
            }  
        }
    }

    private void OnEnable()
    {
        Player.Instance.inventory.changeEvent.OnChange += Draw;
    }

    private void OnDisable()
    {
        Player.Instance.inventory.changeEvent.OnChange -= Draw;
    }

    private void Draw()
    {
        int i = 0;
        int j = 0;
        WindowInventorySlot slot;
        int amountInSlot;

        foreach (KeyValuePair<int, Item> pair in Player.Instance.inventory.items)
        {
            int itemID = pair.Key;
            Item item = pair.Value;
            int quantity = item.quantity;

            if (usedSlots.ContainsKey(item))
            {
                foreach ((int, int) pos in usedSlots[item])
                {
                    slot = grid[pos].GetComponent<WindowInventorySlot>(); 
                    amountInSlot = Math.Min(quantity, item.stackSizeLimit);
                    slot.SetSlot(item, amountInSlot);
                    quantity -= amountInSlot;

                    if (quantity <= 0)
                    {
                        break;
                    }
                }
            }
           
            while (quantity > 0)
            {
                slot = grid[(i, j)].GetComponent<WindowInventorySlot>();

                if (slot.item == null)
                {
                    amountInSlot = Math.Min(quantity, item.stackSizeLimit);
                    slot.SetSlot(item, amountInSlot);
                    quantity -= amountInSlot;
                    
                    if (usedSlots.ContainsKey(item))
                    {
                        usedSlots[item].Add((i, j));
                    }
                    else
                    {
                        usedSlots.Add(item, new List<(int, int)>{(i, j)});
                    }
                }
                else
                {
                    j++;

                    if (!grid.ContainsKey((i, j)))
                    {
                        j = 0;
                        i++;
                    }

                    if (!grid.ContainsKey((i, j)))
                    {
                        break;
                    }
                }
            }
        }
    }
}