using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowCraftingRecipeItem : MonoBehaviour
{
    private Image image;
    private TMP_Text text;
    public Item product;

    private void Awake()
    {
        image = transform.Find("Image Parent/Image").GetComponent<Image>();
        text = transform.Find("Name Parent/Name").GetComponent<TMP_Text>();
    }

    public void SetRecipe(Item item)
    {
        product = item;
        image.sprite = item.icon;
        text.text = item.name;
    }
}