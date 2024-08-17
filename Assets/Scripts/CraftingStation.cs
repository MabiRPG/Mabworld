using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.EventSystems;

public class CraftingStation : MonoBehaviour, IInputHandler
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

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (recipes.Count == 0)
            {
                return;
            }

            List<Skill> skills = new List<Skill>();

            foreach (int skillID in recipes.Keys)
            {
                if (Player.Instance.skillManager.IsLearned(skillID))
                {
                    skills.Add(Player.Instance.skillManager.Get(skillID));
                }
            }

            WindowManager.Instance.ToggleWindow(WindowCrafting.Instance);
            WindowCrafting.Instance.Init(skills, recipes);
        }
    }

    public void HandleKeyboardInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
    }
}