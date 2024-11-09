using UnityEngine;

public class MapResourceModel : Model
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
    public int resource;
    // Maximum capacity of resource
    public int resourceMax;
    // How much it regenerates per interval, and interval duration
    public int resourceRegenPerInterval;
    public int resourceRegenInterval;
    // What loot table this resource draws from
    public int lootTableID;

    public MapResourceModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "map_resource";

        primaryKeys.Add("id");

        fieldMap.Add("id", new ModelFieldReference(this, nameof(this.ID)));
        fieldMap.Add("skill_id", new ModelFieldReference(this, nameof(skillID)));
        fieldMap.Add("rank_required", new ModelFieldReference(this, nameof(rankRequired)));
        fieldMap.Add("success_rate_modifier", new ModelFieldReference(this, nameof(successRateModifier)));
        fieldMap.Add("name", new ModelFieldReference(this, nameof(name)));
        fieldMap.Add("full_sprite", new ModelFieldReference(this, nameof(fullSprite)));
        fieldMap.Add("partial_full_sprite", new ModelFieldReference(this, nameof(partialFullSprite)));
        fieldMap.Add("empty_sprite", new ModelFieldReference(this, nameof(emptySprite)));
        fieldMap.Add("mouse_hover_sprite", new ModelFieldReference(this, nameof(mouseHoverSprite)));
        fieldMap.Add("sfx", new ModelFieldReference(this, nameof(sfx)));
        fieldMap.Add("resource", new ModelFieldReference(this, nameof(resource)));
        fieldMap.Add("resource_max", new ModelFieldReference(this, nameof(resourceMax)));
        fieldMap.Add("resource_regen_per_interval", new ModelFieldReference(this, nameof(resourceRegenPerInterval)));
        fieldMap.Add("resource_regen_interval", new ModelFieldReference(this, nameof(resourceRegenInterval)));
        fieldMap.Add("loot_table_id", new ModelFieldReference(this, nameof(lootTableID)));

        CreateReadQuery();
        CreateWriteQuery();

        ReadRow();
    }
}