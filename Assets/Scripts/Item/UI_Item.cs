using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
///     Handles all individual item sprites in a window.
/// </summary>
public class UI_Item : MonoBehaviour, IMouseInputHandler, IMouseExitHandler
{
    public Item item;
    public int quantity;
    private bool movable;

    private Image icon;
    private TMP_Text quantityText;

    private UI_ItemTooltip tooltip;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    private void Awake()
    {
        icon = GetComponent<Image>();
        quantityText = transform.Find("Quantity").GetComponent<TMP_Text>();
    }

    /// <summary>
    ///     Assigns the item.
    /// </summary>
    /// <param name="item">Item instance.</param>
    /// <param name="quantity">Quantity of item.</param>
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

    public void SetMovable(bool movable)
    {
        this.movable = movable;
    }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (WindowManager.Instance.MouseHovering())
        {
            if (tooltip == null)
            {
                GameObject obj = Instantiate(GameManager.Instance.itemTooltipPrefab,
                    GameManager.Instance.canvas.transform);
                tooltip = obj.GetComponent<UI_ItemTooltip>();
                tooltip.SetItem(item);
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                GameManager.Instance.canvas.GetComponent<RectTransform>(),
                graphicHits[0].screenPosition,
                GameManager.Instance.canvas.GetComponent<Canvas>().worldCamera,
                out Vector2 pos);

            pos.x -= 5;
            pos.y += 5;

            tooltip.SetPosition(pos);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (tooltip != null)
            {
                tooltip.Clear();
                tooltip = null;
            }

            if (movable)
            {
                RectTransform rectTransform = GetComponent<RectTransform>();
                rectTransform.SetParent(GameManager.Instance.canvas.GetComponent<RectTransform>());
                rectTransform.SetAsLastSibling();
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
}