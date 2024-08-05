using System.Collections.Generic;
using UnityEngine;

public class WindowCrafting : Window
{
    public static WindowCrafting Instance { get; private set; }
    public WindowCraftingSkillDropdown dropdown;
    public WindowCraftingRecipeList recipeList;
    public WindowCraftingDetailForm detailForm;

    private List<Skill> skills;

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
        gameObject.SetActive(false);
    }

    public void Init(List<Skill> skills, Dictionary<Item, List<Item>> recipes)
    {
        this.skills = skills;
        List<string> names = new List<string>();

        foreach (Skill skill in skills)
        {
            names.Add(skill.name);
        }

        dropdown.PopulateOptions(names);
        recipeList.PopulateList(recipes, (product, ingredients) => ExpandDetails(product, ingredients));
    }

    public void ExpandDetails(Item product, List<Item> ingredients)
    {
        string currentSkillName = dropdown.GetCurrentOption();
        Skill currentSkill = null;

        foreach (Skill skill in skills)
        {
            if (skill.name == currentSkillName)
            {
                currentSkill = skill;
                break;
            }
        }

        detailForm.SetRecipe(currentSkill, product, ingredients);
    }
}