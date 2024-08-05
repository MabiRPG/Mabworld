using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class CraftingStation : MonoBehaviour
{
    [SerializeField]
    private int ID;
    private Dictionary<int, List<CraftingRecipe>> recipes = new Dictionary<int, List<CraftingRecipe>>();

    private const string skillQuery = @"SELECT id, skill_id
        FROM crafting_recipe
        WHERE crafting_station_id = @id;";

    private void Awake()
    {
        DataTable dt = GameManager.Instance.QueryDatabase(skillQuery, ("@id", ID));

        foreach (DataRow row in dt.Rows)
        {
            int recipeID = int.Parse(row["id"].ToString());
            int skillID = int.Parse(row["skill_id"].ToString());

            if (recipes.ContainsKey(skillID))
            {
                recipes[skillID].Add(new CraftingRecipe(recipeID));
            }
            else
            {
                recipes.Add(skillID, new List<CraftingRecipe> { new CraftingRecipe(recipeID) });
            }
        }
    }

    private void OnMouseDown()
    {
        // List<Skill> skills = new List<Skill>();

        // foreach (int skillID in availableSkillIDs)
        // {
        //     if (Player.Instance.skillManager.IsLearned(skillID))
        //     {
        //         skills.Add(Player.Instance.skillManager.Get(skillID));
        //     }
        // }

        List<Skill> skills = new List<Skill>();

        foreach (int skillID in recipes.Keys)
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