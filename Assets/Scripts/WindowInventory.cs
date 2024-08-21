using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
///     Handles processing the item being moved by user on mouse click.
/// </summary>
public class MovableItem
{
    public InventoryItem inventoryItem;
    public WindowItem windowItem;
    private Transform originTransform;
    public Item item;
    private RectTransform rectTransform;
    private Image image;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    /// <param name="inventoryItem"></param>
    /// <param name="windowItem"></param>
    /// <param name="originTransform">Starting Transform component of window item.</param>
    public MovableItem(InventoryItem inventoryItem, WindowItem windowItem, Transform originTransform)
    {
        this.inventoryItem = inventoryItem;
        this.windowItem = windowItem;
        this.originTransform = originTransform;
        item = inventoryItem.item;
        rectTransform = windowItem.gameObject.GetComponent<RectTransform>();
        image = windowItem.gameObject.GetComponent<Image>();

        windowItem.gameObject.SetActive(true);
    }

    /// <summary>
    ///     Called before the item moves.
    /// </summary>
    public void Begin()
    {
        rectTransform.SetParent(GameManager.Instance.canvas.GetComponent<RectTransform>());
        rectTransform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    /// <summary>
    ///     Called during the item move.
    /// </summary>
    /// <param name="x">x-coordinate to move item.</param>
    /// <param name="y">y-coordinate to move item.</param>
    public void Move(float x, float y)
    {
        rectTransform.localPosition = new Vector2(x, y) / GameManager.Instance.canvas.scaleFactor;
    }

    /// <summary>
    ///     Called after the item stops.
    /// </summary>
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
    private WindowItemTooltip tooltip;
    [SerializeField]
    private GameObject splitStackPrefab;
    private WindowInventorySplitStack splitStack;

    // Dictionary of all items for quick reference
    private PrefabFactory itemPrefabs;

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
        tooltip = obj.GetComponent<WindowItemTooltip>();

        obj = Instantiate(splitStackPrefab, transform.parent);
        splitStack = obj.GetComponent<WindowInventorySplitStack>();

        itemPrefabs = ScriptableObject.CreateInstance<PrefabFactory>();
        itemPrefabs.SetPrefab(itemPrefab);

        // Requires an empty gameobject under body to insert into, since the pivot position of the
        // body is determined by a layout group, giving incorrect size deltas.
        itemCanvasRect = body.transform.Find("Item Canvas").GetComponent<RectTransform>();

        slotWidth = InventoryManager.slotWidth;
        slotHeight = InventoryManager.slotHeight;
        bag = Player.Instance.inventoryManager.Bags[0];

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
    }

    private void Start()
    {
        // Hides the object at start
        // gameObject.SetActive(false);
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
        tooltip.Clear();
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
        // TODO: Fix this so it only calls when window is selected.
        (int row, int column) = ConvertScreenPointToBagPoint();
        InventoryItem inventoryItem = bag.FindItemAt(row, column);

        if (inventoryItem == null)
        {
            return;
        }

        GameObject obj = itemPrefabs.prefabs[inventoryItem];
        WindowItem windowItem = obj.GetComponent<WindowItem>();

        if (inventoryItem.quantity > 1 && Input.GetKey(KeyCode.LeftShift))
        {
            Action<int> onSplitAction = quantity =>
            {
                OnItemSplit(inventoryItem, windowItem, quantity);
            };

            splitStack.SetItem(windowItem, onSplitAction);
        }
        else
        {
            bag.RemoveItemAt(row, column);
            movableItem = new MovableItem(inventoryItem, windowItem, 
                body.transform.Find("Item Canvas"));

            movableItem.Begin();
            isMovingItem = true;
        }
    }

