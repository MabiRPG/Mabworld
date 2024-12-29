using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TypeExtension;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
///     Handles all actor-level info and calculations.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class Actor : MonoBehaviour
{
    [JsonIgnore]
    protected LayerMask blockingLayer;
    [JsonIgnore]
    public Animator animator;
    [JsonIgnore]
    public NavMeshAgent navMeshAgent;

    // Name and level of actor
    [JsonProperty]
    public StringManager actorName = new StringManager();
    [JsonProperty]
    public StatManager actorStage = new StatManager(1, 9, 9);
    [JsonProperty]
    public StatManager actorSubstage = new StatManager(1, 9, 9);

    // Health and mana
    [JsonProperty]
    public StatManager actorHP = new StatManager(100, 100, 100);
    [JsonProperty]
    public StatManager actorMP = new StatManager(100, 100, 100);

    // Strength, Intelligence, Dexterity, and Luck
    [JsonProperty]
    public StatManager actorStr = new StatManager(0, 0, 0);
    [JsonProperty]
    public StatManager actorInt = new StatManager(0, 0, 0);
    [JsonProperty]
    public StatManager actorDex = new StatManager(0, 0, 0);
    [JsonProperty]
    public StatManager actorLuck = new StatManager(0, 0, 0);

    // Defense, Protection, Magic Defense, and Magic Protection
    [JsonProperty]
    public StatManager actorDefense = new StatManager();
    [JsonProperty]
    public StatManager actorProt = new StatManager();
    [JsonProperty]
    public StatManager actorMDefense = new StatManager();
    [JsonProperty]
    public StatManager actorMProt = new StatManager();

    // Dict of primary and secondary (calculated) stats for easier reference
    [JsonIgnore]
    public Dictionary<string, StatManager> primaryStats = new Dictionary<string, StatManager>();
    [JsonIgnore]
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
    [JsonIgnore]
    public SkillBubble bubble;

    // Serialization info
    [JsonProperty]
    private float _x;
    [JsonProperty]
    private float _y;

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
    }

    /// <summary>
    ///     Called when the object becomes enabled and active.
    /// </summary>
    protected virtual void OnEnable()
    {

        actorStr.OnBaseMaximumValueChange += CalculateDefense;
        actorStr.OnBaseMaximumValueChange += CalculateMDefense;
        actorInt.OnBaseMaximumValueChange += CalculateMProt;
    }

    /// <summary>
    ///     Called when the object becomes disabled and inactive.
    /// </summary>
    protected virtual void OnDisable()
    {
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

    [OnSerializing]
    internal void OnSerializing(StreamingContext context)
    {
        _x = navMeshAgent.destination.x;
        _y = navMeshAgent.destination.y;
    }

    [OnDeserialized]
    internal void OnDeserialized(StreamingContext context)
    {
        navMeshAgent.Warp(new Vector2(_x, _y));
    }
}
