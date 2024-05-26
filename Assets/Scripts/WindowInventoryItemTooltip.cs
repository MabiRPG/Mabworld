using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowInventoryItemTooltip : MonoBehaviour
{
    private TMP_Text name;
    private TMP_Text description;
    private TMP_Text itemStackSize;

    private void Awake()
    {
        name = transform.Find("Item Name").GetComponent<TMP_Text>();
        description = transform.Find("Description Parent").Find("Description").GetComponent<TMP_Text>();
        itemStackSize = transform.Find("Description Parent").Find("Stack Size Limit").GetComponent<TMP_Text>();

        gameObject.SetActive(false);
    }

    public void SetItem(Item item)
    {
        name.text = item.name;
        description.text = item.description;
        itemStackSize.text = $"* Max Stack Size: {item.stackSizeLimit}";
        gameObject.SetActive(true);
        gameObject.transform.SetAsLastSibling();
        // Resets the content size fitter.
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)gameObject.transform);
    }

    public void Clear()
    {
        gameObject.SetActive(false);
    }
}