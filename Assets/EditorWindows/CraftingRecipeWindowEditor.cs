using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class CraftingRecipeWindowEditor : EditorWindow
{
    private Button refreshButton;
    private Button commitButton;

    private int index;
    private CraftingRecipeModel selectedRecipe;
    private DropdownField selectedCraftingStation;
    private DropdownField selectedCraftingSkill;
    private DropdownField selectedCraftingRank;

    private MultiColumnListView recipeView;
    private MultiColumnListView ingredientView;
    private MultiColumnListView productView;

    private DatabaseManager database;
    private List<SkillModel> skills;
    private List<ItemModel> items;
    private List<CraftingRecipeModel> recipes;

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("MabWorld/Crafting Recipe Editor")]
    public static void ShowExample()
    {
        CraftingRecipeWindowEditor wnd = GetWindow<CraftingRecipeWindowEditor>();
        wnd.titleContent = new GUIContent("Crafting Recipe Editor");
    }

    private void Initialize()
    {
        database = new DatabaseManager("mabinogi.db");

        DataTable dt = database.Read("SELECT id FROM crafting_recipe;");
        recipes = new List<CraftingRecipeModel>();

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            CraftingRecipeModel recipe = new CraftingRecipeModel(database, ID);
            recipes.Add(recipe);     
        }

        dt = database.Read("SELECT id FROM crafting_station;");

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            new CraftingStationModel(database, ID);
        }

        dt = database.Read("SELECT id FROM skill;");
        skills = new List<SkillModel>();

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            SkillModel skill = new SkillModel(database, ID);
            skills.Add(skill);
        }

        skills = skills.OrderBy(v => v.name).ToList();

        dt = database.Read("SELECT id FROM item;");
        items = new List<ItemModel>();

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            ItemModel item = new ItemModel(database, ID);
            items.Add(item);
        }

        items = items.OrderBy(v => v.name).ToList();
    }

    public void CreateGUI()
    {
        Initialize();

        m_VisualTreeAsset.CloneTree(rootVisualElement);

        refreshButton = rootVisualElement.Q<Button>("refreshButton");
        refreshButton.RegisterCallback<ClickEvent>(e => 
        { 
            Initialize();
            recipeView.itemsSource = recipes;
            DisplayRecipeInfo(index);
            recipeView.RefreshItems();
        });

        commitButton = rootVisualElement.Q<Button>("commitButton");
        commitButton.RegisterCallback<ClickEvent>(e => SaveRecipes());

        recipeView = rootVisualElement.Q<MultiColumnListView>("recipeView");
        ingredientView = rootVisualElement.Q<MultiColumnListView>("ingredientView");
        productView = rootVisualElement.Q<MultiColumnListView>("productView");

        selectedCraftingStation = rootVisualElement.Q<DropdownField>("selectedCraftingStation");
        selectedCraftingStation.RegisterValueChangedCallback(e =>
        {
            selectedRecipe.craftingStationID = CraftingStationModel.FindByName(e.newValue);
        });
        selectedCraftingStation.choices = new List<string>(CraftingStationModel.types.Values);

        selectedCraftingSkill = rootVisualElement.Q<DropdownField>("selectedCraftingSkill");
        selectedCraftingSkill.RegisterValueChangedCallback(e =>
        {
            selectedRecipe.skillID = skills.Where(v => v.name == e.newValue)
                .Select(v => v.ID).First();
        });
        selectedCraftingSkill.choices = new List<string>(skills.Select(v => v.name));

        selectedCraftingRank = rootVisualElement.Q<DropdownField>("selectedCraftingRank");
        selectedCraftingRank.RegisterValueChangedCallback(e =>
        {
            selectedRecipe.rankRequired = e.newValue;
        });
        selectedCraftingRank.choices = SkillModel.ranks;

        CreateRecipeView();
        CreateIngredientView();
        CreateProductView();
    }

    private void CreateRecipeView()
    {
        recipeView.columns["product"].makeCell = () => new Label();
        recipeView.columns["product"].bindCell = (item, index) =>
        {
            CraftingRecipeModel recipe = (CraftingRecipeModel)recipeView.itemsSource[index];
            string label = "";

            foreach (CraftingRecipeProductModel product in recipe.products)
            {
                label += $"{product.item.name} ";
            }

            (item as Label).text = label;
        };

        recipeView.itemsSource = recipes;
        recipeView.selectedIndicesChanged += OnRecipeSelectionChange;
        recipeView.RefreshItems();
    }

    private void CreateIngredientView()
    {
        ingredientView.columns["ingredient"].makeCell = () =>
        {
            DropdownField dropdown = new DropdownField();
            dropdown.choices = items.Select(v => v.name).ToList();
            dropdown.RegisterValueChangedCallback(e =>
            {
                (ingredientView.itemsSource[(int)dropdown.userData] as CraftingRecipeIngredientModel)
                    .ChangeItem(items.Where(v => v.name == e.newValue).Select(v => v.ID).First());
            });

            return dropdown;
        };
        ingredientView.columns["ingredient"].bindCell = (item, index) =>
        {
            CraftingRecipeIngredientModel ingredient = 
                (CraftingRecipeIngredientModel)ingredientView.itemsSource[index];
            (item as DropdownField).SetValueWithoutNotify(ingredient.item.name);
            (item as DropdownField).userData = index;
        };

        ingredientView.columns["quantity"].makeCell = () =>
        {
            IntegerField integerField = new IntegerField();
            integerField.RegisterValueChangedCallback(e =>
            {
                (ingredientView.itemsSource[(int)integerField.userData] as CraftingRecipeIngredientModel)
                    .quantity = e.newValue;
            });

            return integerField;
        };
        ingredientView.columns["quantity"].bindCell = (item, index) =>
        {
            CraftingRecipeIngredientModel ingredient = 
                (CraftingRecipeIngredientModel)ingredientView.itemsSource[index];
            (item as IntegerField).SetValueWithoutNotify(ingredient.quantity);
            (item as IntegerField).userData = index;
        };
    }

    private void CreateProductView()
    {
        productView.columns["product"].makeCell = () =>
        {
            DropdownField dropdown = new DropdownField();
            dropdown.choices = items.Select(v => v.name).ToList();
            dropdown.RegisterValueChangedCallback(e =>
            {
                (productView.itemsSource[(int)dropdown.userData] as CraftingRecipeProductModel)
                    .ChangeItem(items.Where(v => v.name == e.newValue).Select(v => v.ID).First());
            });

            return dropdown;
        };
        productView.columns["product"].bindCell = (item, index) =>
        {
            CraftingRecipeProductModel product = 
                (CraftingRecipeProductModel)productView.itemsSource[index];
            (item as DropdownField).SetValueWithoutNotify(product.item.name);
            (item as DropdownField).userData = index;
        };

        productView.columns["quantity"].makeCell = () =>
        {
            IntegerField integerField = new IntegerField();
            integerField.RegisterValueChangedCallback(e =>
            {
                (productView.itemsSource[(int)integerField.userData] as CraftingRecipeProductModel)
                    .quantity = e.newValue;
            });

            return integerField;
        };
        productView.columns["quantity"].bindCell = (item, index) =>
        {
            CraftingRecipeProductModel product = 
                (CraftingRecipeProductModel)productView.itemsSource[index];
            (item as IntegerField).SetValueWithoutNotify(product.quantity);
            (item as IntegerField).userData = index;
        };
    }

    private void OnRecipeSelectionChange(IEnumerable<int> selectedIndex)
    {
        var enumerator = selectedIndex.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            return;
        }

        index = enumerator.Current;
        DisplayRecipeInfo(index);        
    }

    private void DisplayRecipeInfo(int index)
    {
        selectedRecipe = (CraftingRecipeModel)recipeView.itemsSource[index];

        ingredientView.itemsSource = selectedRecipe.ingredients;
        ingredientView.RefreshItems();

        productView.itemsSource = selectedRecipe.products;
        productView.RefreshItems();

        selectedCraftingStation.SetValueWithoutNotify(
            CraftingStationModel.FindByID(selectedRecipe.craftingStationID));

        selectedCraftingSkill.SetValueWithoutNotify(
            skills.Where(v => v.ID == selectedRecipe.skillID).Select(v => v.name).First());

        selectedCraftingRank.SetValueWithoutNotify(selectedRecipe.rankRequired);
    }

    private void SaveRecipes()
    {
        database.Write(@"DELETE FROM crafting_recipe; DELETE FROM crafting_recipe_ingredient; 
            DELETE FROM crafting_recipe_product;", new Dictionary<string, ModelFieldReference>());

        foreach (CraftingRecipeModel recipe in recipes)
        {
            recipe.Upsert();

            foreach (CraftingRecipeIngredientModel ingredient in recipe.ingredients)
            {
                ingredient.Upsert();
            }

            foreach (CraftingRecipeProductModel product in recipe.products)
            {
                product.Upsert();
            }
        }
    }
}
