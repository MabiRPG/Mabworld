using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
///     Handles all actor-level info and calculations.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class Actor : MonoBehaviour
{
    protected LayerMask blockingLayer;
    public Animator animator;
    public NavMeshAgent navMeshAgent;

    // Name and level of actor
    public StringManager actorName = new StringManager();
    public StatManager actorStage = new StatManager(1, 9, 9);
    public StatManager actorSubstage = new StatManager(1, 9, 9);

    // Health and mana
    public StatManager actorHP = new StatManager(100, 100, 100);
    public StatManager actorMP = new StatManager(100, 100, 100);

    // Strength, Intelligence, Dexterity, and Luck
    public StatManager actorStr = new StatManager(0, 0, 0);
    public StatManager actorInt = new StatManager(0, 0, 0);
    public StatManager actorDex = new StatManager(0, 0, 0);
    public StatManager actorLuck = new StatManager(0, 0, 0);

    // Defense, Protection, Magic Defense, and Magic Protection
    public StatManager actorDefense = new StatManager();
    public StatManager actorProt = new StatManager();
    public StatManager actorMDefense = new StatManager();
    public StatManager actorMProt = new StatManager();

    // Dict of primary and secondary (calculated) stats for easier reference
    public Dictionary<string, StatManager> primaryStats = new Dictionary<string, StatManager>();
    public Dictionary<string, StatManager> secondaryStats = new Dictionary<string, StatManager>();

    [JsonProperty]
    // How much defense scales with strength
    protected int defenseStrFactor = 10;
    [JsonProperty]
    // Magic defense scale with str
    protected int mDefenseStrFactor = 10;
    [JsonProperty]
    // Magic Protection scale with int
    protected int mProtIntFactor = 20;

    [JsonProperty]
    public SkillManager skillManager;
    public SkillBubble bubble;

    // Serialization info
    private string _actorName;
    private float _actorStageValue;
    private float _actorStageMaximum;
    private float _actorStageBaseMaximum;
    private float _actorSubstageValue;
    private float _actorSubstageMaximum;
    private float _actorSubstageBaseMaximum;


    /// <summary>
    ///     Initializes the object.
    /// </summary>
    protected virtual void Awake()
    {
        primaryStats.Add("HP", actorHP);
        primaryStats.Add("MP", actorMP);
        primaryStats.Add("STR", actorStr);
        primaryStats.Add("INT", actorInt);
        primaryStats.Add("DEX", actorDex);
        primaryStats.Add("Luck", actorLuck);

        secondaryStats.Add("Defense", actorDefense);
        secondaryStats.Add("Protection", actorProt);
        secondaryStats.Add("Magic Defense", actorMDefense);
        secondaryStats.Add("Magic Protection", actorMProt);

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        // Adjust skill bubble so it is slightly above the actor head.
        float offsetY = spriteRenderer.bounds.size.y / 2 + 1;
        GameObject obj = Instantiate(GameManager.Instance.skillBubblePrefab,
            transform.TransformPoint(new Vector2(0, offsetY)), Quaternion.identity, transform);
        bubble = obj.GetComponent<SkillBubble>();
        skillManager = new SkillManager(bubble);

        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        transform.rotation = Quaternion.identity;

        UpdateStats();
    }

    /// <summary>
    ///     Called when the object becomes enabled and active.
    /// </summary>
    protected virtual void OnEnable()
    {
        actorStage.OnChange += UpdateStats;
        actorSubstage.OnChange += UpdateStats;
        actorStr.OnBaseMaximumValueChange += CalculateDefense;
        actorStr.OnBaseMaximumValueChange += CalculateMDefense;
        actorInt.OnBaseMaximumValueChange += CalculateMProt;
    }

    /// <summary>
    ///     Called when the object becomes disabled and inactive.
    /// </summary>
    protected virtual void OnDisable()
    {
        actorStage.OnChange -= UpdateStats;
        actorSubstage.OnChange -= UpdateStats;
        actorStr.OnBaseMaximumValueChange -= CalculateDefense;
        actorStr.OnBaseMaximumValueChange -= CalculateMDefense;
        actorInt.OnBaseMaximumValueChange -= CalculateMProt;
    }

    /// <summary>
    ///     Calculates the defense
    /// </summary>
    private void CalculateDefense()
    {
        actorDefense.BaseMaximum = actorStr.BaseMaximum / defenseStrFactor;
    }

    /// <summary>
    ///     Calculates the magic defense
    /// </summary>
    private void CalculateMDefense()
    {
        actorMDefense.BaseMaximum = actorStr.BaseMaximum / mDefenseStrFactor;
    }

    /// <summary>
    ///     Calculates the magic protection
    /// </summary>
    private void CalculateMProt()
    {
        actorMProt.BaseMaximum = actorInt.BaseMaximum / mProtIntFactor;
    }

    private void UpdateStats()
    {
        CultivationStageModel stage = CultivationStageModel.stages
            [((int)actorStage.Value, (int)actorSubstage.Value)];

        actorHP.Value = stage.hp;
        actorMP.Value = stage.mp;
        actorStr.Value = stage.strength;
        actorInt.Value = stage.intelligence;
        actorDex.Value = stage.dexterity;
        actorLuck.Value = stage.luck;
    }
}
