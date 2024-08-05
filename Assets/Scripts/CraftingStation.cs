using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class CraftingStation : MonoBehaviour
{
    [SerializeField]
    private int ID;
    private List<int> availableSkillIDs = new List<int>();
    private Dictionary<Item, List<Item>> recipes = new Dictionary<Item, List<Item>>();

    private const string skillQuery = @"SELECT DISTINCT crafting_recipe.skill_id
        FROM crafting_station
        JOIN crafting_recipe
        ON crafting_station.id = crafting_recipe.crafting_station_id
        WHERE crafting_station.id = @id;";

    private const string recipeQuery = @"SELECT 
            crafting_recipe_ingredient.item_id as ""ingredientID"", 
            crafting_recipe_ingredient.item_quantity as ""ingredientQuantity"", 
            crafting_recipe_product.item_id as ""productID"", 
            crafting_recipe_product.item_quantity as ""productQuantity""
        FROM crafting_recipe
        JOIN crafting_recipe_ingredient
        ON crafting_recipe.id = crafting_recipe_ingredient.crafting_recipe_id
        JOIN crafting_recipe_product
        ON crafting_recipe.id = crafting_recipe_product.crafting_recipe_id
        WHERE crafting_recipe.crafting_station_id = @id;";

    private void Awake()
    {
        DataTable dt = GameManager.Instance.QueryDatabase(skillQuery, ("@id", ID));

        foreach (DataRow row in dt.Rows)
        {
            availableSkillIDs.Add(int.Parse(row["skill_id"].ToString()));
        }

        dt = GameManager.Instance.QueryDatabase(recipeQuery, ("@id", ID));

        foreach (DataRow row in dt.Rows)
        {
            Item ingredient = new Item(int.Parse(row["ingredientID"].ToString()));
            ingredient.quantity = int.Parse(row["ingredientQuantity"].ToString());
            Item product = new Item(int.Parse(row["productID"].ToString()));
            product.quantity = int.Parse(row["productQuantity"].ToString());

            if (recipes.ContainsKey(product))
            {
                recipes[product].Add(ingredient);
            }
            else
            {
                recipes.Add(product, new List<Item> { ingredient });
            }
        }
    }

    private void OnMouseDown()
    {
        List<Skill> skills = new List<Skill>();

        foreach (int skillID in availableSkillIDs)
        {
            if (Player.Instance.skillManager.IsLearned(skillID))
            {
                skills.Add(Player.Instance.skillManager.Get(skillID));
            }
        }

        WindowCrafting.Instance.ToggleVisible();
        WindowCrafting.Instance.Init(skills, recipes);
    }
}