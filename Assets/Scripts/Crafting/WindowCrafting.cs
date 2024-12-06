using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowCrafting : Window
{
    public static WindowCrafting Instance { get; private set; }
    public WindowCraftingSkillDropdown dropdown;
    public WindowCraftingItemSearchField searchField;
    public WindowCraftingRecipeList recipeList;
    public WindowCraftingDetailForm detailForm;

    private List<Skill> skills;
    private Dictionary<int, List<CraftingRecipe>> recipes;

    protected override void Awake()
    {
        base.Awake();

        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        dropdown = GetComponentInChildren<WindowCraftingSkillDropdown>();
        searchField = GetComponentInChildren<WindowCraftingItemSearchField>();
        recipeList = GetComponentInChildren<WindowCraftingRecipeList>();
        detailForm = GetComponentInChildren<WindowCraftingDetailForm>();

        isFullscreenFocus = true;
    }

    private void Start()
    {
        detailForm.gameObject.SetActive(false);
        // gameObject.SetActive(false);
    }

    public void Init(List<Skill> skills, Dictionary<int, List<CraftingRecipe>> recipes)
    {
        this.skills = skills;
        this.recipes = recipes;

        List<string> names = new List<string>();

        foreach (Skill skill in skills)
        {
            names.Add(skill.model.name);
        }

        dropdown.PopulateOptions(names, (option) => UpdateRecipeList(option));
        searchField.SetSearchAction((input) => SearchRecipeList(input));
        detailForm.gameObject.SetActive(false);

        // Resets the content size fitter.
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }

    public void UpdateRecipeList(string currentSkillName)
    {
        Skill currentSkill = FindSkillByName(currentSkillName);
        recipeList.Populate(recipes[currentSkill.model.ID], (recipe) => ExpandDetails(recipe));

        // Resets the content size fitter.
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }

    public void SearchRecipeList(string recipeName)
    {
        Skill currentSkill = FindSkillByName(dropdown.GetCurrentOption());
        List<CraftingRecipe> newRecipeList = new List<CraftingRecipe>();

        foreach (CraftingRecipe recipe in recipes[currentSkill.model.ID])
        {
            // if (recipe.products.name.StartsWith(recipeName))
            // {
            //     newRecipeList.Add(recipe);
            // }
            foreach ((int ID, CraftingRecipeProductModel product) in recipe.products)
            {
                if (product.item.name.StartsWith(recipeName))
                {
                    newRecipeList.Add(recipe);
                    break;
                }
            }
        }

        recipeList.Populate(newRecipeList, (recipe) => ExpandDetails(recipe));

        // Resets the content size fitter.
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }

    public void ExpandDetails(CraftingRecipe recipe)
    {
        string currentSkillName = dropdown.GetCurrentOption();
        detailForm.gameObject.SetActive(true);
        detailForm.SetRecipe(FindSkillByName(currentSkillName), recipe);

        // Resets the content size fitter.
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)detailForm.gameObject.transform);
    }

    private Skill FindSkillByName(string name)
    {
        foreach (Skill skill in skills)
        {
            if (skill.model.name == name)
            {
                return skill;
            }
        }

        return null;
    }

    public void Update()
    {
        //Debug.Log(TypingInField());
    }

    public bool TypingInField()
    {
        TMP_InputField[] inputFields = GetComponentsInChildren<TMP_InputField>();

        foreach (TMP_InputField inputField in inputFields)
        {
            if (inputField.isFocused)
            {
                return true;
            }
        }

        return false;
    }
}