using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
///     Handles all individual item sprites in a window.
/// </summary>
public class WindowItem : MonoBehaviour, IInputHandler, IPointerMoveHandler, IPointerExitHandler
{
    public Item item;
    public int quantity;

    private Image icon;
    private TMP_Text quantityText;

    private Camera canvasCamera;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    private void Awake()
    {
        icon = GetComponent<Image>();
        quantityText = transform.Find("Quantity").GetComponent<TMP_Text>();
        canvasCamera = GameManager.Instance.canvas.GetComponent<Canvas>().worldCamera;
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
        icon.sprite = item.icon;
        quantityText.text = quantity.ToString();
    }

    public void SetItem(Item item, string text)
    {
        this.item = item;
        icon.sprite = item.icon;
        quantityText.text = text;
    }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        // if (WindowManager.Instance.MouseHovering())
        // {
        //     WindowItemTooltip.Instance.SetItem(item);
        // }
    }

    public void HandleKeyboardInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        WindowItemTooltip.Instance.SetItem(item);

        RectTransform canvasRect = GameManager.Instance.canvas.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, canvasCamera, out Vector2 pos);
        pos.x -= 5;
        pos.y += 5;

        WindowItemTooltip.Instance.rectTransform.anchoredPosition = pos;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        WindowItemTooltip.Instance.Clear();
    }
}