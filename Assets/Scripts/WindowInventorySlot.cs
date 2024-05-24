using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowInventorySlot : MonoBehaviour
{
    public Item item;
    private Image icon;
    private TMP_Text quantity;

    private void Awake()
    {
        icon = transform.Find("Item").GetComponent<Image>();
        quantity = transform.Find("Item").Find("Quantity").GetComponent<TMP_Text>();
    }

    public void SetSlot(Item item, int quantity)
    {
        this.item = item;
        icon.sprite = item.icon;
        this.quantity.text = quantity.ToString();
        transform.Find("Item").gameObject.SetActive(true);
    }
}
