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
    private NumberRangeValidator rangeValidator;
    private Button craftButton;

    [SerializeField]
    private GameObject ingredientPrefab;
    private PrefabFactory ingredientPrefabs;

    private Skill currentSkill;
    private CraftingRecipe currentRecipe;

    private void Awake()
    {
        detailsText = transform.Find("Details Text").GetComponent<TMP_Text>();
        productItem = transform.Find("Item Image Boxes/Product Item").GetComponent<WindowInventoryItem>();
        ingredientParentTransform = transform.Find("Item Image Boxes/Ingredient Parent");
        quantityInput = transform.Find("Production Form/Quantity Input Field").GetComponent<TMP_InputField>();
        rangeValidator = (NumberRangeValidator)quantityInput.inputValidator;
        craftButton = transform.Find("Production Form/Craft Button").GetComponent<Button>();

        ingredientPrefabs = ScriptableObject.CreateInstance<PrefabFactory>();
        ingredientPrefabs.SetPrefab(ingredientPrefab);
    }

    private void OnEnable()
    {
        craftButton.onClick.AddListener(delegate { Craft(currentSkill, currentRecipe, int.Parse(quantityInput.text)); });
        Player.Instance.inventoryManager.changeEvent.OnChange += () => SetRecipe(currentSkill, currentRecipe);
    }

    private void OnDisable()
    {
        craftButton.onClick.RemoveAllListeners();
        Player.Instance.inventoryManager.changeEvent.OnChange -= () => SetRecipe(currentSkill, currentRecipe);
    }

    public void SetRecipe(Skill skill, CraftingRecipe recipe)
    {
        if (skill == null || recipe == null)
        {
            return;
        }

        currentSkill = skill;
        currentRecipe = recipe;

        int craftable = int.MaxValue;
        string details = $"{recipe.product.name} (Rank {recipe.rankRequired} {skill.name})\nSuccess Rate:?";
        detailsText.text = details;
        productItem.SetItem(recipe.product, recipe.product.quantity);

        ingredientPrefabs.SetActiveAll(false);

        foreach (Item ingredient in recipe.ingredients)
        {
            GameObject obj = ingredientPrefabs.GetFree(ingredient, ingredientParentTransform);
            WindowInventoryItem inventoryItem = obj.GetComponentInChildren<WindowInventoryItem>();

            int playerQuantity = Player.Instance.inventoryManager.GetQuantity(ingredient);
            Debug.Log(playerQuantity);
            string text;

            if (playerQuantity == 0)
            {
                text = $"<color=\"red\">{playerQuantity}</color>";
            }
            else
            {
                text = $"{playerQuantity}";
            }

            text += $"/{ingredient.quantity}";
            inventoryItem.SetItem(ingredient, text);

            if (playerQuantity / ingredient.quantity < craftable)
            {
                craftable = playerQuantity / ingredient.quantity;
            } 
        }

        if (craftable == 0)
        {
            quantityInput.interactable = false;
            craftButton.interactable = false;
            return;
        }

        quantityInput.interactable = true;
        craftButton.interactable = true;
        rangeValidator.SetRange(1, craftable);
    }

    private void Craft(Skill skill, CraftingRecipe recipe, int quantity)
    {
        Player.Instance.inventoryManager.AddItem(recipe.product.ID, quantity);

        foreach (Item ingredient in recipe.ingredients)
        {
            int usedAmount = ingredient.quantity * quantity;
            Player.Instance.inventoryManager.RemoveItem(ingredient.ID, usedAmount);
        }
    }
}