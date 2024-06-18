using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MovableItem
{
    public InventoryItem inventoryItem;
    public WindowInventoryItem windowItem;
    private Transform originTransform;
    public Item item;
    private RectTransform rectTransform;
    private Image image;

    public MovableItem(InventoryItem inventoryItem, WindowInventoryItem windowItem, Transform originTransform)
    {
        this.inventoryItem = inventoryItem;
        this.windowItem = windowItem;
        this.originTransform = originTransform;
        item = inventoryItem.item;
        rectTransform = windowItem.gameObject.GetComponent<RectTransform>();
        image = windowItem.gameObject.GetComponent<Image>();
    }

    public void Begin()
    {
        rectTransform.SetParent(GameManager.Instance.canvas.GetComponent<RectTransform>());
        rectTransform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    public void Move(float x, float y)
    {
        rectTransform.localPosition = new Vector2(x, y) / GameManager.Instance.canvas.scaleFactor;
    }

    public void End()
    {
        rectTransform.SetParent(originTransform);
        rectTransform.localPosition = Vector2.zero;
        image.raycastTarget = true;
    }
}

/// <summary>
///     Handles all window inventory processing.
/// </summary>
public class WindowInventory : Window, IPointerMoveHandler, IPointerExitHandler
{
    public static WindowInventory Instance = null;

    // Prefabs for the slot background objects
    [SerializeField]
    private GameObject slotBackgroundPrefab;

    // Dimensions of the cell width in the item canvas
    private RectTransform itemCanvasRect;
    private float slotWidth;
    private float slotHeight;
    
    // Prefabs for the actual item sprites
    [SerializeField]
    private GameObject itemPrefab;
    // Tooltip on hover over item
    [SerializeField]
    private GameObject itemTooltipPrefab;
    private WindowInventoryItemTooltip tooltip;
    [SerializeField]
    private GameObject splitStackPrefab;
    private WindowInventorySplitStack splitStack;

    // Dictionary of all items for quick reference
    private PrefabManager itemPrefabs;

    // For dragging and dropping items around...
    // Slot background objects that are currently highlighted
    private List<GameObject> highlightObjs = new List<GameObject>();
    private MovableItem movableItem;
    private bool isMovingItem;

    private GraphicRaycaster raycaster;
    private Camera canvasCamera;

    public InventoryBag bag;

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

        obj = Instantiate(splitStackPrefab, transform.parent);
        splitStack = obj.GetComponent<WindowInventorySplitStack>();

        itemPrefabs = ScriptableObject.CreateInstance<PrefabManager>();
        itemPrefabs.SetPrefab(itemPrefab);

        // Requires an empty gameobject under body to insert into, since the pivot position of the
        // body is determined by a layout group, giving incorrect size deltas.
        itemCanvasRect = body.transform.Find("Item Canvas").GetComponent<RectTransform>();

        slotWidth = InventoryManager.slotWidth;
        slotHeight = InventoryManager.slotHeight;
        bag = Player.Instance.inventory.Bags[0];

        itemCanvasRect.sizeDelta = new Vector2(bag.width * slotWidth, bag.height * slotHeight);

        // Creating the background of the inventory
        for (int i = 0; i < bag.height; i++)
        {
            for (int j = 0; j < bag.width; j++)
            {
                if (!bag.excludedSlots.Contains((i, j)))
                {
                    obj = Instantiate(slotBackgroundPrefab, itemCanvasRect);
                    RectTransform transform = obj.GetComponent<RectTransform>();
                    transform.sizeDelta = new Vector2(slotWidth, slotHeight);
                    transform.anchoredPosition = new Vector2(j * slotWidth, -i * slotHeight);
                }
            }
        }

        // Pushing item canvas to last so it appears on top of grid
        itemCanvasRect.SetAsLastSibling();

        // Resets the content size fitter.
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)gameObject.transform);

        // Hides the object at start
        gameObject.SetActive(false);
    }

    /// <summary>
    ///     Called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        bag.changeEvent.OnChange += Draw;
        Draw();
    }

    /// <summary>
    ///     Called when the object becomes disabled and inactive.
    /// </summary>
    private void OnDisable()
    {
        bag.changeEvent.OnChange -= Draw;
    }

    /// <summary>
    ///     Called on every frame.
    /// </summary>
    public void Update()
    {
        // If mouse click, check the item hold state and update
        if (Input.GetMouseButtonDown(0))
        {
            // If we're not holding, then we pick up
            if (!isMovingItem)
            {
                OnItemClick();
            }
            // If we are holding, drop & delete or place back into inventory if possible.
            else
            {
                OnItemDrop();
            }
        }
        // Else if the mouse is moving, move the held item if holding
        else
        {
            if (isMovingItem)
            {
                OnItemMove();
            }
        }
    }

    /// <summary>
    ///     Called when an item is clicked for the first time.
    /// </summary>
    /// <param name="hits"></param>
    private void OnItemClick()
    {
        (int row, int column) = ConvertScreenPointToBagPoint();
        InventoryItem inventoryItem = bag.RemoveItemAt(row, column);

        if (inventoryItem == null)
        {
            return;
        }

        GameObject obj = itemPrefabs.prefabs[inventoryItem];

        movableItem = new MovableItem(
            inventoryItem,
            obj.GetComponent<WindowInventoryItem>(),
            body.transform.Find("Item Canvas")
        );

        movableItem.Begin();
        isMovingItem = true;
    }

    /// <summary>
    ///     Called when an item is moved after clicking.
    /// </summary>
    private void OnItemMove()
    {
        ClearHighlight();
        SetHighlight();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GameManager.Instance.canvas.GetComponent<RectTransform>(), Input.mousePosition,
            canvasCamera, out Vector2 pos);
        pos.x -= slotWidth / 2;
        pos.y += slotHeight / 2;

        movableItem.Move(pos.x, pos.y);
    }

    /// <summary>
    ///     Called when an item is released from the cursor.
    /// </summary>
    /// <param name="hits"></param>
    private void OnItemDrop()
    {
        ClearHighlight();    

        // Stores all the results of our raycasts
        List<RaycastResult> hits = new List<RaycastResult>();
        // Create a new pointer data for our raycast manipulation
        PointerEventData pointerData = new PointerEventData(GetComponent<EventSystem>());
        pointerData.position = Input.mousePosition;
        // Raycast for any windows underneath
        raycaster.Raycast(pointerData, hits);

        // Dropped outside of any window, removes the item from inventory and clears cursor.
        if (hits.Count == 0)
        {
            // Restores the transform to the inventory window and sets inactive
            movableItem.End();
            movableItem.windowItem.gameObject.SetActive(false);
            // Removes from window inventory and reduces quantity on inventory side
            // TODO : Remove from master inventory...
            isMovingItem = false;      
            return;             
        }

        (int row, int column) = ConvertScreenPointToBagPoint();

        if (bag.InsertItemAt(movableItem.inventoryItem, row, column))
        {
            // Restores the transform to the inventory window to align our item again
            movableItem.End();
            movableItem.Move(column * slotWidth, -row * slotHeight);
            isMovingItem = false;
        }
        else
        {
            movableItem.Begin();
        }
    }

    private void SetHighlight()
    {
        // Slots encountered by raycasting
        List<GameObject> slots = new List<GameObject>();
        // Stores all the results of our raycasts
        List<RaycastResult> hits = new List<RaycastResult>();
        // Create a new pointer data for our raycast manipulation
        PointerEventData pointerData = new PointerEventData(GetComponent<EventSystem>());

        // Iterate over the area given by the starting pos vector2, moving over slot dimensions
        // and raycasting
        for (int i = 0; i < movableItem.item.heightInGrid; i++)
        {
            for (int j = 0; j < movableItem.item.widthInGrid; j++)
            {
                float nx = Input.mousePosition.x + j * slotWidth;
                float ny = Input.mousePosition.y - i * slotHeight;
                pointerData.position = new Vector3(nx, ny);
                raycaster.Raycast(pointerData, hits);
            }
        }

        foreach (RaycastResult hit in hits)
        {
            if (hit.gameObject.transform.Find("Overlay") != null)
            {
                slots.Add(hit.gameObject.transform.Find("Overlay").gameObject);
            }
        }

        // Enforcing dimensional requirements here
        if (slots.Count != movableItem.item.widthInGrid * movableItem.item.heightInGrid)
        {
            return;
        }

        foreach (GameObject slot in slots)
        {
            slot.SetActive(true);
            highlightObjs.Add(slot);
        }
    }

    /// <summary>
    ///     Clears all highlighted areas.
    /// </summary>
    private void ClearHighlight()
    {
        // Reset all the highlighted objects and clear list
        foreach (GameObject slot in highlightObjs)
        {
            slot.SetActive(false);
        }

        highlightObjs.Clear();
    }

    private (int row, int column) ConvertScreenPointToBagPoint()
    {
        Vector2 pos = Input.mousePosition;
        // Converts our screen point of our mouse cursor to a local point
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            body.transform.Find("Item Canvas").GetComponent<RectTransform>(), pos, 
            canvasCamera, out pos);  
        return (-(int)pos.y / (int)slotWidth, (int)pos.x / (int)slotHeight);        
    }

    /// <summary>
    ///     Called when the mouse pointer moves inside the object.
    /// </summary>
    /// <param name="pointerData"></param>
    public void OnPointerMove(PointerEventData pointerData)
    {
        if (isMovingItem)
        {
            tooltip.Clear();
            return;
        }

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
    ///     Draws the inventory canvas.
    /// </summary>
    private void Draw()
    {
        List<InventoryItem> itemsAdded = new List<InventoryItem>();

        foreach (InventoryItem inventoryItem in bag.items.Values)
        {
            if (itemsAdded.Contains(inventoryItem))
            {
                continue;
            }

            GameObject obj = itemPrefabs.GetFree(inventoryItem, body.transform.Find("Item Canvas"));
            WindowInventoryItem windowItem = obj.GetComponent<WindowInventoryItem>();
            windowItem.SetItem(inventoryItem.item, inventoryItem.quantity);

            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(inventoryItem.width * slotWidth, 
                inventoryItem.height * slotHeight);
            rectTransform.anchoredPosition = new Vector2(
                inventoryItem.origin.column * slotWidth, 
                -inventoryItem.origin.row * slotHeight
            );

            itemsAdded.Add(inventoryItem); 
        }
    }
}