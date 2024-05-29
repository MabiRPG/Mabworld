using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowInventory : Window, IPointerMoveHandler, IPointerExitHandler
{
    public static WindowInventory Instance = null;

    private float slotWidth;
    private float slotHeight;
    
    [SerializeField]
    private GameObject itemPrefab;
    [SerializeField]
    private GameObject itemTooltipPrefab;
    private WindowInventoryItemTooltip tooltip;

    private Dictionary<Item, List<WindowInventoryItem>> items = new Dictionary<Item, List<WindowInventoryItem>>();
    private List<List<bool>> isSlotUsed = new List<List<bool>>();
    private PrefabManager itemPrefabs;

    //private Dictionary<(int, int), GameObject> grid = new Dictionary<(int, int), GameObject>();
    //private Dictionary<Item, List<(int, int)>> usedSlots = new Dictionary<Item, List<(int, int)>>();

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
        
        GameObject obj = Instantiate(itemTooltipPrefab, transform.parent);
        tooltip = obj.GetComponent<WindowInventoryItemTooltip>();

        itemPrefabs = ScriptableObject.CreateInstance<PrefabManager>();
        itemPrefabs.SetPrefab(itemPrefab);

        for (int i = 0; i < Player.Instance.inventory.Height; i++)
        {
            isSlotUsed.Add(new List<bool>(new bool[Player.Instance.inventory.Width]));
        }

        float width = body.transform.Find("Background").GetComponent<RectTransform>().sizeDelta.x;
        float height = body.transform.Find("Background").GetComponent<RectTransform>().sizeDelta.y;
        slotWidth = width / Player.Instance.inventory.Width;
        slotHeight = height / Player.Instance.inventory.Height;

        Draw();

        // Hides the object at start
        //gameObject.SetActive(false);
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
            pos.x -= 5;
            pos.y += 5;

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

    /// <summary>
    ///     Finds the nearest free space for the item, given the dimensions.
    /// </summary>
    /// <param name="item">Item instance</param>
    /// <returns>Point2D of the free space, null otherwise</returns>
    private Point2D GetFree(Item item)
    {
        Point2D point = new Point2D();

        // Sliding window approach, going through the full usedslot matrix to find
        // an open space for the item.
        for (int i = 0; i < Player.Instance.inventory.Height - item.heightInGrid + 1; i++)
        {
            for (int j = 0; j < Player.Instance.inventory.Width - item.widthInGrid + 1; j++)
            {
                bool isAllEmpty = true;

                for (int k = 0; k < item.heightInGrid; k++)
                {
                    if (isSlotUsed[i + k].GetRange(j, item.widthInGrid).Any(i => i.Equals(true)))
                    {
                        isAllEmpty = false;
                        break;
                    }    
                }

                if (isAllEmpty)
                {
                    point.Point = (i, j);
                    return point;
                }
            }
        }

        return null;
    }

    private void Draw()
    {
        foreach (Item item in Player.Instance.inventory.items.Values)
        {
            int remainingQuantity = item.quantity;

            // If the item already exists in the inventory, try to add the new quantity
            // and populate it
            if (items.ContainsKey(item))
            {
                foreach (WindowInventoryItem itemScript in items[item])
                {
                    // Remove the object if the quantity is below 0
                    if (remainingQuantity <= 0)
                    {
                        itemScript.gameObject.SetActive(false);
                        items[item].Remove(itemScript);
                    }

                    // Sets the new quantity based on stack limits
                    itemScript.SetItem(item, Math.Min(remainingQuantity, item.stackSizeLimit));
                    remainingQuantity -= itemScript.quantity;
                }
            }
            // If the item does not exist, create new container for it
            else
            {
                items.Add(item, new List<WindowInventoryItem>());
            }

            // If there is remaining quantity, create new prefabs.
            while (remainingQuantity > 0)
            {
                // Find a free space on the inventory
                Point2D pos = GetFree(item);

                // Allocate a new prefab and set the item details
                GameObject obj = itemPrefabs.GetFree((item, pos), body.transform.Find("Background"));
                WindowInventoryItem itemScript = obj.GetComponent<WindowInventoryItem>();
                itemScript.SetItem(item, Math.Min(remainingQuantity, item.stackSizeLimit));
                remainingQuantity -= itemScript.quantity;

                // Change the position and sie according to dimensions
                RectTransform rectTransform = obj.GetComponent<RectTransform>();
                // Here, our Y is equal to X in world space, and our -X is Y.
                rectTransform.anchoredPosition = new Vector3(pos.Y * slotWidth, -pos.X * slotHeight, 0);
                rectTransform.sizeDelta = new Vector2(item.widthInGrid * slotWidth, item.heightInGrid * slotHeight);

                // Reserve the spaces in the used matrix
                for (int i = (int)pos.X; i < (int)pos.X + item.heightInGrid; i++)
                {
                    for (int j = (int)pos.Y; j < (int)pos.Y + item.widthInGrid; j++)
                    {
                        isSlotUsed[i][j] = true;
                    }
                } 

                items[item].Add(itemScript);
            }
        }
    }

    private void PrintMatrix()
    {
        for (int i = 0; i < isSlotUsed.Count; i++)
        {
            string s = "";

            for (int j = 0; j < isSlotUsed[i].Count; j++)
            {
                s += isSlotUsed[i][j] + ",";
            }

            Debug.Log(s);
        }
    }
}

//     private void Draw()
//     {
//         int i = 0;
//         int j = 0;
//         WindowInventorySlot slot;
//         int amountInSlot;

//         foreach (KeyValuePair<int, Item> pair in Player.Instance.inventory.items)
//         {
//             int itemID = pair.Key;
//             Item item = pair.Value;
//             int quantity = item.quantity;

//             if (usedSlots.ContainsKey(item))
//             {
//                 foreach ((int, int) pos in usedSlots[item])
//                 {
//                     slot = grid[pos].GetComponent<WindowInventorySlot>(); 
//                     amountInSlot = Math.Min(quantity, item.stackSizeLimit);
//                     slot.SetSlot(item, amountInSlot);
//                     quantity -= amountInSlot;

//                     if (quantity <= 0)
//                     {
//                         break;
//                     }
//                 }
//             }
           
//             while (quantity > 0)
//             {
//                 slot = grid[(i, j)].GetComponent<WindowInventorySlot>();

//                 if (slot.item == null)
//                 {
//                     amountInSlot = Math.Min(quantity, item.stackSizeLimit);
//                     slot.SetSlot(item, amountInSlot);
//                     quantity -= amountInSlot;
                    
//                     if (usedSlots.ContainsKey(item))
//                     {
//                         usedSlots[item].Add((i, j));
//                     }
//                     else
//                     {
//                         usedSlots.Add(item, new List<(int, int)>{(i, j)});
//                     }
//                 }
//                 else
//                 {
//                     j++;

//                     if (!grid.ContainsKey((i, j)))
//                     {
//                         j = 0;
//                         i++;
//                     }

//                     if (!grid.ContainsKey((i, j)))
//                     {
//                         break;
//                     }
//                 }
//             }
//         }
//     }
// }