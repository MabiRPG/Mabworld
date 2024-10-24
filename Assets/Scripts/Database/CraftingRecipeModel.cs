using System.Collections.Generic;
using System.Data;

public class CraftingRecipeModel : Model
{
    public int ID;
    public int craftingStationID;
    public int skillID;
    public string rankRequired;

    private string ingredientTableName;
    private string productTableName;

    public Dictionary<int, CraftingRecipeIngredientModel> ingredients = 
        new Dictionary<int, CraftingRecipeIngredientModel>();
    public Dictionary<int, CraftingRecipeProductModel> products =
        new Dictionary<int, CraftingRecipeProductModel>();

    public CraftingRecipeModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "crafting_recipe";
        ingredientTableName = "crafting_recipe_ingredient";
        productTableName = "crafting_recipe_product";

        primaryKeys.Add("id");

        fieldMap.Add("id", new ModelFieldReference(this, nameof(ID)));
        fieldMap.Add("crafting_station_id", new ModelFieldReference(this, nameof(craftingStationID)));
        fieldMap.Add("skill_id", new ModelFieldReference(this, nameof(skillID)));
        fieldMap.Add("rank_required", new ModelFieldReference(this, nameof(rankRequired)));

        CreateReadQuery();
        CreateWriteQuery();

        ReadRow();
        ReadIngredients();
        ReadProducts();
    }

    private void ReadIngredients()
    {
        string query = @$"SELECT item_id
            FROM {ingredientTableName}
            WHERE crafting_recipe_id = @id;";

        DataTable table = database.ReadTable(query, fieldMap);

        foreach (DataRow row in table.Rows)
        {
            int itemID = int.Parse(row["item_id"].ToString());
            ingredients.Add(itemID, new CraftingRecipeIngredientModel(database, ID, itemID));
        }
    }

    private void ReadProducts()
    {
        string query = @$"SELECT item_id
            FROM {productTableName}
            WHERE crafting_recipe_id = @id;";

        DataTable table = database.ReadTable(query, fieldMap);

        foreach (DataRow row in table.Rows)
        {
            int itemID = int.Parse(row["item_id"].ToString());
            products.Add(itemID, new CraftingRecipeProductModel(database, ID, itemID));
        }
    }
}