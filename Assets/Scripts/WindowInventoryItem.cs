using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Handles all individual item sprites in the inventory window.
/// </summary>
public class WindowInventoryItem : MonoBehaviour
{
    public Item item;
    public int quantity;

    private Image icon;
    private TMP_Text quantityText;

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
        icon.sprite = item.icon;
        quantityText.text = quantity.ToString();
    }

    public void SetItem(Item item, string text)
    {
        this.item = item;
        icon.sprite = item.icon;
        quantityText.text = text;
    }
}