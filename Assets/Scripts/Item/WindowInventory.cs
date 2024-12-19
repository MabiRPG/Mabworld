using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
///     Handles all window inventory processing.
/// </summary>
public class WindowInventory : Window, IItemPickupHandler, IItemDropHandler, IItemHoverHandler
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
                    GameObject obj = Instantiate(slotBackgroundPrefab, itemCanvasRect);
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
    }

    public void HandleItemPickup(
        List<RaycastResult> graphicHits,
        RaycastHit2D sceneHits,
        UI_Item uiItem
    )
    {
        (int row, int column) = ConvertScreenPointToBagPoint();

        if (row == -1 || column == -1)
        {
            return;
        }

        // Detach from the prefab factory to prevent overwrite, remove from bag, and then
        // start pickup.
        if (bag.FindItemAt(row, column) == uiItem.inventoryItem)
        {
            itemPrefabs.Remove(uiItem);
            bag.RemoveItemAt(row, column);
            uiItem.StartPickup();
        }
    }

    public void HandleItemDrop(
        List<RaycastResult> graphicHits,
        RaycastHit2D sceneHits,
        UI_Item uiItem
    )
    {
        ClearHighlight();

        (int row, int column) = ConvertScreenPointToBagPoint();

        if (row == -1 || column == -1)
        {
            return;
        }

        // Reinserting into an empty bag slot.
        if (bag.IsEmpty(row, column,
                uiItem.item.model.widthInGrid, uiItem.item.model.heightInGrid))
        {
            uiItem.SetParent(body.transform.Find("Item Canvas"));
            uiItem.SetPosition(new Vector2(column * slotWidth, -row * slotHeight));
            uiItem.EndPickup();

            // Attach it to the prefab factory, and insert into bag.
            // This order prevents the bag insert from creating new unnecessary prefabs!
            itemPrefabs.Add(uiItem, uiItem.gameObject);
            bag.InsertItemAt(uiItem.inventoryItem, row, column);
        }
        // If there is exactly one item in space, handle.
        else if (bag.CountItemsAt(row, column,
            uiItem.item.model.widthInGrid, uiItem.item.model.heightInGrid) == 1)
        {
            List<InventoryItem> itemsFound = bag.FindItemsAt(row, column,
                row + uiItem.item.model.heightInGrid, column + uiItem.item.model.widthInGrid);
            InventoryItem itemFound = itemsFound[0];

            // Merging the same items.
            if (itemFound.item == uiItem.item &&
                itemFound.quantity < itemFound.item.model.stackSizeLimit)
            {
                int diff = Math.Min(itemFound.quantity + uiItem.quantity,
                    itemFound.item.model.stackSizeLimit);
                diff -= itemFound.quantity;

                itemFound.quantity += diff;
                uiItem.quantity -= diff;

                if (uiItem.quantity == 0)
                {
                    uiItem.EndPickup();
                    Destroy(uiItem.gameObject);
                }

                Draw();
            }
            // Swapping different items.
            else if (itemFound.item != uiItem.item)
            {
                // First we move the item back to the canvas, set position, and then
                // end the pickup.
                uiItem.SetParent(body.transform.Find("Item Canvas"));
                uiItem.SetPosition(new Vector2(column * slotWidth, -row * slotHeight));
                uiItem.EndPickup();

                // Next, we cycle through the prefabs to find the item underneath
                // then remove the item and start pickup.
                foreach (UI_Item newItem in itemPrefabs.prefabs.Keys.ToList().Cast<UI_Item>())
                {
                    if (newItem.inventoryItem == itemFound)
                    {
                        itemPrefabs.Remove(newItem);
                        bag.RemoveItemAt(itemFound.origin.row, itemFound.origin.column);
                        newItem.StartPickup();
                        break;
                    }
                }

                // Attach it to the prefab factory, and insert into bag.
                // This order prevents the bag insert from creating new unnecessary prefabs!
                // Finally, now that the bag slot is free by previous step, readd the item 
                // back into the bag!
                itemPrefabs.Add(uiItem, uiItem.gameObject);
                bag.InsertItemAt(uiItem.inventoryItem, row, column);
            }
        }
    }

    public void HandleItemHover(
        List<RaycastResult> graphicHits,
        RaycastHit2D sceneHits,
        UI_Item uiItem
    )
    {
        ClearHighlight();
        SetHighlight(uiItem);
    }

    /// <summary>
    ///     Sets the slot highlight depending on the cursor position.
    /// </summary>
    private void SetHighlight(UI_Item uiItem)
    {
        // Slots encountered by raycasting
        List<GameObject> slots = new List<GameObject>();
        // Stores all the results of our raycasts
        List<RaycastResult> hits = new List<RaycastResult>();
        // Create a new pointer data for our raycast manipulation
        PointerEventData pointerData = new PointerEventData(GetComponent<EventSystem>());

        // Iterate over the area given by the starting pos vector2, moving over slot dimensions
        // and raycasting
        for (int i = 0; i < uiItem.item.model.heightInGrid; i++)
        {
            for (int j = 0; j < uiItem.item.model.widthInGrid; j++)
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
        int itemsHit = bag.CountItemsAt(row, column,
            uiItem.item.model.widthInGrid, uiItem.item.model.heightInGrid);

        // Enforcing dimensional requirements here
        if (slots.Count != uiItem.item.model.widthInGrid * uiItem.item.model.heightInGrid)
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
    ///     Draws the inventory canvas.
    /// </summary>
    private void Draw()
    {
        itemPrefabs.SetActiveAll(false);

        foreach (InventoryItem inventoryItem in bag.items.Values.Distinct())
        {
            GameObject obj = itemPrefabs.GetFree(inventoryItem, body.transform.Find("Item Canvas"));
            UI_Item uiItem = obj.GetComponent<UI_Item>();
            uiItem.SetItem(inventoryItem, inventoryItem.quantity);
            uiItem.canPickup = true;
            uiItem.canSplit = true;
            itemPrefabs.ChangeKey(inventoryItem, uiItem);

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