using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
///     Handles all window inventory processing.
/// </summary>
public class WindowInventory : Window, IPointerMoveHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static WindowInventory Instance = null;

    // Prefabs for the slot background objects
    [SerializeField]
    private GameObject slotBackgroundPrefab;

    // Dimensions of the cell width in the item canvas
    private RectTransform itemRect;
    private float slotWidth;
    private float slotHeight;
    
    // Prefabs for the actual item sprites
    [SerializeField]
    private GameObject itemPrefab;
    // Tooltip on hover over item
    [SerializeField]
    private GameObject itemTooltipPrefab;
    private WindowInventoryItemTooltip tooltip;

    // Dictionary of all items for quick reference
    private Dictionary<Item, List<WindowInventoryItem>> items = new Dictionary<Item, List<WindowInventoryItem>>();
    // 2d list of all slots that are occupied (true) or empty (false)
    private List<List<bool>> isSlotUsed = new List<List<bool>>();
    private PrefabManager itemPrefabs;

    // For dragging and dropping items around...
    // Starting item position of the drag
    private Vector2 startingPos;
    private GameObject draggingObj;
    private Item draggingItem;
    // Slot background objects that are currently highlighted
    private List<GameObject> highlightObjs = new List<GameObject>();
    private RectTransform draggingRect;
    private bool isDragging;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
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
        itemRect = body.transform.Find("Item Canvas").GetComponent<RectTransform>();
        slotWidth = itemRect.sizeDelta.x / Player.Instance.inventory.Width;
        slotHeight = itemRect.sizeDelta.y / Player.Instance.inventory.Height;

        for (int i = 0; i < Player.Instance.inventory.Height; i++)
        {
            // Creating lists by default of false.
            isSlotUsed.Add(new List<bool>(new bool[Player.Instance.inventory.Width]));

            // Creating the background of the inventory
            for (int j = 0; j < Player.Instance.inventory.Width; j++)
            {
                Instantiate(slotBackgroundPrefab, body.transform);
            }
        }

        // Pushing item canvas to last so it appears on top of grid
        itemRect.SetAsLastSibling();
        // Changing grid size based on values above
        GridLayoutGroup gridGroup = body.GetComponent<GridLayoutGroup>();
        gridGroup.cellSize = new Vector2(slotWidth, slotHeight);

        // Hides the object at start
        gameObject.SetActive(false);
    }

    /// <summary>
    ///     Called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        Player.Instance.inventory.changeEvent.OnChange += Draw;
        Draw();
    }

    /// <summary>
    ///     Called when the object becomes disabled and inactive.
    /// </summary>
    private void OnDisable()
    {
        Player.Instance.inventory.changeEvent.OnChange -= Draw;
    }

    /// <summary>
    ///     Called when the mouse pointer moves inside the object.
    /// </summary>
    /// <param name="pointerData"></param>
    public void OnPointerMove(PointerEventData pointerData)
    {
        WindowInventoryItem itemHover = pointerData.pointerEnter.GetComponent<WindowInventoryItem>();

        if (itemHover != null && itemHover.item != null)
        {
            tooltip.SetItem(itemHover.item);

            RectTransform canvasRect = transform.parent.GetComponent<RectTransform>();
            Camera canvasCamera = transform.parent.GetComponent<Canvas>().worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, pointerData.position, canvasCamera, out Vector2 pos);
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

    /// <summary>
    ///     Called when the mouse pointer exits the object.
    /// </summary>
    /// <param name="pointerData"></param>
    public void OnPointerExit(PointerEventData pointerData)
    {
        tooltip.Clear();
    }

    /// <summary>
    ///     Called on beginning of mouse drag
    /// </summary>
    /// <param name="pointerData"></param>
    public void OnBeginDrag(PointerEventData pointerData)
    {
        WindowInventoryItem itemHover = pointerData.pointerEnter.GetComponent<WindowInventoryItem>();

        if (itemHover != null)
        {
            draggingObj = itemHover.gameObject;
            draggingRect = draggingObj.GetComponent<RectTransform>();
            startingPos = draggingRect.anchoredPosition;
            draggingItem = itemHover.item;

            draggingObj.GetComponent<Image>().raycastTarget = false;
            isDragging = true;
        }
    }

    /// <summary>
    ///     Called during mouse drag
    /// </summary>
    /// <param name="pointerData"></param>
    public new void OnDrag(PointerEventData pointerData)
    {
        if (isDragging)
        {
            draggingRect.anchoredPosition += pointerData.delta / GameManager.Instance.canvas.scaleFactor;

            HighlightArea(pointerData);
        }
    }

    /// <summary>
    ///     Called at end of mouse drag
    /// </summary>
    /// <param name="pointerData"></param>
    public void OnEndDrag(PointerEventData pointerData)
    {
        if (!isDragging)
        {
            return;
        }

        ClearHighlight();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(body.GetComponent<RectTransform>(),
            pointerData.position, pointerData.enterEventCamera, out Vector2 pos);

        // Check if it is within bounds of our inventory window then
        // round it to the nearest cell size
        if (pos.x >= 0 && pos.x <= itemRect.sizeDelta.x && -pos.y >= 0 && -pos.y <= itemRect.sizeDelta.y)
        {
            pos.x = (int)pos.x / (int)slotWidth * slotWidth;
            pos.y = (int)pos.y / (int)slotHeight * slotHeight;
            draggingRect.anchoredPosition = pos;
        }
        else
        {
            draggingRect.anchoredPosition = startingPos;
        }

        draggingObj.GetComponent<Image>().raycastTarget = true;
        isDragging = false;
    }

    /// <summary>
    ///     Finds the slots underneath pointer and sets them active (highlights them).
    /// </summary>
    /// <param name="pointerData"></param>
    /// <returns>True if there is a free space for the item, false otherwise.</returns>
    public bool HighlightArea(PointerEventData pointerData)
    {
        ClearHighlight();

        GraphicRaycaster rayCaster = GameManager.Instance.canvas.GetComponent<GraphicRaycaster>();
        List<RaycastResult> hits = new List<RaycastResult>();

        // Create a new pointer data for our raycast manipulation
        PointerEventData newPointerData = new PointerEventData(GetComponent<EventSystem>());
        Vector2 tempPos = pointerData.position;
        // Random offset of slotHeight for reasons unknown? Seems to work
        tempPos.y += slotHeight;
        
        // Loop through item dimensions and raycast below object to get slots
        for (int i = 0; i < draggingItem.widthInGrid; i++)
        {
            for (int j = 0; j < draggingItem.heightInGrid; j++)
            {
                tempPos.y -= slotHeight;
                newPointerData.position = tempPos;
                rayCaster.Raycast(newPointerData, hits);  
            }

            tempPos.x += slotWidth;
            tempPos.y = pointerData.position.y + slotHeight;
        }     

        // Set the highlight objects active and add to list if they exist.
        foreach (RaycastResult hit in hits)
        {
            if(hit.gameObject.transform.Find("Overlay") != null)
            {
                GameObject obj = hit.gameObject.transform.Find("Overlay").gameObject;
                obj.SetActive(true);
                highlightObjs.Add(obj);
            }
        }

        // Checking if there is enough free space below for the item.
        if (highlightObjs.Count == draggingItem.widthInGrid * draggingItem.heightInGrid)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Clears all highlighted areas.
    /// </summary>
    private void ClearHighlight()
    {
        // Reset all the highlighted objects and clear list
        foreach (GameObject obj in highlightObjs)
        {
            obj.SetActive(false);
        }

        highlightObjs.Clear();
    }

    /// <summary>
    ///     Finds the nearest free space for the item, given the dimensions.
    /// </summary>
    /// <param name="item">Item instance</param>
    /// <returns>Vector2 of the free space, (-1, -1) otherwise</returns>
    private Vector2 GetFree(Item item)
    {
        Vector2 point = -Vector2.one;

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
                    point.Set(i, j);
                    return point;
                }
            }
        }

        return point;
    }

    /// <summary>
    ///     Draws the inventory canvas and allocates the usedSlot matrix.
    /// </summary>
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
                Vector2 pos = GetFree(item);

                // Allocate a new prefab and set the item details
                GameObject obj = itemPrefabs.GetFree((item, pos), body.transform.Find("Item Canvas"));
                WindowInventoryItem itemScript = obj.GetComponent<WindowInventoryItem>();
                itemScript.SetItem(item, Math.Min(remainingQuantity, item.stackSizeLimit));
                remainingQuantity -= itemScript.quantity;

                // Change the position and size according to dimensions
                RectTransform rectTransform = obj.GetComponent<RectTransform>();
                // Here, our Y is equal to X in world space, and our -X is Y.
                rectTransform.anchoredPosition = new Vector3(pos.y * slotWidth, -pos.x * slotHeight, 0);
                rectTransform.sizeDelta = new Vector2(item.widthInGrid * slotWidth, item.heightInGrid * slotHeight);

                // Reserve the spaces in the used matrix
                for (int i = (int)pos.x; i < (int)pos.x + item.heightInGrid; i++)
                {
                    for (int j = (int)pos.y; j < (int)pos.y + item.widthInGrid; j++)
                    {
                        isSlotUsed[i][j] = true;
                    }
                } 

                items[item].Add(itemScript);
            }
        }
    }
}