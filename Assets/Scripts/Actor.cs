using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
///     Handles all actor-level info and calculations.
/// </summary>
public class Actor : MonoBehaviour
{
    [SerializeField]
    protected LayerMask blockingLayer;
    protected BoxCollider2D boxCollider;
    protected Rigidbody2D rigidBody2d;
    protected Animator animator;
    protected NavMeshAgent navMeshAgent;

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
        Moving,
        SkillLoading,
        SkillLoaded,
        SkillCancelling,
        SkillUsing
    }
    public State state = State.Idle;
    protected Queue<Action> actionQueue = new Queue<Action>();

    public SkillManager skillManager;
    public IEnumerator actorCoroutine;
    public Skill skillLoaded;

    public EventManager moveEvent = new EventManager();

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    protected virtual void Awake()
    {
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

        boxCollider = GetComponent<BoxCollider2D>(); 
        rigidBody2d = GetComponent<Rigidbody2D>();
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
    private void OnEnable()
    {
        actorStr.OnBaseMaximumValueChange += CalculateDefense;
        actorStr.OnBaseMaximumValueChange += CalculateMDefense;
        actorInt.OnBaseMaximumValueChange += CalculateMProt;

        moveEvent.OnChange += OnMoved;
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

        moveEvent.OnChange -= OnMoved;
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

    /// <summary>
    ///     Called when leaving Actor State.Moving.
    /// </summary>
    private void OnMoved()
    {
        actorCoroutine = null;
        state = State.Idle;
        AdvanceQueue();
    }

    /// <summary>
    ///     Called to load a skill on the actor.
    /// </summary>
    /// <param name="skill">Skill instance to load.</param>
    public void LoadSkill(Skill skill)
    {
        if (state != State.Idle || skill.cooldown.Value != 0)
        {
            return;
        }

        actorCoroutine = skillManager.Ready(skill);
        skillLoaded = skill;

        StartCoroutine(actorCoroutine);
        state = State.SkillLoading;
    }

    /// <summary>
    ///     Called when leaving Actor State.Loading.
    /// </summary>
    private void OnLoaded()
    {
        state = State.SkillLoaded;
        AdvanceQueue();
    }

    /// <summary>
    ///     Called to cancel the current skill on the actor.
    /// </summary>
    public void CancelLoadSkill()
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

    /// <summary>
    ///     Called when leaving Actor State.Cancelling.
    /// </summary>
    private void OnCancelled()
    {
        state = State.Idle;
    }

    /// <summary>
    ///     Called to use the loaded skill on the target.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resultHandler">ResultHandler instance of the target.</param>
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

    /// <summary>
    ///     Called when leaving Actor State.SkillUsing.
    /// </summary>
    public void OnUsed()
    {
        actorCoroutine = null;
        CooldownSkill(skillLoaded);
        skillLoaded = null;
        skillManager.bubble.Hide();
        state = State.Idle;
        AdvanceQueue();
    }

    public void CancelUseSkill()
    {
        if (state != State.SkillUsing)
        {
            return;
        }

        StopCoroutine(actorCoroutine);

        actorCoroutine = skillManager.Cancel();
        skillLoaded = null;

        StartCoroutine(actorCoroutine);
        state = State.SkillCancelling;
    }

    /// <summary>
    ///     Called to put the skill on cooldown.
    /// </summary>
    /// <param name="skill">Skill instance to cooldown.</param>
    public void CooldownSkill(Skill skill)
    {
        StartCoroutine(skillManager.Cooldown(skill));
    }

    public void AddToQueue(List<Action> actions)
    {
        foreach (Action action in actions)
        {
            actionQueue.Enqueue(action);
        }
    }

    public void AdvanceQueue()
    {
        if (actionQueue.Count > 0)
        {
            actionQueue.Dequeue().Invoke();
        }
    }

    public void InterruptAction()
    {
        switch (state)
        {
            case State.SkillLoading:
                CancelLoadSkill();
                break;
            case State.SkillLoaded:
                CancelLoadSkill();
                break;
            case State.SkillUsing:
                CancelUseSkill();
                break;
        }

        actionQueue.Clear();
    }
}
