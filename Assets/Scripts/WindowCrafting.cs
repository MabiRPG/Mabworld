using System.Collections.Generic;
using UnityEngine;

public class WindowCrafting : Window
{
    public static WindowCrafting Instance { get; private set; }
    public WindowCraftingSkillDropdown dropdown;
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
        recipeList = GetComponentInChildren<WindowCraftingRecipeList>();
        detailForm = GetComponentInChildren<WindowCraftingDetailForm>();
    }

    private void Start()
    {
        detailForm.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void Init(List<Skill> skills, Dictionary<int, List<CraftingRecipe>> recipes)
    {
        this.skills = skills;
        this.recipes = recipes;

        List<string> names = new List<string>();

        foreach (Skill skill in skills)
        {
            names.Add(skill.name);
        }

        dropdown.PopulateOptions(names, (option) => UpdateRecipeList(option));
        //recipeList.PopulateList(recipes, (product, ingredients) => ExpandDetails(product, ingredients));
    }

    public void UpdateRecipeList(string currentSkillName)
    {
        Skill currentSkill = FindSkillByName(currentSkillName);
        recipeList.Populate(recipes[currentSkill.ID], (recipe) => ExpandDetails(recipe));
    }

    public void ExpandDetails(CraftingRecipe recipe)
    {
        string currentSkillName = dropdown.GetCurrentOption();
        detailForm.gameObject.SetActive(true);
        detailForm.SetRecipe(FindSkillByName(currentSkillName), recipe);
    }

    private Skill FindSkillByName(string name)
    {
        foreach (Skill skill in skills)
        {
            if (skill.name == name)
            {
                return skill;
            }
        }

        return null;
    }
}