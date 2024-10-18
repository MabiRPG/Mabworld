using System.Collections.Generic;
using System.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class CraftingRecipeWindowEditor : EditorWindow
{
    private MultiColumnListView recipeView;

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
        CreateRecipeView();
    }

    private void CreateRecipeView()
    {

    }
}
