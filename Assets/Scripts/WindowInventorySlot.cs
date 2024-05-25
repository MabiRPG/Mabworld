using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowInventorySlot : MonoBehaviour
{
    public Item item;
    public int quantity;
    private Image icon;
    private TMP_Text quantityText;

    private void Awake()
    {
        icon = transform.Find("Item").GetComponent<Image>();
        quantityText = transform.Find("Item").Find("Quantity").GetComponent<TMP_Text>();
    }

    public void SetSlot(Item item, int quantity)
    {
        this.item = item;
        transform.Find("Item").gameObject.SetActive(true);
        icon.sprite = item.icon;
        quantityText.text = quantity.ToString();
    }
}
