using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowCraftingDetailForm : MonoBehaviour
{
    private TMP_Text detailsText;
    private UI_Item productItem;
    private Transform ingredientParentTransform;
    private TMP_InputField quantityInput;
    private UI_NumberRangeValidator rangeValidator;
    private Button craftButton;

    [SerializeField]
    private GameObject ingredientPrefab;
    private PrefabFactory ingredientPrefabs;

    private Skill currentSkill;
    private CraftingRecipe currentRecipe;

    private void Awake()
    {
        detailsText = transform.Find("Details Text").GetComponent<TMP_Text>();
        productItem = transform.Find("Item Image Boxes/Product Item").GetComponent<UI_Item>();
        ingredientParentTransform = transform.Find("Item Image Boxes/Ingredient Parent");
        quantityInput = transform
            .Find("Production Form/Quantity Input Field")
            .GetComponent<TMP_InputField>();
        rangeValidator = (UI_NumberRangeValidator)quantityInput.inputValidator;
        craftButton = transform.Find("Production Form/Craft Button").GetComponent<Button>();

        ingredientPrefabs = ScriptableObject.CreateInstance<PrefabFactory>();
        ingredientPrefabs.SetPrefab(ingredientPrefab);
    }

    private void OnEnable()
    {
        craftButton.onClick.AddListener(
            delegate
            {
                Craft(currentSkill, currentRecipe, int.Parse(quantityInput.text));
            }
        );
        Player.Instance.inventoryManager.changeEvent.OnChange += () =>
            SetRecipe(currentSkill, currentRecipe);
    }

    private void OnDisable()
    {
        craftButton.onClick.RemoveAllListeners();
        Player.Instance.inventoryManager.changeEvent.OnChange -= () =>
            SetRecipe(currentSkill, currentRecipe);
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

        StringBuilder builder = new StringBuilder();

        foreach (CraftingRecipeProductModel product in recipe.products.Values)
        {
            builder.Append($"{product.item.name} ");
        }

        builder.Append($"{recipe.rankRequired} {skill.model.name}\nSuccess Rate:?");
        string details = builder.ToString();
        detailsText.text = details;

        CraftingRecipeProductModel firstProduct = recipe.products.Values.First();

        productItem.SetItem(new Item(firstProduct.itemID), firstProduct.quantity);

        ingredientPrefabs.SetActiveAll(false);

        foreach (CraftingRecipeIngredientModel ingredient in recipe.ingredients.Values)
        {
            GameObject obj = ingredientPrefabs.GetFree(ingredient, ingredientParentTransform);
            UI_Item inventoryItem = obj.GetComponentInChildren<UI_Item>();

            int playerQuantity = Player.Instance.inventoryManager.GetQuantity(ingredient.item);
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
            inventoryItem.SetItem(new Item(ingredient.itemID), text);

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
        quantityInput.text = "1";
        quantityInput.stringPosition = 1;
        quantityInput.caretPosition = 1;
    }

    private void Craft(Skill skill, CraftingRecipe recipe, int quantity)
    {
        ActionCraftController action = new ActionCraftController(
            Player.Instance,
            this,
            transform.TransformPoint(Vector3.zero),
            skill,
            recipe.ingredients.Values.ToList(),
            recipe.products.Values.ToList(),
            quantity
        );
        action.Handle();
    }
}
