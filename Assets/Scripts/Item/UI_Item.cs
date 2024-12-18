using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IItemPickupHandler
{
    public void HandleItemPickup(
        List<RaycastResult> graphicHits,
        RaycastHit2D sceneHits,
        UI_Item item
    );
}

public interface IItemDropHandler
{
    public void HandleItemDrop(
        List<RaycastResult> graphicHits,
        RaycastHit2D sceneHits,
        UI_Item item
    );
}

public interface IItemHoverHandler
{
    public void HandleItemHover(
        List<RaycastResult> graphicHits,
        RaycastHit2D sceneHits,
        UI_Item item
    );
}

/// <summary>
///     Handles all individual item sprites in a window.
/// </summary>
public class UI_Item : MonoBehaviour, IMouseInputHandler, IMouseExitHandler
{
    public InventoryItem inventoryItem;
    public Item item;
    public int quantity;

    public bool canPickup = false;
    public bool canSplit = false;

    private Image icon;
    private TMP_Text quantityText;
    private RectTransform rectTransform;
    private Canvas gameCanvas;

    [SerializeField]
    private GameObject itemTooltipPrefab;
    private UI_ItemTooltip tooltip;

    [SerializeField]
    private GameObject splitStackPrefab;
    private WindowInventorySplitStack splitStack;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    private void Awake()
    {
        icon = GetComponent<Image>();
        quantityText = transform.Find("Quantity").GetComponent<TMP_Text>();
        rectTransform = GetComponent<RectTransform>();
        gameCanvas = GameManager.Instance.canvas;
    }

    /// <summary>
    ///     Assigns the item.
    /// </summary>
    /// <param name="inventoryItem">Item instance.</param>
    /// <param name="quantity">Quantity of item.</param>
    public void SetItem(InventoryItem inventoryItem, int quantity)
    {
        this.inventoryItem = inventoryItem;
        item = inventoryItem.item;
        this.quantity = quantity;
        icon.sprite = item.model.icon;
        quantityText.text = quantity.ToString();
    }

    public void SetItem(InventoryItem inventoryItem, string text)
    {
        this.inventoryItem = inventoryItem;
        item = inventoryItem.item;
        icon.sprite = item.model.icon;
        quantityText.text = text;
    }

    public void SetItem(Item item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
        icon.sprite = item.model.icon;
        quantityText.text = quantity.ToString();
    }

    public void SetItem(Item item, string text)
    {
        this.item = item;
        icon.sprite = item.model.icon;
        quantityText.text = text;
    }

    public void SetPosition(Vector2 position)
    {
        rectTransform.localPosition = position / gameCanvas.scaleFactor;
    }

    public void SetParent(Transform transform)
    {
        rectTransform.SetParent(transform);
        rectTransform.SetAsLastSibling();
    }

    private bool PickedUp()
    {
        return InputController.Instance.GetActiveItem() == this;
    }

    private void Update()
    {
        if (PickedUp())
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gameCanvas.GetComponent<RectTransform>(),
                Input.mousePosition,
                gameCanvas.GetComponent<Canvas>().worldCamera,
                out Vector2 pos
            );

            pos.x -= InventoryManager.slotWidth / 2;
            pos.y += InventoryManager.slotHeight / 2;

            rectTransform.localPosition = pos / gameCanvas.scaleFactor;
        }
    }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (WindowManager.Instance.MouseHovering())
        {
            if (InputController.Instance.GetActiveItem() == null)
            {
                if (tooltip == null)
                {
                    GameObject obj = Instantiate(itemTooltipPrefab, gameCanvas.transform);
                    tooltip = obj.GetComponent<UI_ItemTooltip>();
                    tooltip.SetItem(inventoryItem.item);
                }

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    gameCanvas.GetComponent<RectTransform>(),
                    graphicHits[0].screenPosition,
                    gameCanvas.GetComponent<Canvas>().worldCamera,
                    out Vector2 pos);

                pos.x -= 5;
                pos.y += 5;

                tooltip.SetPosition(pos);
            }
            else
            {
                CallItemHoverHandlers(graphicHits, sceneHits);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (tooltip != null)
            {
                tooltip.Clear();
                tooltip = null;
            }

            if (canPickup && !PickedUp() &&
                InputController.Instance.GetActiveItem() == null)
            {
                if (canSplit && Input.GetKey(KeyCode.LeftShift) && quantity > 1)
                {
                    GameObject obj = Instantiate(splitStackPrefab, gameCanvas.transform);
                    splitStack = obj.GetComponent<WindowInventorySplitStack>();

                    Action<int> onSplitAction = splitQuantity =>
                    {
                        inventoryItem.quantity -= splitQuantity;
                        SetItem(inventoryItem, inventoryItem.quantity);

                        GameObject clonedItem = Instantiate(gameObject, gameCanvas.transform);
                        UI_Item closeduiItem = clonedItem.GetComponent<UI_Item>();
                        InventoryItem clonedInventoryItem = new InventoryItem(item, splitQuantity,
                            -1, -1);
                        closeduiItem.SetItem(clonedInventoryItem, splitQuantity);
                        closeduiItem.StartPickup();
                    };

                    splitStack.SetItem(this, onSplitAction);
                }
                else
                {
                    CallItemPickupHandlers(graphicHits, sceneHits);
                }
            }
            else if (PickedUp())
            {
                CallItemDropHandlers(graphicHits, sceneHits);
            }
        }
    }

    public void HandleMouseExit(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (tooltip != null)
        {
            tooltip.Clear();
            tooltip = null;
        }
    }

    private void CallItemPickupHandlers(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (WindowManager.Instance.GetWindowHit(graphicHits, out Window foundWindow))
        {
            IItemPickupHandler[] handlers = foundWindow.gameObject.GetComponents<IItemPickupHandler>();

            foreach (IItemPickupHandler handler in handlers)
            {
                handler.HandleItemPickup(graphicHits, sceneHits, this);
            }
        }
    }

    public void StartPickup()
    {
        SetParent(gameCanvas.GetComponent<RectTransform>());
        InputController.Instance.SetActiveItem(this);
    }

    private void CallItemDropHandlers(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (WindowManager.Instance.GetWindowHit(graphicHits, out Window foundWindow))
        {
            IItemDropHandler[] handlers = foundWindow.gameObject.GetComponents<IItemDropHandler>();

            foreach (IItemDropHandler handler in handlers)
            {
                handler.HandleItemDrop(graphicHits, sceneHits, this);
            }
        }
    }

    public void EndPickup()
    {
        InputController.Instance.SetActiveItem(null);
    }

    private void CallItemHoverHandlers(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (WindowManager.Instance.GetWindowHit(graphicHits, out Window foundWindow))
        {
            IItemHoverHandler[] handlers = foundWindow.gameObject.GetComponents<IItemHoverHandler>();

            foreach (IItemHoverHandler handler in handlers)
            {
                handler.HandleItemHover(graphicHits, sceneHits, this);
            }
        }
    }
}