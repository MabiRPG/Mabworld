using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemWorldDrop : MonoBehaviour
{
    private UI_Item uiItem;

    private Button button;
    private TMP_Text text;
    private RectTransform rectTransform;

    private void Awake()
    {
        button = GetComponent<Button>();
        text = GetComponentInChildren<TMP_Text>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void Init(UI_Item uiItem, Vector2 localPosition)
    {
        this.uiItem = uiItem;
        button.image.color = Color.yellow;
        text.text = uiItem.item.model.name;
        rectTransform.localPosition = localPosition;

        button.onClick.AddListener(Pickup);

        // Resets the content size fitter.
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)gameObject.transform);
    }

    private void Pickup()
    {
        uiItem.gameObject.SetActive(true);
        uiItem.StartPickup();
        Destroy(gameObject);
    }
}