using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
///     Handles the individual item processing.
/// </summary>
[JsonObject]
public class Item
{
    public ItemModel model;
    public int quantity;
    public Dictionary<int, float> stats = new Dictionary<int, float>();

    [JsonConstructor]
    public Item() { }

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    /// <param name="ID">Item ID in database.</param>
    public Item(int ID)
    {
        model = new ItemModel(GameManager.Instance.Database, ID);

        foreach ((int statID, ItemStatModel statModel) in model.stats)
        {
            float roll = UnityEngine.Random.Range((int)statModel.min, (int)statModel.max + 1);
            stats.Add(statID, roll);
        }
    }
}