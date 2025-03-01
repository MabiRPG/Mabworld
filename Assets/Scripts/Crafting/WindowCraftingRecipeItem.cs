using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowCraftingRecipeItem : MonoBehaviour
{
    private UI_Item product;
    private TMP_Text productName;
    public CraftingRecipe recipe;

    private void Awake()
    {
        product = GetComponentInChildren<UI_Item>();
        productName = transform.Find("Name Parent/Name").GetComponent<TMP_Text>();
    }

    public void SetRecipe(CraftingRecipe recipe)
    {
        this.recipe = recipe;

        CraftingRecipeProductModel firstProduct = recipe.products.Values.First();

        product.SetItem(new Item(firstProduct.itemID), firstProduct.quantity);
        productName.text = firstProduct.item.Name;
    }
}