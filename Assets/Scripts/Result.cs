using System.Data;
using UnityEngine;

/// <summary>
///     Handles all player state results.
/// </summary>
public class Result
{
    // If action was success
    public bool isSuccess;
    // Type of action
    public enum Type
    {
        None,
        Gather
    }
    public Type type;
    public Skill skill;
    // Resource ID and gain if gather action
    public int lootTableID;
    public int resourceGain;
    // Event handler for status
    public EventManager trainingEvent = new EventManager();
    public EventManager mapEvent = new EventManager();

    private const string lootTableQuery = @"SELECT * FROM loot_table WHERE id = @id;";

    public void SetState(bool isSuccess)
    {
        this.isSuccess = isSuccess;

        if (isSuccess && lootTableID != -1)
        {   
            DataTable dt = GameManager.Instance.QueryDatabase(lootTableQuery, ("@id", lootTableID));
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

            float roll = (float)GameManager.Instance.rnd.NextDouble();
            float i = 0f;
            int itemID = -1;

            foreach (DataRow row in dt.Rows)
            {
                float probability = i + float.Parse(row["probability"].ToString());

                if (i <= roll && roll <= probability)
                {
                    itemID = int.Parse(row["item_id"].ToString()); 
                    break;
                }
                
                i += probability;
            }

            if (itemID != -1)
            { 
                resourceGain = Player.Instance.CalculateLuckyGainMultiplier();
                Player.Instance.inventory.Add(itemID, resourceGain);
            }
            else
            {
            }
        }

        trainingEvent.RaiseOnChange();
        mapEvent.RaiseOnChange();
    }

    /// <summary>
    ///     Clears the status.
    /// </summary>
    public void Clear()
    {
        isSuccess = false;
        type = Type.None;
        skill = null;
        lootTableID = -1;
        resourceGain = 1;
        mapEvent.Clear();
    }
}