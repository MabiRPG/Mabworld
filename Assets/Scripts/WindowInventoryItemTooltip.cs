using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
///     Handles rendering the item tooltip when mousing over an item.
/// </summary>
public class WindowInventoryItemTooltip : MonoBehaviour
{
    public WindowInventoryItemTooltip Instance { get; private set; }

    private TMP_Text itemName;
    private TMP_Text description;
    private TMP_Text itemStackSize;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private GraphicRaycaster raycaster;
    private Camera canvasCamera;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }        

        itemName = transform.Find("Item Name").GetComponent<TMP_Text>();
        description = transform.Find("Description Parent/Description").GetComponent<TMP_Text>();
        itemStackSize = transform.Find("Description Parent/Stack Size Limit").GetComponent<TMP_Text>();

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        raycaster = GameManager.Instance.canvas.GetComponent<GraphicRaycaster>();
        canvasCamera = GameManager.Instance.canvas.GetComponent<Canvas>().worldCamera;

        canvasGroup.alpha = 0;
    }

    private void Update()
    {
        // Stores all the results of our raycasts
        List<RaycastResult> hits = new List<RaycastResult>();
        // Create a new pointer data for our raycast manipulation
        PointerEventData pointerData = new PointerEventData(GetComponent<EventSystem>());
        pointerData.position = Input.mousePosition;
        raycaster.Raycast(pointerData, hits);

        foreach (RaycastResult hit in hits)
        {
            if (hit.gameObject.TryGetComponent(out WindowInventoryItem windowItem))
            {
                if (windowItem.item != null)
                {
                    SetItem(windowItem.item);
                }

                RectTransform canvasRect = GameManager.Instance.canvas.GetComponent<RectTransform>();
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, pointerData.position, canvasCamera, out Vector2 pos);
                pos.x -= 5;
                pos.y += 5;
                rectTransform.anchoredPosition = pos;

                return;
            }
        }

        Clear();
    }

    /// <summary>
    ///     Sets the item.
    /// </summary>
    /// <param name="item"></param>
    public void SetItem(Item item)
    {
        itemName.text = item.name;
        description.text = item.description;
        itemStackSize.text = $"* Max Stack Size: {item.stackSizeLimit}";
        canvasGroup.alpha = 1;
        gameObject.transform.SetAsLastSibling();
        // Resets the content size fitter.
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)gameObject.transform);
    }

    /// <summary>
    ///     Hides the tooltip.
    /// </summary>
    public void Clear()
    {
        canvasGroup.alpha = 0;
    }
}