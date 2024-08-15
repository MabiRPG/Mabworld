using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
///     Handles all interactable objects in the world.
/// </summary>
public class MapResource : MonoBehaviour, IInputHandler
{
    // Primary key for event
    public int ID;
    // Skill associated
    [HideInInspector]
    public int skillID;
    // Rank restrictions on skill to gather
    [HideInInspector]
    public string rankRequired;
    [HideInInspector]
    public float successRateModifier;
    // Label to display in world
    [HideInInspector]
    public string sName;
    // Sprites to display depending on state of resource
    private Sprite fullSprite;
    private Sprite partialFullSprite;
    private Sprite emptySprite;
    // Mouse hover sprite
    private Sprite mouseHoverSprite;
    // Sound effect when interacting
    private AudioClip sfx;
    // How much the resource currently has
    private IntManager resource;
    // Maximum capacity of resource
    private int resourceMax;
    // How much it regenerates per interval, and interval duration
    private int resourceRegenPerInterval;
    private int resourceRegenInterval;
    // What loot table this resource draws from
    private int lootTableID;

    private SpriteRenderer spriteRenderer;
    private bool isRegening;

    private const string eventQuery = @"SELECT * FROM map_resource WHERE id = @id LIMIT 1;";
    private MapResourceResultHandler resultHandler = new MapResourceResultHandler();

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    private void Awake()
    {
        // Fetch event info from database.
        DataTable dt = GameManager.Instance.QueryDatabase(eventQuery, ("@id", ID));
        DataRow row = dt.Rows[0];
        GameManager.Instance.ParseDatabaseRow(row, this, 
            ("loot_table_id", "lootTableID"), ("name", "sName"), ("skill_id", "skillID"));

        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    /// <summary>
    ///     Called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        // Add event hook
        resource.OnChange += ChangeSpriteState;
        resultHandler.mapEvent.OnChange += UpdateResource;
        // Set up initial state.
        ChangeSpriteState();
    }

    /// <summary>
    ///     Called when the object becomes disabled and inactive.
    /// </summary>
    private void OnDisable()
    {
        resource.Clear();
        resultHandler.mapEvent.Clear();
    }

    // /// <summary>
    // ///     Called when mouse clicked on collider.
    // /// </summary>
    // private void OnMouseDown()
    // {
    //     if (resource.Value == 0)
    //     {
    //         return;
    //     }
    //     else if (!GameManager.Instance.isCanvasEmptyUnderMouse)
    //     {
    //         return;
    //     }

    //     Skill playerSkill = Player.Instance.skillManager.Get(skillID);

    //     if (rankRequired == null || !playerSkill.IsRankOrGreater(rankRequired))
    //     {
    //         return;
    //     }

    //     resultHandler.SetResource(playerSkill, lootTableID, resource.Value);

    //     Vector3 position = transform.TransformPoint(Vector3.zero);
    //     IEnumerator task = Player.Instance.controller.HarvestResource(position, playerSkill, resultHandler);
    //     Player.Instance.controller.SetTask(task);
    // }

    /// <summary>
    ///     Changes the sprite state depending on the resource amount.
    /// </summary>
    private void ChangeSpriteState()
    {
        // If resource is full, set sprite, stop regeneration
        if (resource.Value == resourceMax)
        {
            spriteRenderer.sprite = fullSprite;
            StopCoroutine(Regenerate());
            return;
        }
        // Otherwise, start regen coroutine
        else if (!isRegening && resource.Value == 0)
        {
            StartCoroutine(Regenerate());
        }

        // If empty, set sprite.
        if (resource.Value == 0)
        {
            spriteRenderer.sprite = emptySprite;
        }
        // If partial full sprite exists, use it, otherwise default to full.
        else
        {
            if (partialFullSprite != null)
            {
                spriteRenderer.sprite = partialFullSprite;
            }
            else
            {
                spriteRenderer.sprite = fullSprite;
            }
        }
    }

    /// <summary>
    ///     Begins regenerating the resource if necessary through a coroutine.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Regenerate()
    {
        isRegening = true;

        while (resource.Value < resourceMax)
        {
            yield return new WaitForSeconds(resourceRegenInterval);
            resource.Value = Math.Min(resourceMax, resource.Value + resourceRegenPerInterval);
        }

        isRegening = false;
    }

    private void UpdateResource()
    {
        if (resultHandler.isSuccess)
        {
            resource.Value--;
        }
    }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (resource.Value == 0)
            {
                return;
            }

            Skill playerSkill = Player.Instance.skillManager.Get(skillID);

            if (rankRequired == null || !playerSkill.IsRankOrGreater(rankRequired))
            {
                Player.Instance.HandleMouseInput(graphicHits, sceneHits);
                return;
            }

            resultHandler.SetResource(playerSkill, lootTableID, resource.Value);

            Vector3 position = transform.TransformPoint(Vector3.zero);
            IEnumerator task = Player.Instance.controller.HarvestResource(position, playerSkill, resultHandler);
            Player.Instance.controller.SetTask(task);            
        }
    }

    public void HandleKeyboardInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        throw new NotImplementedException();
    }
}
