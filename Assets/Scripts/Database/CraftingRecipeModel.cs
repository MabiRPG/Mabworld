using System.Collections.Generic;

public class CraftingRecipeModel : Model
{
    public int ID;
    public int craftingStationID;
    public int skillID;
    public string rankRequired;

    private string ingredientTableName;
    private string productTableName;

    public List<CraftingRecipeIngredientModel> ingredients = 
        new List<CraftingRecipeIngredientModel>();
    public List<CraftingRecipeProductModel> products =
        new List<CraftingRecipeProductModel>();

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

        ReadRow();
    }

    private void ReadIngredients()
    {
    }
}