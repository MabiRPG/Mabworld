using System.Data;

public class LootGenerator
{
    private DataTable lootTable;
    private const string lootTableQuery = @"SELECT * FROM loot_table WHERE id = @id;";

    public void SetLootTable(int lootTableID)
    {
        lootTable = GameManager.Instance.QueryDatabase(lootTableQuery, ("@id", lootTableID));
        NormalizeDataTable(lootTable);
    }

    private void NormalizeDataTable(DataTable dt)
    {
        float totalProbability = 0f;

        foreach (DataRow row in dt.Rows)
        {
            totalProbability += float.Parse(row["probability"].ToString());
        }

        if (totalProbability > 1)
        {
            foreach (DataRow row in dt.Rows)
            {
                float normalizedProbability = float.Parse(row["probability"].ToString()) / totalProbability;
                row["probability"] = normalizedProbability;
            }
        }
    }

    public (int resourceID, int resourceGain) Generate()
    {
        float roll = (float)GameManager.Instance.rnd.NextDouble();
        float i = 0f;
        int resourceID = -1;

        foreach (DataRow row in lootTable.Rows)
        {
            float probability = i + float.Parse(row["probability"].ToString());

            if (i <= roll && roll <= probability)
            {
                resourceID = int.Parse(row["item_id"].ToString()); 
                break;
            }
            
            i += probability;
        }

        int resourceGain = Player.Instance.CalculateLuckyGainMultiplier();
        Player.Instance.inventory.AddItem(resourceID, resourceGain);
        return (resourceID, resourceGain);
    }
}