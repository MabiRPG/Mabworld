using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowCraftingDetailForm : MonoBehaviour
{
    private TMP_Text detailsText;
    private WindowInventoryItem productItem;
    private Transform ingredientParentTransform;
    private TMP_InputField quantityInput;
    private Button craftButton;

    [SerializeField]
    private GameObject ingredientPrefab;
    private PrefabFactory ingredientPrefabs;

    private void Awake()
    {
        detailsText = transform.Find("Details Text").GetComponent<TMP_Text>();
        productItem = transform.Find("Item Image Boxes/Product Item").GetComponent<WindowInventoryItem>();
        ingredientParentTransform = transform.Find("Item Image Boxes/Ingredient Parent");
        quantityInput = transform.Find("Production Form/Quantity Input Field").GetComponent<TMP_InputField>();
        craftButton = transform.Find("Production Form/Craft Button").GetComponent<Button>();

        ingredientPrefabs = ScriptableObject.CreateInstance<PrefabFactory>();
        ingredientPrefabs.SetPrefab(ingredientPrefab);
    }

    public void SetRecipe(Skill skill, CraftingRecipe recipe)
    {
        string details = $"{recipe.product.name} (Rank {recipe.rankRequired} {skill.name})\nSuccess Rate:?";
        detailsText.text = details;
        productItem.SetItem(recipe.product, recipe.product.quantity);

        ingredientPrefabs.SetActiveAll(false);

        foreach (Item ingredient in recipe.ingredients)
        {
            GameObject obj = ingredientPrefabs.GetFree(ingredient, ingredientParentTransform);
            WindowInventoryItem inventoryItem = obj.GetComponentInChildren<WindowInventoryItem>();
            inventoryItem.SetItem(ingredient, ingredient.quantity);
        }
    }
}