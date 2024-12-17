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

/// <summary>
///     Handles all individual item sprites in a window.
/// </summary>
public class UI_Item : MonoBehaviour, IMouseInputHandler, IMouseExitHandler
{
    public InventoryItem inventoryItem;
    public Item item;
    public int quantity;

    private bool canPickup = false;

    private Image icon;
    private TMP_Text quantityText;
    private RectTransform rectTransform;
    private Canvas gameCanvas;

    private UI_ItemTooltip tooltip;

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

    public void SetCanPickup(bool canPickup)
    {
        this.canPickup = canPickup;
    }

    private bool PickedUp()
    {
        return InputController.Instance.GetActiveItem() == this;
    }

    private void Update()
    {
        if (PickedUp())
        {
            HandleItemMove();
        }
    }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        // Debug.Log("handling mouse");

        if (WindowManager.Instance.MouseHovering())
        {
            if (InputController.Instance.GetActiveItem() == null)
            {
                if (tooltip == null)
                {
                    GameObject obj = Instantiate(
                        GameManager.Instance.itemTooltipPrefab,
                        gameCanvas.transform);
                    tooltip = obj.GetComponent<UI_ItemTooltip>();
                    tooltip.SetItem(inventoryItem.item);
                }

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    gameCanvas.GetComponent<RectTransform>(),
                    Input.mousePosition,
                    gameCanvas.GetComponent<Canvas>().worldCamera,
                    out Vector2 pos);

                pos.x -= 5;
                pos.y += 5;

                tooltip.SetPosition(pos);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (tooltip != null)
            {
                tooltip.Clear();
                tooltip = null;
            }

            if (PickedUp())
            {
                HandleItemDrop(graphicHits, sceneHits);
            }
            else if (
                canPickup &&
                !PickedUp() &&
                InputController.Instance.GetActiveItem() == null)
            {
                HandleItemPickup(graphicHits, sceneHits);
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

    private void HandleItemPickup(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        // Debug.Log("start");

        if (WindowManager.Instance.GetWindowHit(graphicHits, out Window foundWindow))
        {
            IItemPickupHandler[] handlers = foundWindow.gameObject.GetComponents<IItemPickupHandler>();

            foreach (IItemPickupHandler handler in handlers)
            {
                handler.HandleItemPickup(graphicHits, sceneHits, this);
            }
        }
    }

    public void Pickup()
    {
        SetParent(gameCanvas.GetComponent<RectTransform>());
        InputController.Instance.SetActiveItem(this);
    }

    private void HandleItemMove()
    {
        // Debug.Log("moving");

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

    private void HandleItemDrop(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        Debug.Log("stop");

        if (WindowManager.Instance.GetWindowHit(graphicHits, out Window foundWindow))
        {
            IItemDropHandler[] handlers = foundWindow.gameObject.GetComponents<IItemDropHandler>();

            foreach (IItemDropHandler handler in handlers)
            {
                handler.HandleItemDrop(graphicHits, sceneHits, this);
            }
        }
    }

    public void Drop()
    {
        InputController.Instance.SetActiveItem(null);
    }
}