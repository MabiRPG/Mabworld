using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowInventory : Window
{
    public static WindowInventory Instance = null;

    [SerializeField]
    private GameObject slot;

    private Dictionary<(int, int), GameObject> grid = new Dictionary<(int, int), GameObject>();

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

        Debug.Log("Drawing..");

        foreach (KeyValuePair<int, Item> pair in Player.Instance.inventory.items)
        {
            int quantity = pair.Value.quantity;
            Debug.Log(quantity);
           
            while (quantity > 0)
            {
                WindowInventorySlot slot = grid[(i, j)].GetComponent<WindowInventorySlot>();

                Debug.Log($"{i} {j}");

                if (slot.item == null)
                {
                    slot.SetSlot(pair.Value, Math.Min(quantity, pair.Value.stackSizeLimit));
                    quantity -= Math.Min(quantity, pair.Value.stackSizeLimit);
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
                        return;
                    }
                }
            }
        }
    }
}