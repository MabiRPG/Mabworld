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

    private void Awake()
    {
        raycaster = GameManager.Instance.canvas.GetComponent<GraphicRaycaster>();
        parentRectTransform = gameObject.transform.parent.parent.GetComponent<RectTransform>();

        recipePrefabs = ScriptableObject.CreateInstance<PrefabFactory>();
        recipePrefabs.SetPrefab(recipePrefab);
    }

    public void Populate(List<CraftingRecipe> recipes, Action<CraftingRecipe> expandDetailsAction)
    {
        if (recipes.Count == 0)
        {
            return;
        }

        this.recipes = recipes;
        this.expandDetailsAction = expandDetailsAction;

        recipePrefabs.SetActiveAll(false);

        foreach (CraftingRecipe recipe in recipes)
        {
            GameObject obj = recipePrefabs.GetFree(recipe, transform);
            WindowCraftingRecipeItem recipeScript = obj.GetComponent<WindowCraftingRecipeItem>();
            recipeScript.SetRecipe(recipe);
        }

        RectTransform rectTransform = recipePrefab.GetComponent<RectTransform>();
        int columnConstraint = GetComponent<GridLayoutGroup>().constraintCount;

        Debug.Log($"{rectTransform.sizeDelta.y} {recipes.Count}  {columnConstraint}");

        if (recipes.Count % columnConstraint < 3)
        {
            parentRectTransform.sizeDelta = new Vector2(
                parentRectTransform.sizeDelta.x,
                rectTransform.sizeDelta.y * ((recipes.Count % columnConstraint) + 1) + 5);
        }
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