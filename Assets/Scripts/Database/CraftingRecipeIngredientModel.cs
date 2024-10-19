public class CraftingRecipeIngredientModel : Model
{
    public int recipeID;
    public int itemID;
    public int quantity;

    public ItemModel item;

    public CraftingRecipeIngredientModel(DatabaseManager database, int recipeID, int itemID)
        : base(database)
    {
        this.recipeID = recipeID;
        this.itemID = itemID;
        tableName = "crafting_recipe_ingredient";

        primaryKeys.Add("crafting_recipe_id");
        primaryKeys.Add("item_id");

        fieldMap.Add("crafting_recipe_id", new ModelFieldReference(this, nameof(recipeID)));
        fieldMap.Add("item_id", new ModelFieldReference(this, nameof(itemID)));
        fieldMap.Add("item_quantity", new ModelFieldReference(this, nameof(quantity)));

        CreateReadQuery();

        ReadRow();
        item = new ItemModel(database, itemID);
    }
}