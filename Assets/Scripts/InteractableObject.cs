using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    // Primary key for event
    public int ID;
    // Skill associated
    public int skillID;
    // Rank restrictions on skill to gather
    public string rankRequired;
    public float successRateModifier;
    // Label to display in world
    public string name;
    // Sprites to display depending on state of resource
    public Sprite fullSprite;
    public Sprite partialFullSprite;
    public Sprite emptySprite;
    // Mouse hover sprite
    public Sprite mouseHoverSprite;
    // Sound effect when interacting
    public AudioClip sfx;
    // How much the resource currently has
    public ValueManager resource;
    // Maximum capacity of resource
    public int resourceMax;
    // How much it regenerates per interval, and interval duration
    public int resourceRegenPerInterval;
    public int resourceRegenInterval;
    // What loot table this resource draws from
    public int lootTableID;
    public bool isMovable;

    private const string eventQuery = @"SELECT * FROM map_resources WHERE event_id = @id LIMIT 1;";

    private void Start()
    {
        DataTable dt = GameManager.Instance.QueryDatabase(eventQuery, ("@id", ID));
        DataRow row = dt.Rows[0];
        GameManager.Instance.ParseDatabaseRow(row, this);
    }
}
