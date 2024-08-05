using System.Collections.Generic;
using System.Data;

public class CraftingRecipe
{
    public int ID;
    public int craftingStationID;
    public int skillID;
    public string rankRequired;
    public Item product;
    public List<Item> ingredients = new List<Item>();

    private const string recipeQuery = @"SELECT *
        FROM crafting_recipe
        WHERE id = @id;";

    private const string detailsQuery = @"SELECT 
            crafting_recipe_ingredient.item_id as ""ingredientID"", 
            crafting_recipe_ingredient.item_quantity as ""ingredientQuantity"", 
            crafting_recipe_product.item_id as ""productID"", 
            crafting_recipe_product.item_quantity as ""productQuantity""
        FROM crafting_recipe
        JOIN crafting_recipe_ingredient
        ON crafting_recipe.id = crafting_recipe_ingredient.crafting_recipe_id
        JOIN crafting_recipe_product
        ON crafting_recipe.id = crafting_recipe_product.crafting_recipe_id
        WHERE crafting_recipe.id = @id;";

    public CraftingRecipe(int ID)
    {
        this.ID = ID;
        DataTable dt = GameManager.Instance.QueryDatabase(recipeQuery, ("@id", ID));
        DataRow row = dt.Rows[0];
        GameManager.Instance.ParseDatabaseRow(row, this);

        dt = GameManager.Instance.QueryDatabase(detailsQuery, ("@id", ID));

        foreach (DataRow r in dt.Rows)
        {
            Item ingredient = new Item(int.Parse(r["ingredientID"].ToString()));
            ingredient.quantity = int.Parse(r["ingredientQuantity"].ToString());
            Item product = new Item(int.Parse(r["productID"].ToString()));
            product.quantity = int.Parse(r["productQuantity"].ToString());

            if (this.product == null)
            {
                this.product = product;
            }

            ingredients.Add(ingredient);
        }
    }
}