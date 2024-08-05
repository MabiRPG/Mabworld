using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowCraftingRecipeItem : MonoBehaviour
{
    private Image image;
    private TMP_Text text;
    public CraftingRecipe recipe;

    private void Awake()
    {
        image = transform.Find("Image Parent/Image").GetComponent<Image>();
        text = transform.Find("Name Parent/Name").GetComponent<TMP_Text>();
    }

    public void SetRecipe(CraftingRecipe recipe)
    {
        this.recipe = recipe;
        image.sprite = recipe.product.icon;
        text.text = recipe.product.name;
    }
}