using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Handles rendering the item tooltip when mousing over an item.
/// </summary>
public class UI_ItemTooltip : MonoBehaviour
{
    private TMP_Text itemName;
    private TMP_Text description;
    private TMP_Text itemStackSize;
    private TMP_Text stats;

    public RectTransform rectTransform;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    private void Awake()
    {
        itemName = transform.Find("Item Name").GetComponent<TMP_Text>();
        description = transform.Find("Description Parent/Description").GetComponent<TMP_Text>();
        itemStackSize = transform.Find("Description Parent/Stack Size Limit").GetComponent<TMP_Text>();
        stats = transform.Find("Stat Parent/Description").GetComponent<TMP_Text>();

        rectTransform = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    /// <summary>
    ///     Sets the item.
    /// </summary>
    /// <param name="item"></param>
    public void SetItem(Item item)
    {
        itemName.text = item.model.Name;
        description.text = item.model.Description;
        itemStackSize.text = $"* Max Stack Size: {item.model.StackSizeLimit}";
        stats.text = "";

        foreach ((int statID, float roll) in item.stats)
        {
            stats.text += $"{ItemStatTypeModel.FindByID(statID)}: {roll}\n";
        }

        gameObject.SetActive(true);
        gameObject.transform.SetAsLastSibling();
        // Resets the content size fitter.
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    public void SetPosition(Vector2 position)
    {
        rectTransform.anchoredPosition = position;
    }

    /// <summary>
    ///     Hides the tooltip.
    /// </summary>
    public void Clear()
    {
        Destroy(gameObject);
    }
}