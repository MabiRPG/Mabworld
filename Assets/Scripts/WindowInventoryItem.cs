using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowInventoryItem : MonoBehaviour
{
    public Item item;
    public int quantity;
    public Point2D pos;
    public int widthInGrid;
    public int heightInGrid;
    public int widthInPixel;
    public int heightInPixel;

    private Image icon;
    private TMP_Text text;

    private void Awake()
    {
        icon = GetComponent<Image>();
        text = transform.Find("Quantity").GetComponent<TMP_Text>();
    }

    public void SetItem(Item item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
        icon.sprite = item.icon;
        text.text = quantity.ToString();
    }

    public void Clear()
    {
        gameObject.SetActive(false);
    }
}