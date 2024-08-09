using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WindowCraftingRecipeList : MonoBehaviour
{
    [SerializeField]
    private GameObject recipePrefab;
    private PrefabFactory recipePrefabs;

    private List<CraftingRecipe> recipes;
    private Action<CraftingRecipe> expandDetailsAction;

    private GraphicRaycaster raycaster;
    private RectTransform parentRectTransform;
    int columnConstraint;

    private void Awake()
    {
        raycaster = GameManager.Instance.canvas.GetComponent<GraphicRaycaster>();
        parentRectTransform = gameObject.transform.parent.parent.GetComponent<RectTransform>();
        columnConstraint = GetComponent<GridLayoutGroup>().constraintCount;

        recipePrefabs = ScriptableObject.CreateInstance<PrefabFactory>();
        recipePrefabs.SetPrefab(recipePrefab);
    }

    public void Populate(List<CraftingRecipe> recipes, Action<CraftingRecipe> expandDetailsAction)
    {
        if (recipes.Count == 0)
        {
            recipePrefabs.SetActiveAll(false);
            return;
        }

        this.recipes = recipes;
        this.expandDetailsAction = expandDetailsAction;

        recipePrefabs.SetActiveAll(false);
        GameObject obj = null;

        foreach (CraftingRecipe recipe in recipes)
        {
            obj = recipePrefabs.GetFree(recipe, transform);
            WindowCraftingRecipeItem recipeScript = obj.GetComponent<WindowCraftingRecipeItem>();
            recipeScript.SetRecipe(recipe);
        }

        // Resets the content size fitter.
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        RectTransform rectTransform = obj.GetComponent<RectTransform>();
        // Adjusting the recipe list to be at most 3 rows deep, and shrinking as necessary.
        parentRectTransform.sizeDelta = new Vector2(
            parentRectTransform.sizeDelta.x,
            rectTransform.sizeDelta.y * Math.Min((recipes.Count % columnConstraint) + 1, 3) + 10);
    }

    private void Update()
    {
        if (expandDetailsAction == null)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            // Stores all the results of our raycasts
            List<RaycastResult> hits = new List<RaycastResult>();
            // Create a new pointer data for our raycast manipulation
            PointerEventData pointerData = new PointerEventData(GetComponent<EventSystem>());
            pointerData.position = Input.mousePosition;
            raycaster.Raycast(pointerData, hits);

            foreach (RaycastResult hit in hits)
            {
                if (hit.gameObject.TryGetComponent(out WindowCraftingRecipeItem recipeScript))
                {
                    CraftingRecipe recipe = recipeScript.recipe;
                    expandDetailsAction?.Invoke(recipe);
                    break;
                }
            }
        }
    }
}