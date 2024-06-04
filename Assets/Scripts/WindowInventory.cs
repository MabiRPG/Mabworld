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

    private GraphicRaycaster raycaster;
    private Camera canvasCamera;

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

        raycaster = GameManager.Instance.canvas.GetComponent<GraphicRaycaster>();
        canvasCamera = GameManager.Instance.canvas.GetComponent<Canvas>().worldCamera;
        
        GameObject obj = Instantiate(itemTooltipPrefab, transform.parent);
        tooltip = obj.GetComponent<WindowInventoryItemTooltip>();

        itemPrefabs = ScriptableObject.CreateInstance<PrefabManager>();
        itemPrefabs.SetPrefab(itemPrefab);

        // Requires an empty gameobject under body to insert into, since the pivot position of the
        // body is determined by a layout group, giving incorrect size deltas.
        itemRect = body.transform.Find("Item Canvas").GetComponent<RectTransform>();
        slotWidth = itemRect.sizeDelta.x / Player.Instance.inventory.Width;
        slotHeight = itemRect.sizeDelta.y / Player.Instance.inventory.Height;

        // Creating the background of the inventory
        for (int i = 0; i < Player.Instance.inventory.Height; i++)
        {
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
            ClearHighlight();
            draggingRect.anchoredPosition += pointerData.delta / GameManager.Instance.canvas.scaleFactor;
            CheckSlotsFree(draggingItem, pointerData.position, true);
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

        if (CheckSlotsFree(draggingItem, pointerData.position, false))
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                body.GetComponent<RectTransform>(), pointerData.position, 
                canvasCamera, out Vector2 pos);  

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
    ///     Clears all highlighted areas.
    /// </summary>
    private void ClearHighlight()
    {
        // Reset all the highlighted objects and clear list
        foreach (GameObject slot in highlightObjs)
        {
            slot.transform.Find("Overlay").gameObject.SetActive(false);
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
        // Find the starting point for our raycasts through the rect transform
        RectTransform rectTransform = body.transform.GetComponent<RectTransform>();
        Vector2 pos = canvasCamera.WorldToScreenPoint(rectTransform.position);
        // Assign loop counters and full dimensions of the inventory space
        //int i = 0;
        //int j = 0;
        float fullWidth = rectTransform.sizeDelta.x / slotWidth;
        float fullHeight = rectTransform.sizeDelta.y / slotHeight;

        for (int i = 0; i < fullHeight - item.heightInGrid + 1; i++)
        {
            for (int j = 0; j < fullWidth - item.widthInGrid + 1; j++)
            {
                // Create a new screenpoint vector for our raycast function with a 
                // 0.5 offset to center onto slot
                float nx = pos.x + (j + 0.5f) * slotWidth;
                float ny = pos.y - (i + 0.5f) * slotHeight;

                // Checks if the slots are free, if so, convert the screenpoint back to local
                // for our item's anchored position later.
                if (CheckSlotsFree(item, new Vector2(nx, ny), false))
                {
                    // Remove offset to position item sprite correctly
                    float ox = nx - 0.5f * slotWidth;
                    float oy = ny + 0.5f * slotHeight;

                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        body.GetComponent<RectTransform>(), new Vector2(ox, oy), 
                        canvasCamera, out pos);      

                    return pos;             
                }
            }
        }

        return -Vector2.one;
    }

    // TODO : Minimize number of raycasts per object by cacheing results in an array.
    private bool CheckSlotsFree(Item item, Vector2 pos, bool highlightArea)
    {
        // Slots encountered by raycasting
        List<GameObject> slots = new List<GameObject>();
        // Stores all the results of our raycasts
        List<RaycastResult> hits = new List<RaycastResult>();
        // Create a new pointer data for our raycast manipulation
        PointerEventData pointerData = new PointerEventData(GetComponent<EventSystem>());

        // Iterate over the area given by the starting pos vector2, moving over slot dimensions
        // and raycasting
        for (int i = 0; i < item.heightInGrid; i++)
        {
            for (int j = 0; j < item.widthInGrid; j++)
            {
                float nx = pos.x + j * slotWidth;
                float ny = pos.y - i * slotHeight;
                pointerData.position = new Vector3(nx, ny);
                raycaster.Raycast(pointerData, hits);
            }
        }

        // Iterates over raycast results, checking if slot exists and if not occupied
        foreach (RaycastResult hit in hits)
        {
            //Debug.Log($"{hit.gameObject.name} {hit.worldPosition}");

            // Encountering an item in space
            if (hit.gameObject.GetComponent<WindowInventoryItem>() != null)
            {
                return false;
            }

            // Otherwise, accumulate inventory slots in space for later processing
            if (hit.gameObject.transform.Find("Overlay"))
            {
                slots.Add(hit.gameObject);
            }
        }

        // Enforcing dimensional requirements here
        if (slots.Count != item.widthInGrid * item.heightInGrid)
        {
            return false;
        }

        // Turn on new highlights
        if (highlightArea)
        {
            foreach (GameObject slot in slots)
            {
                slot.transform.Find("Overlay").gameObject.SetActive(true);
                highlightObjs.Add(slot);
            }
        }

        return true;
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

                // If no space can be found
                if (pos == -Vector2.one)
                {
                    break;
                }

                // Allocate a new prefab and set the item details
                GameObject obj = itemPrefabs.GetFree((item, pos), body.transform.Find("Item Canvas"));
                WindowInventoryItem itemScript = obj.GetComponent<WindowInventoryItem>();
                itemScript.SetItem(item, Math.Min(remainingQuantity, item.stackSizeLimit));
                remainingQuantity -= itemScript.quantity;

                // Change the position and size according to dimensions
                RectTransform rectTransform = obj.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = pos;
                rectTransform.sizeDelta = new Vector2(item.widthInGrid * slotWidth, item.heightInGrid * slotHeight);

                items[item].Add(itemScript);
            }
        }
    }
}