using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

/// <summary>
///     Handles all interactable objects in the world.
/// </summary>
public class MapResource : MonoBehaviour
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
    private bool isMovable;

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

    /// <summary>
    ///     Called when mouse clicked on collider.
    /// </summary>
    private void OnMouseDown()
    {
        if (resource.Value == 0)
        {
            return;
        }

        Skill playerSkill = Player.Instance.skillManager.Get(skillID);

        if (rankRequired == null || !playerSkill.IsRankOrGreater(rankRequired))
        {
            return;
        }

        resultHandler.SetResource(playerSkill, lootTableID, resource.Value);

        // Player.Instance.AddToQueue(new List<Action>{
        //     () => Player.Instance.MoveToPosition(transform.TransformPoint(Vector2.left * 0.5f)),
        //     () => {Player.Instance.Orientate(new Vector2(1, 0)); Player.Instance.AdvanceQueue();},
        //     () => Player.Instance.LoadSkill(playerSkill),
        //     () => Player.Instance.UseSkill(resultHandler)
        // });

        StartCoroutine(Interact(playerSkill));
    }

    private IEnumerator Interact(Skill playerSkill)
    {
        // Player.Instance.movementController.PauseUpdate = true;

        MoveState moveState = new MoveState(Player.Instance.movementController);
        SkillLoadState loadState = new SkillLoadState(Player.Instance.skillController, playerSkill);
        moveState.exitAction += () =>
        {
            Player.Instance.Orientate(new Vector2(1, 0));
            Player.Instance.skillController.SetResultHandler(resultHandler);
            Player.Instance.skillController.SetState(loadState);
        };

        Player.Instance.movementController.PathToPosition(transform.TransformPoint(Vector2.left * 0.5f));

        while (!Player.Instance.navMeshAgent.hasPath)
        {
            yield return null;
        }

        Player.Instance.movementController.SetState(moveState);

        while (Player.Instance.movementController.State is not IdleState 
            && Player.Instance.skillController.State is not IdleState)
        {
            yield return null;
        } 

        // Player.Instance.movementController.PauseUpdate = false;
    }

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
}
