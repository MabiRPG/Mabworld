using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowCraftingDetailForm : MonoBehaviour
{
    private TMP_Text detailsText;
    private Image productImage;
    private Image[] ingredientImages;
    private TMP_InputField quantityInput;
    private Button craftButton;

    private void Awake()
    {
        detailsText = transform.Find("Details Text").GetComponent<TMP_Text>();
        productImage = transform.Find("Item Image Boxes/Product Image").GetComponent<Image>();
        ingredientImages = transform.Find("Item Image Boxes/Ingredient Parent").GetComponentsInChildren<Image>();
        quantityInput = transform.Find("Production Form/Quantity Input Field").GetComponent<TMP_InputField>();
        craftButton = transform.Find("Production Form/Craft Button").GetComponent<Button>();
    }

    public void SetRecipe(Skill skill, Item product, List<Item> ingredients)
    {
        string details = $"{product.name}\nSkill: Rank {skill.ranks[skill.index.Value]} {skill.name}\nSuccess Rate:?";
        detailsText.text = details;
        productImage.sprite = product.icon;

        for (int i = 0; i < Math.Min(ingredients.Count, ingredientImages.Length); i++)
        {
            ingredientImages[i].sprite = ingredients[i].icon;
        }
    }
}