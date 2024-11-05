using System.Data;

/// <summary>
///     Handles all loot tables, rng, and item generation. 
/// </summary>
public class LootGenerator
{
    private DataTable lootTable;
    private const string lootTableQuery = @"SELECT * FROM loot_table WHERE id = @id;";

    /// <summary>
    ///     Loads the loot table from the database.
    /// </summary>
    /// <param name="lootTableID">Loot Table ID in database.</param>
    public void SetLootTable(int lootTableID)
    {
        lootTable = GameManager.Instance.QueryDatabase(lootTableQuery, ("@id", lootTableID));
        NormalizeDataTable(lootTable);
    }

    /// <summary>
    ///     Normalizes the cumulative probabilities in the loot table to be 1, if necessary.
    /// </summary>
    /// <param name="dt"></param>
    private void NormalizeDataTable(DataTable dt)
    {
        // First, we add up the cumulative probability across all rows.
        float totalProbability = 0f;

        foreach (DataRow row in dt.Rows)
        {
            totalProbability += float.Parse(row["probability"].ToString());
        }

        // If it is greater than one, we divide every probability by the total,
        // to normalize everything back to 1. This ensures our relative probabilities
        // do not exceed 0->1.
        if (totalProbability > 1)
        {
            foreach (DataRow row in dt.Rows)
            {
                float normalizedProbability = float.Parse(row["probability"].ToString()) / totalProbability;
                row["probability"] = normalizedProbability;
            }
        }
    }

    /// <summary>
    ///     Randomly generates loot according to the loot table, and gives it to the player.
    /// </summary>
    /// <returns>(Resource ID in database, how much of the resource was provided).</returns>
    public (int resourceID, int resourceGain) Generate()
    {
        float roll = UnityEngine.Random.Range(0f, 1f);
        float i = 0f;
        int resourceID = -1;

        foreach (DataRow row in lootTable.Rows)
        {
            float probability = i + float.Parse(row["probability"].ToString());

            // The rolled probability must be within the window of probabilities
            // to land on it properly.
            if (i <= roll && roll <= probability)
            {
                resourceID = int.Parse(row["item_id"].ToString()); 
                break;
            }
            
            i += probability;
        }

        // Multiply the resource gain by the player's lucky multiplier.
        int resourceGain = 1 * Player.Instance.CalculateLuckyGainMultiplier();
        Player.Instance.inventoryManager.AddItem(resourceID, resourceGain);
        return (resourceID, resourceGain);
    }
}