using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowCraftingRecipeItem : MonoBehaviour
{
    private WindowInventoryItem product;
    private TMP_Text productName;
    public CraftingRecipe recipe;

    private void Awake()
    {
        product = GetComponentInChildren<WindowInventoryItem>();
        productName = transform.Find("Name Parent/Name").GetComponent<TMP_Text>();
    }

    public void SetRecipe(CraftingRecipe recipe)
    {
        this.recipe = recipe;
        product.SetItem(recipe.product, recipe.product.quantity);
        productName.text = recipe.product.name;
    }
}