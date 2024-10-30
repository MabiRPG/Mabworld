using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowCraftingRecipeItem : MonoBehaviour
{
    private WindowItem product;
    private TMP_Text productName;
    public CraftingRecipe recipe;

    private void Awake()
    {
        product = GetComponentInChildren<WindowItem>();
        productName = transform.Find("Name Parent/Name").GetComponent<TMP_Text>();
    }

    public void SetRecipe(CraftingRecipe recipe)
    {
        this.recipe = recipe;

        CraftingRecipeProductModel firstProduct = recipe.products.Values.First();

        product.SetItem(new Item(firstProduct.itemID), firstProduct.quantity);
        productName.text = firstProduct.item.name;
    }
}