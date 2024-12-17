using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
///     Handles all window inventory processing.
/// </summary>
public class WindowInventory : Window, IMouseInputHandler, IItemPickupHandler, IItemDropHandler
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
    [SerializeField]
    private GameObject splitStackPrefab;
    private WindowInventorySplitStack splitStack;

    // Dictionary of all items for quick reference
    private PrefabFactory itemPrefabs;

    // For dragging and dropping items around...
    // Slot background objects that are currently highlighted
    private List<GameObject> highlightObjs = new List<GameObject>();

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

        GameObject obj = Instantiate(splitStackPrefab, transform.parent);
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
        // tooltip.Clear();
    }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        // // If mouse click, check the item hold state and update
        // if (Input.GetMouseButtonDown(0))
        // {
        //     // If we're not holding, then we pick up
        //     if (!isMovingItem)
        //     {
        //         OnItemClick();
        //     }
        //     // If we are holding, drop & delete or place back into inventory if possible.
        //     else
        //     {
        //         OnItemDrop();
        //     }
        // }
        // // Else if the mouse is moving, move the held item if holding
        // else
        // {
        //     if (isMovingItem)
        //     {
        //         OnItemMove();
        //     }
        // }
    }

    public void HandleItemPickup(
        List<RaycastResult> graphicHits,
        RaycastHit2D sceneHits,
        UI_Item uiItem
    )
    {
        int row = -1;
        int column = -1;

        RectTransform itemCanvasRect = body.transform.Find("Item Canvas").GetComponent<RectTransform>();
        // Checks if its in our rect.
        if (RectTransformUtility.RectangleContainsScreenPoint(
            itemCanvasRect,
            graphicHits[0].screenPosition,
            canvasCamera))
        {
            // Converts our screen point of our mouse cursor to a local point
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                itemCanvasRect,
                graphicHits[0].screenPosition,
                canvasCamera,
                out Vector2 pos);

            (row, column) = (-(int)pos.y / (int)slotWidth, (int)pos.x / (int)slotHeight);
        }

        if (row == -1 || column == -1)
        {
            return;
        }

        InventoryItem inventoryItem = bag.FindItemAt(row, column);

        if (inventoryItem == null)
        {
            return;
        }

        // Detach from the prefab factory to prevent overwrite, remove from bag, and then
        // start pickup.
        itemPrefabs.Remove(uiItem.inventoryItem);
        bag.RemoveItemAt(row, column);
        uiItem.Pickup();
    }

    public void HandleItemDrop(
        List<RaycastResult> graphicHits,
        RaycastHit2D sceneHits,
        UI_Item uiItem
    )
    {
        ClearHighlight();

        int row = -1;
        int column = -1;

        RectTransform itemCanvasRect = body.transform.Find("Item Canvas").GetComponent<RectTransform>();
        // Checks if its in our rect.
        if (RectTransformUtility.RectangleContainsScreenPoint(
            itemCanvasRect,
            graphicHits[0].screenPosition,
            canvasCamera))
        {
            // Converts our screen point of our mouse cursor to a local point
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                itemCanvasRect,
                graphicHits[0].screenPosition,
                canvasCamera,
                out Vector2 pos);

            (row, column) = (-(int)pos.y / (int)slotWidth, (int)pos.x / (int)slotHeight);
        }

        // Debug.Log($"{row}, {column}");

        if (row == -1 || column == -1)
        {
            return;
        }

        if (bag.FindItemAt(row, column) == null)
        {
            uiItem.SetParent(body.transform.Find("Item Canvas"));
            uiItem.SetPosition(new Vector2(column * slotWidth, -row * slotHeight));
            uiItem.Drop();

            // Attach it to the prefab factory, and insert into bag.
            // This order prevents the bag insert from creating new unnecessary prefabs!
            itemPrefabs.Add(uiItem.inventoryItem, uiItem.gameObject);
            bag.InsertItemAt(uiItem.inventoryItem, row, column);
        }

        // If the space is insertable (empty) then insert the item back
        // if (bag.InsertItemAt(movableItem.inventoryItem, row, column))
        // {
        //     // Restores the transform to the inventory window to align our item again
        //     movableItem.End();
        //     movableItem.Move(column * slotWidth, -row * slotHeight);
        //     isMovingItem = false;
        // }
        // // Otherwise, check if there is exactly one item underneath, then swap
        // else if (bag.CountItemsAt(row, column,
        //     movableItem.item.model.widthInGrid, movableItem.item.model.heightInGrid) == 1)
        // {
        //     // Find the item underneath and remove it
        //     InventoryItem itemFound = bag.FindItemsAt(
        //         row, column,
        //         row + movableItem.item.model.heightInGrid,
        //         column + movableItem.item.model.widthInGrid
        //     )[0];

        //     GameObject obj;

        //     // Check if we can recombine the stacks assuming the same item
        //     if (itemFound.item == movableItem.inventoryItem.item
        //         && itemFound.quantity < itemFound.item.model.stackSizeLimit)
        //     {
        //         int diff = Math.Min(itemFound.quantity + movableItem.inventoryItem.quantity,
        //             itemFound.item.model.stackSizeLimit);
        //         diff -= itemFound.quantity;

        //         itemFound.quantity += diff;
        //         obj = itemPrefabs.prefabs[itemFound];
        //         UI_Item windowItem = obj.GetComponent<UI_Item>();
        //         windowItem.SetItem(itemFound.item, itemFound.quantity);

        //         movableItem.inventoryItem.quantity -= diff;
        //         movableItem.windowItem.SetItem(itemFound.item, movableItem.inventoryItem.quantity);

        //         if (movableItem.inventoryItem.quantity == 0)
        //         {
        //             movableItem.windowItem.gameObject.SetActive(false);
        //             movableItem.End();
        //             isMovingItem = false;
        //             return;
        //         }

        //         movableItem.Begin();
        //         isMovingItem = true;
        //     }
        //     else
        //     {
        //         (int i, int j) = itemFound.origin;
        //         InventoryItem inventoryItem = bag.RemoveItemAt(i, j);

        //         // Reinsert our item
        //         bag.InsertItemAt(movableItem.inventoryItem, row, column);
        //         // Restores the transform to the inventory window to align our item again
        //         movableItem.End();
        //         movableItem.Move(column * slotWidth, -row * slotHeight);

        //         // Create a new movable item and restart
        //         obj = itemPrefabs.prefabs[inventoryItem];
        //         movableItem = new MovableItem(
        //             inventoryItem,
        //             obj.GetComponent<UI_Item>(),
        //             body.transform.Find("Item Canvas")
        //         );

        //         movableItem.Begin();
        //         isMovingItem = true;
        //     }
        // }
        // else
        // {
        //     movableItem.Begin();
        // }
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
        UI_Item windowItem = obj.GetComponent<UI_Item>();

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
        }
    }

    /// <summary>
    ///     Called when an item is split through the split stack window.
    /// </summary>
    /// <param name="inventoryItem"></param>
    /// <param name="windowItem"></param>
    /// <param name="quantity">Quantity of resulting new split stack.</param>
    private void OnItemSplit(InventoryItem inventoryItem, UI_Item windowItem, int quantity)
    {
        // InventoryItem newInventoryItem = new InventoryItem(inventoryItem.item, quantity, -1, -1);

        // GameObject obj = itemPrefabs.GetFree(newInventoryItem, body.transform.Find("Item Canvas"));
        // UI_Item newWindowItem = obj.GetComponent<UI_Item>();
        // newWindowItem.SetItem(inventoryItem.item, quantity);

        // RectTransform rectTransform = obj.GetComponent<RectTransform>();
        // rectTransform.sizeDelta = new Vector2(inventoryItem.width * slotWidth,
        //     inventoryItem.height * slotHeight);

        // movableItem = new MovableItem(newInventoryItem, newWindowItem,
        //     body.transform.Find("Item Canvas"));
        // movableItem.Begin();
        // isMovingItem = true;

        // inventoryItem.quantity -= quantity;
        // windowItem.SetItem(inventoryItem.item, inventoryItem.quantity);

        // if (inventoryItem.quantity == 0)
        // {
        //     bag.RemoveItemAt(inventoryItem.origin.row, inventoryItem.origin.column);
        //     windowItem.gameObject.SetActive(false);
        // }
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
    }

    /// <summary>
    ///     Called when an item is released from the cursor.
    /// </summary>
    /// <param name="hits"></param>
    private void OnItemDrop()
    {
        // ClearHighlight();

        // // Stores all the results of our raycasts
        // List<RaycastResult> hits = new List<RaycastResult>();
        // // Create a new pointer data for our raycast manipulation
        // PointerEventData pointerData = new PointerEventData(GetComponent<EventSystem>());
        // pointerData.position = Input.mousePosition;
        // // Raycast for any windows underneath
        // raycaster.Raycast(pointerData, hits);

        // // Dropped outside of any window, removes the item from inventory and clears cursor.
        // if (hits.Count == 0)
        // {
        //     // Restores the transform to the inventory window and sets inactive
        //     movableItem.End();
        //     movableItem.windowItem.gameObject.SetActive(false);
        //     // Removes from window inventory and reduces quantity on inventory side
        //     // TODO : Remove from master inventory...
        //     isMovingItem = false;
        //     return;
        // }

        // (int row, int column) = ConvertScreenPointToBagPoint();

        // // If the space is insertable (empty) then insert the item back
        // if (bag.InsertItemAt(movableItem.inventoryItem, row, column))
        // {
        //     // Restores the transform to the inventory window to align our item again
        //     movableItem.End();
        //     movableItem.Move(column * slotWidth, -row * slotHeight);
        //     isMovingItem = false;
        // }
        // // Otherwise, check if there is exactly one item underneath, then swap
        // else if (bag.CountItemsAt(row, column,
        //     movableItem.item.model.widthInGrid, movableItem.item.model.heightInGrid) == 1)
        // {
        //     // Find the item underneath and remove it
        //     InventoryItem itemFound = bag.FindItemsAt(
        //         row, column,
        //         row + movableItem.item.model.heightInGrid,
        //         column + movableItem.item.model.widthInGrid
        //     )[0];

        //     GameObject obj;

        //     // Check if we can recombine the stacks assuming the same item
        //     if (itemFound.item == movableItem.inventoryItem.item
        //         && itemFound.quantity < itemFound.item.model.stackSizeLimit)
        //     {
        //         int diff = Math.Min(itemFound.quantity + movableItem.inventoryItem.quantity,
        //             itemFound.item.model.stackSizeLimit);
        //         diff -= itemFound.quantity;

        //         itemFound.quantity += diff;
        //         obj = itemPrefabs.prefabs[itemFound];
        //         UI_Item windowItem = obj.GetComponent<UI_Item>();
        //         windowItem.SetItem(itemFound.item, itemFound.quantity);

        //         movableItem.inventoryItem.quantity -= diff;
        //         movableItem.windowItem.SetItem(itemFound.item, movableItem.inventoryItem.quantity);

        //         if (movableItem.inventoryItem.quantity == 0)
        //         {
        //             movableItem.windowItem.gameObject.SetActive(false);
        //             movableItem.End();
        //             isMovingItem = false;
        //             return;
        //         }

        //         movableItem.Begin();
        //         isMovingItem = true;
        //     }
        //     else
        //     {
        //         (int i, int j) = itemFound.origin;
        //         InventoryItem inventoryItem = bag.RemoveItemAt(i, j);

        //         // Reinsert our item
        //         bag.InsertItemAt(movableItem.inventoryItem, row, column);
        //         // Restores the transform to the inventory window to align our item again
        //         movableItem.End();
        //         movableItem.Move(column * slotWidth, -row * slotHeight);

        //         // Create a new movable item and restart
        //         obj = itemPrefabs.prefabs[inventoryItem];
        //         movableItem = new MovableItem(
        //             inventoryItem,
        //             obj.GetComponent<UI_Item>(),
        //             body.transform.Find("Item Canvas")
        //         );

        //         movableItem.Begin();
        //         isMovingItem = true;
        //     }
        // }
        // else
        // {
        //     movableItem.Begin();
        // }
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
        // for (int i = 0; i < movableItem.item.model.heightInGrid; i++)
        // {
        //     for (int j = 0; j < movableItem.item.model.widthInGrid; j++)
        //     {
        //         float nx = Input.mousePosition.x + j * slotWidth;
        //         float ny = Input.mousePosition.y - i * slotHeight;
        //         pointerData.position = new Vector3(nx, ny);
        //         raycaster.Raycast(pointerData, hits);
        //     }
        // }

        foreach (RaycastResult hit in hits)
        {
            if (hit.gameObject.transform.Find("Overlay") != null)
            {
                slots.Add(hit.gameObject.transform.Find("Overlay").gameObject);
            }
        }

        // Count the number of items under our cursor
        // (int row, int column) = ConvertScreenPointToBagPoint();
        // int itemsHit = bag.CountItemsAt(row, column,
        //     movableItem.item.model.widthInGrid, movableItem.item.model.heightInGrid);

        // // Enforcing dimensional requirements here
        // if (slots.Count != movableItem.item.model.widthInGrid * movableItem.item.model.heightInGrid)
        // {
        //     return;
        // }
        // // Can only swap with exactly one item
        // else if (itemsHit > 1)
        // {
        //     return;
        // }

        // foreach (GameObject slot in slots)
        // {
        //     slot.SetActive(true);
        //     highlightObjs.Add(slot);
        // }
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
    ///     Draws the inventory canvas.
    /// </summary>
    private void Draw()
    {
        itemPrefabs.SetActiveAll(false);

        foreach (InventoryItem inventoryItem in bag.items.Values)
        {
            GameObject obj = itemPrefabs.GetFree(inventoryItem, body.transform.Find("Item Canvas"));
            UI_Item windowItem = obj.GetComponent<UI_Item>();
            windowItem.SetItem(inventoryItem, inventoryItem.quantity);
            windowItem.SetCanPickup(true);

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