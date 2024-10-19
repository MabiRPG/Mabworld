using System;
using System.Collections.Generic;
using System.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class CraftingRecipeWindowEditor : EditorWindow
{
    private int index;
    private CraftingRecipeModel selectedRecipe;

    private MultiColumnListView recipeView;
    private MultiColumnListView ingredientView;
    private MultiColumnListView productView;

    private DatabaseManager database;
    private List<CraftingRecipeModel> recipes;

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("MabWorld/Crafting Recipe Editor")]
    public static void ShowExample()
    {
        CraftingRecipeWindowEditor wnd = GetWindow<CraftingRecipeWindowEditor>();
        wnd.titleContent = new GUIContent("CraftingRecipeWindowEditor");
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
    }

    public void CreateGUI()
    {
        Initialize();

        m_VisualTreeAsset.CloneTree(rootVisualElement);

        recipeView = rootVisualElement.Q<MultiColumnListView>("recipeView");
        ingredientView = rootVisualElement.Q<MultiColumnListView>("ingredientView");
        productView = rootVisualElement.Q<MultiColumnListView>("productView");

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
        ingredientView.columns["ingredient"].makeCell = () => new Label();
        ingredientView.columns["ingredient"].bindCell = (item, index) =>
        {
            CraftingRecipeIngredientModel ingredient = 
                (CraftingRecipeIngredientModel)ingredientView.itemsSource[index];
            (item as Label).text = ingredient.item.name;
        };

        ingredientView.columns["quantity"].makeCell = () => new Label();
        ingredientView.columns["quantity"].bindCell = (item, index) =>
        {
            CraftingRecipeIngredientModel ingredient = 
                (CraftingRecipeIngredientModel)ingredientView.itemsSource[index];
            (item as Label).text = ingredient.quantity.ToString();
        };
    }

    private void CreateProductView()
    {
        productView.columns["product"].makeCell = () => new Label();
        productView.columns["product"].bindCell = (item, index) =>
        {
            CraftingRecipeProductModel product = 
                (CraftingRecipeProductModel)productView.itemsSource[index];
            (item as Label).text = product.item.name;
        };

        productView.columns["quantity"].makeCell = () => new Label();
        productView.columns["quantity"].bindCell = (item, index) =>
        {
            CraftingRecipeProductModel product = 
                (CraftingRecipeProductModel)productView.itemsSource[index];
            (item as Label).text = product.quantity.ToString();
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
    }
}