    /// <summary>
    ///     Called when an item is split through the split stack window.
    /// </summary>
    /// <param name="inventoryItem"></param>
    /// <param name="windowItem"></param>
    /// <param name="quantity">Quantity of resulting new split stack.</param>
    private void OnItemSplit(InventoryItem inventoryItem, WindowItem windowItem, int quantity)
    {
        InventoryItem newInventoryItem = new InventoryItem(inventoryItem.item, quantity, -1, -1);
        
        GameObject obj = itemPrefabs.GetFree(newInventoryItem, body.transform.Find("Item Canvas"));
        WindowItem newWindowItem = obj.GetComponent<WindowItem>();
        newWindowItem.SetItem(inventoryItem.item, quantity);

        RectTransform rectTransform = obj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(inventoryItem.width * slotWidth, 
            inventoryItem.height * slotHeight);

        movableItem = new MovableItem(newInventoryItem, newWindowItem, 
            body.transform.Find("Item Canvas"));
        movableItem.Begin();
        isMovingItem = true;

        inventoryItem.quantity -= quantity;
        windowItem.SetItem(inventoryItem.item, inventoryItem.quantity);

        if (inventoryItem.quantity == 0)
        {
            bag.RemoveItemAt(inventoryItem.origin.row, inventoryItem.origin.column);
            windowItem.gameObject.SetActive(false);
        }
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

        // If the space is insertable (empty) then insert the item back
        if (bag.InsertItemAt(movableItem.inventoryItem, row, column))
        {
            // Restores the transform to the inventory window to align our item again
            movableItem.End();
            movableItem.Move(column * slotWidth, -row * slotHeight);
            isMovingItem = false;
        }
        // Otherwise, check if there is exactly one item underneath, then swap
        else if (bag.CountItemsAt(row, column, movableItem.item.widthInGrid, movableItem.item.heightInGrid) == 1)
        {
            // Find the item underneath and remove it
            InventoryItem itemFound = bag.FindItemsAt(
                row, column,
                row + movableItem.item.heightInGrid,
                column + movableItem.item.widthInGrid
            )[0];

            GameObject obj;

            // Check if we can recombine the stacks assuming the same item
            if (itemFound.item == movableItem.inventoryItem.item && itemFound.quantity < itemFound.item.stackSizeLimit)
            {
                int diff = Math.Min(itemFound.quantity + movableItem.inventoryItem.quantity, itemFound.item.stackSizeLimit);
                diff -= itemFound.quantity;

                itemFound.quantity += diff;
                obj = itemPrefabs.prefabs[itemFound];
                WindowItem windowItem = obj.GetComponent<WindowItem>();
                windowItem.SetItem(itemFound.item, itemFound.quantity);

                movableItem.inventoryItem.quantity -= diff;
                movableItem.windowItem.SetItem(itemFound.item, movableItem.inventoryItem.quantity);

                if (movableItem.inventoryItem.quantity == 0)
                {
                    movableItem.windowItem.gameObject.SetActive(false);
                    movableItem.End();
                    isMovingItem = false;
                    return;
                }

                movableItem.Begin();
                isMovingItem = true;
            }
            else
            {
                (int i, int j) = itemFound.origin;
                InventoryItem inventoryItem = bag.RemoveItemAt(i, j);

                // Reinsert our item
                bag.InsertItemAt(movableItem.inventoryItem, row, column);
                // Restores the transform to the inventory window to align our item again
                movableItem.End();
                movableItem.Move(column * slotWidth, -row * slotHeight);

                // Create a new movable item and restart
                obj = itemPrefabs.prefabs[inventoryItem];
                movableItem = new MovableItem(
                    inventoryItem,
                    obj.GetComponent<WindowItem>(),
                    body.transform.Find("Item Canvas")
                );

                movableItem.Begin();
                isMovingItem = true;
            }
        }
        else
        {
            movableItem.Begin();
        }
    }

    /// <summary>
    ///     Sets the slot highlight depending on the cursor position.
    /// </summary>
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

        // Count the number of items under our cursor
        (int row, int column) = ConvertScreenPointToBagPoint();
        int itemsHit = bag.CountItemsAt(row, column, movableItem.item.widthInGrid, movableItem.item.heightInGrid);

        // Enforcing dimensional requirements here
        if (slots.Count != movableItem.item.widthInGrid * movableItem.item.heightInGrid)
        {
            return;
        }
        // Can only swap with exactly one item
        else if (itemsHit > 1)
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

    /// <summary>
    ///     Converts the mouse position to a position in our bag matrix.
    /// </summary>
    /// <returns>Tuple of (row, column) indices.</returns>
    private (int row, int column) ConvertScreenPointToBagPoint()
    {
        Vector2 pos = Input.mousePosition;
        RectTransform itemCanvasRect = body.transform.Find("Item Canvas").GetComponent<RectTransform>();
        // Checks if its in our rect.
        if (RectTransformUtility.RectangleContainsScreenPoint(itemCanvasRect, pos, canvasCamera))
        {
            // Converts our screen point of our mouse cursor to a local point
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                itemCanvasRect, pos, canvasCamera, out pos);

            return (-(int)pos.y / (int)slotWidth, (int)pos.x / (int)slotHeight);
        }

        return (-1, -1);     
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

        WindowItem itemHover = pointerData.pointerEnter.GetComponent<WindowItem>();

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
        itemPrefabs.SetActiveAll(false);

        foreach (InventoryItem inventoryItem in bag.items.Values)
        {
            GameObject obj = itemPrefabs.GetFree(inventoryItem, body.transform.Find("Item Canvas"));
            WindowItem windowItem = obj.GetComponent<WindowItem>();
            windowItem.SetItem(inventoryItem.item, inventoryItem.quantity);

            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(inventoryItem.width * slotWidth, 
                inventoryItem.height * slotHeight);
            rectTransform.anchoredPosition = new Vector2(
                inventoryItem.origin.column * slotWidth, 
                -inventoryItem.origin.row * slotHeight
            );
        }
    }
}