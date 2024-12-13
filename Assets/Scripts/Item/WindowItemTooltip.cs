using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
///     Handles rendering the item tooltip when mousing over an item.
/// </summary>
public class WindowItemTooltip : MonoBehaviour
{
    public static WindowItemTooltip Instance { get; private set; }

    private TMP_Text itemName;
    private TMP_Text description;
    private TMP_Text itemStackSize;
    private TMP_Text stats;

    public RectTransform rectTransform;
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
        stats = transform.Find("Stat Parent/Description").GetComponent<TMP_Text>();

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        raycaster = GameManager.Instance.canvas.GetComponent<GraphicRaycaster>();
        canvasCamera = GameManager.Instance.canvas.GetComponent<Canvas>().worldCamera;

        // canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }

    /// <summary>
    ///     Sets the item.
    /// </summary>
    /// <param name="item"></param>
    public void SetItem(Item item)
    {
        itemName.text = item.model.name;
        description.text = item.model.description;
        itemStackSize.text = $"* Max Stack Size: {item.model.stackSizeLimit}";
        stats.text = "";

        foreach ((int statID, float roll) in item.stats)
        {
            stats.text += $"{ItemStatTypeModel.FindByID(statID)}: {roll}\n";
        }

        gameObject.SetActive(true);
        gameObject.transform.SetAsLastSibling();
        // Resets the content size fitter.
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        // canvasGroup.alpha = 1;
    }

    /// <summary>
    ///     Hides the tooltip.
    /// </summary>
    public void Clear()
    {
        // canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }
}