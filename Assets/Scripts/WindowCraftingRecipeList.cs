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

    private Dictionary<Item, List<Item>> recipes;
    private Action<Item, List<Item>> expandDetailsAction;

    private GraphicRaycaster raycaster;

    private void Awake()
    {
        raycaster = GameManager.Instance.canvas.GetComponent<GraphicRaycaster>();

        recipePrefabs = ScriptableObject.CreateInstance<PrefabFactory>();
        recipePrefabs.SetPrefab(recipePrefab);
    }

    public void PopulateList(Dictionary<Item, List<Item>> recipes, Action<Item, List<Item>> expandDetailsAction)
    {
        this.recipes = recipes;
        this.expandDetailsAction = expandDetailsAction;

        recipePrefabs.SetActiveAll(false);

        foreach (Item product in recipes.Keys)
        {
            GameObject obj = recipePrefabs.GetFree(product, transform);
            WindowCraftingRecipeItem recipeScript = obj.GetComponent<WindowCraftingRecipeItem>();
            recipeScript.SetRecipe(product);
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
                    Item product = recipeScript.product;
                    List<Item> ingredients = recipes[product];
                    expandDetailsAction?.Invoke(product, ingredients);
                    break;
                }
            }
        }
    }
}