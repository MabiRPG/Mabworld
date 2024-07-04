using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Handles all actor-level info and calculations.
/// </summary>
public class Actor : Movement
{
    // Name and level of actor
    public StringManager actorName = new StringManager();
    public IntManager actorLevel = new IntManager();

    // Health and mana
    public StatManager actorHP = new StatManager(100, 100, 100);
    public StatManager actorMP = new StatManager(100, 100, 100);

    // Strength, Intelligence, Dexterity, and Luck
    public StatManager actorStr = new StatManager(10, 10, 10);
    public StatManager actorInt = new StatManager(10, 10, 10);
    public StatManager actorDex = new StatManager(10, 10, 10);
    public StatManager actorLuck = new StatManager(10, 10, 10);

    // Defense, Protection, Magic Defense, and Magic Protection
    public StatManager actorDefense = new StatManager();
    public StatManager actorProt = new StatManager();
    public StatManager actorMDefense = new StatManager();
    public StatManager actorMProt = new StatManager();

    // Dict of primary and secondary (calculated) stats for easier reference
    public Dictionary<string, StatManager> primaryStats = new Dictionary<string, StatManager>();
    public Dictionary<string, StatManager> secondaryStats = new Dictionary<string, StatManager>();

    // How much defense scales with strength
    protected int defenseStrFactor = 10;
    // Magic defense scale with str
    protected int mDefenseStrFactor = 10;
    // Magic Protection scale with int
    protected int mProtIntFactor = 20; 

    public enum State
    {
        Idle,
        SkillLoading,
        SkillLoaded,
        SkillCancelling,
        SkillUsing
    }
    public State state = State.Idle;

    public SkillManager skillManager;
    public IEnumerator actorCoroutine;
    public Skill skillLoaded;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        primaryStats.Add("hp", actorHP);
        primaryStats.Add("mp", actorMP);
        primaryStats.Add("str", actorStr);
        primaryStats.Add("int", actorInt);
        primaryStats.Add("dex", actorDex);
        primaryStats.Add("luck", actorLuck);

        secondaryStats.Add("defense", actorDefense);
        secondaryStats.Add("protection", actorProt);
        secondaryStats.Add("m_defense", actorMDefense);
        secondaryStats.Add("m_protection", actorMProt);

        GameObject obj = Instantiate(GameManager.Instance.skillBubblePrefab, transform);
        SkillBubble bubble = obj.GetComponent<SkillBubble>();
        skillManager = new SkillManager(bubble);
    }

    /// <summary>
    ///     Called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        actorStr.OnBaseMaximumValueChange += CalculateDefense;
        actorStr.OnBaseMaximumValueChange += CalculateMDefense;
        actorInt.OnBaseMaximumValueChange += CalculateMProt;

        skillManager.bubble.readyEvent.OnChange += OnLoaded;
        skillManager.bubble.cancelEvent.OnChange += OnCancelled;
    }

    /// <summary>
    ///     Called when the object becomes disabled and inactive.
    /// </summary>
    private void OnDisable()
    {
        actorStr.OnBaseMaximumValueChange -= CalculateDefense;
        actorStr.OnBaseMaximumValueChange -= CalculateMDefense;
        actorInt.OnBaseMaximumValueChange -= CalculateMProt;

        skillManager.bubble.readyEvent.OnChange -= OnLoaded;
        skillManager.bubble.cancelEvent.OnChange -= OnCancelled;
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

    public void LoadSkill(Skill skill)
    {
        if (state != State.Idle)
        {
            return;
        }

        actorCoroutine = skillManager.Ready(skill);
        skillLoaded = skill;

        StartCoroutine(actorCoroutine);
        state = State.SkillLoading;
    }

    private void OnLoaded()
    {
        state = State.SkillLoaded;
    }

    public void CancelSkill()
    {
        if (state != State.SkillLoading && state != State.SkillLoaded)
        {
            return;
        }

        StopCoroutine(actorCoroutine);
        
        actorCoroutine = skillManager.Cancel();
        skillLoaded = null;

        StartCoroutine(actorCoroutine);
        state = State.SkillCancelling;
    }

    private void OnCancelled()
    {
        state = State.Idle;
    }

    public void UseSkill<T>(T resultHandler) where T : ResultHandler
    {
        if (state != State.SkillLoaded || skillLoaded == null)
        {
            return;
        }

        actorCoroutine = skillManager.Use(skillLoaded, resultHandler);
        
        StartCoroutine(actorCoroutine);
        state = State.SkillUsing;
    }

    public void OnUsed()
    {
        actorCoroutine = null;
        skillLoaded = null;
        skillManager.bubble.Hide();
        state = State.Idle;
    }
}
