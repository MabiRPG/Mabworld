using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowInventory : Window, IPointerMoveHandler, IPointerExitHandler
{
    public static WindowInventory Instance = null;
    
    [SerializeField]
    private GameObject slotPrefab;
    [SerializeField]
    private GameObject itemTooltipPrefab;
    private WindowInventoryItemTooltip tooltip;

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

        GameObject obj;

        for (int i = 0; i < Player.Instance.inventory.Height; i++)
        {
            for (int j = 0; j < Player.Instance.inventory.Width; j++)
            {
                obj = Instantiate(slotPrefab, body.transform);
                grid.Add((i, j), obj);          
            }  
        }

        obj = Instantiate(itemTooltipPrefab, transform.parent);
        tooltip = obj.GetComponent<WindowInventoryItemTooltip>();

        Draw();
    }

    private void OnEnable()
    {
        Player.Instance.inventory.changeEvent.OnChange += Draw;
        Draw();
    }

    private void OnDisable()
    {
        Player.Instance.inventory.changeEvent.OnChange -= Draw;
    }

    public void OnPointerMove(PointerEventData pointerData)
    {
        WindowInventorySlot slot = pointerData.pointerEnter.GetComponent<WindowInventorySlot>();

        if (slot != null && slot.item != null)
        {
            tooltip.SetItem(slot.item);
            
            Vector2 pos;
            RectTransform canvasRect = transform.parent.GetComponent<RectTransform>();
            Camera canvasCamera = transform.parent.GetComponent<Canvas>().worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, Input.mousePosition, canvasCamera, out pos);
            pos.x -= 10;
            pos.y -= 10;

            RectTransform rect = tooltip.gameObject.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
        }
        else
        {
            tooltip.Clear();
        }
    }

    public void OnPointerExit(PointerEventData pointerData)
    {
        tooltip.Clear();
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