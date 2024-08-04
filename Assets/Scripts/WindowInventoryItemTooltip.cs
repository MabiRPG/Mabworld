using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Handles rendering the item tooltip when mousing over an item in the inventory window.
/// </summary>
public class WindowInventoryItemTooltip : MonoBehaviour
{
    private TMP_Text tname;
    private TMP_Text description;
    private TMP_Text itemStackSize;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    private void Awake()
    {
        tname = transform.Find("Item Name").GetComponent<TMP_Text>();
        description = transform.Find("Description Parent/Description").GetComponent<TMP_Text>();
        itemStackSize = transform.Find("Description Parent/Stack Size Limit").GetComponent<TMP_Text>();

        gameObject.SetActive(false);
    }

    /// <summary>
    ///     Sets the item.
    /// </summary>
    /// <param name="item"></param>
    public void SetItem(Item item)
    {
        tname.text = item.name;
        description.text = item.description;
        itemStackSize.text = $"* Max Stack Size: {item.stackSizeLimit}";
        gameObject.SetActive(true);
        gameObject.transform.SetAsLastSibling();
        // Resets the content size fitter.
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)gameObject.transform);
    }

    /// <summary>
    ///     Hides the tooltip.
    /// </summary>
    public void Clear()
    {
        gameObject.SetActive(false);
    }
}