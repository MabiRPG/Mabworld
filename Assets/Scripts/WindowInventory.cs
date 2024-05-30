using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WindowInventory : Window, IPointerMoveHandler, IPointerExitHandler
{
    public static WindowInventory Instance = null;

    [SerializeField]
    private GameObject slotBackgroundPrefab;

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

        // Requires an empty gameobject under body to insert into, since the pivot position of the
        // body is determined by a layout group, giving incorrect size deltas.
        RectTransform canvasTransform = body.transform.Find("Item Canvas").GetComponent<RectTransform>();
        slotWidth = canvasTransform.sizeDelta.x / Player.Instance.inventory.Width;
        slotHeight = canvasTransform.sizeDelta.y / Player.Instance.inventory.Height;

        for (int i = 0; i < Player.Instance.inventory.Height; i++)
        {
            isSlotUsed.Add(new List<bool>(new bool[Player.Instance.inventory.Width]));

            // Creating the background of the inventory
            for (int j = 0; j < Player.Instance.inventory.Width; j++)
            {
                Instantiate(slotBackgroundPrefab, body.transform);
            }
        }

        // Pushing item canvas to last so it appears on top of grid
        canvasTransform.SetAsLastSibling();
        // Changing grid size based on values above
        GridLayoutGroup gridGroup = body.GetComponent<GridLayoutGroup>();
        gridGroup.cellSize = new Vector2(slotWidth, slotHeight);

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
        WindowInventoryItem itemHover = pointerData.pointerEnter.GetComponent<WindowInventoryItem>();

        if (itemHover != null && itemHover.item != null)
        {
            tooltip.SetItem(itemHover.item);
            
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
                GameObject obj = itemPrefabs.GetFree((item, pos), body.transform.Find("Item Canvas"));
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
}