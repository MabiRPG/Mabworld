using System;
using System.Collections;
using System.Data;
using UnityEngine;

/// <summary>
///     Handles all interactable objects in the world.
/// </summary>
public class InteractableObject : MonoBehaviour
{
    // Primary key for event
    public int ID;
    // Skill associated
    [System.NonSerialized]
    public int skillID;
    // Rank restrictions on skill to gather
    [System.NonSerialized]
    public string rankRequired;
    [System.NonSerialized]
    public float successRateModifier;
    // Label to display in world
    [System.NonSerialized]
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
        // Set up initial state.
        ChangeSpriteState();
    }

    /// <summary>
    ///     Called when the object becomes disabled and inactive.
    /// </summary>
    private void OnDisable()
    {
        resource.Clear();
    }

    private void OnMouseDown()
    {
        Interact();
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
        if (!isRegening)
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

    /// <summary>
    ///     Called when player interacts with the object in the world.
    /// </summary>
    private void Interact()
    {
        if (resource.Value == 0)
        {
            return;
        }

        if (Player.Instance.IsSkillLearned(skillID))
        {
            if (rankRequired != null && Player.Instance.skills[skillID].IsRankOrGreater(rankRequired))
            {
                Result result = Player.Instance.result;
                result.Clear();
                result.lootTableID = lootTableID;
                result.type = Result.Type.Gather;
                result.mapEvent.OnChange += UpdateResource;

                Player.Instance.StartAction(Player.Instance.skills[skillID]);
            }
        }
    }

    private void UpdateResource()
    {
        Result result = Player.Instance.result;

        if (result.isSuccess)
        {
            resource.Value--;
        }
    }
}
