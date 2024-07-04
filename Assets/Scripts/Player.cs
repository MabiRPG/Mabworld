using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

/// <summary>
///     Handles all player & input processing.
/// </summary>
public class Player : Actor
{
    // Global instance of player
    public static Player Instance = null;

    // Ability points and experience
    public IntManager actorAP = new IntManager(0);
    public StatManager actorXP = new StatManager(0, 100, 100);
    // Inventory
    public InventoryManager inventoryManager = new InventoryManager();

    // How much our life skill success rates scale with dex.
    private int lifeSkillDexFactor = 10;
    // What the maximize success rate increase is.
    private int lifeSkillSuccessCap = 18;

    // How much our lucky gathers scale with luck stat
    private int luckyFactor = 20;
    // How much resource multiplier is applied on trigger lucky
    private int luckyGain = 2;
    private int hugeLuckyFactor = 50000;
    private int hugeLuckyGain = 20;

    public GameObject map;
    public event Action<MapResourceResultHandler> trainingEvent;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        // Singleton recipe
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    ///     Called after all Awakes.
    /// </summary>
    protected void Start()
    {
        // Debug purposes...
        actorName.Value = "Test";
        skillManager.Learn(1);
        skillManager.Learn(2);
        skillManager.Learn(3);
        skillManager.Learn(4);
    }

    /// <summary>
    ///     Triggered whenever colliding with another collider 2D.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out MapResource target))
        {
            //Debug.Log(target);
        }
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (horizontal != 0 || vertical != 0)
        {
            AttemptMove(new Vector2(horizontal, vertical));
        }
    }

    private void AttemptMove(Vector2 direction)
    {
        if (state != State.Idle)
        {
            return;
        }

        // Must be initialized with some length or it won't detect anything!
        RaycastHit2D[] results = new RaycastHit2D[10];
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(blockingLayer);
        boxCollider.Cast(direction, filter, results);

        Vector2 closestPoint = boxCollider.ClosestPoint(results[0].point);
        Vector2 target = (results[0].point - closestPoint) * (direction * direction);

        // Find the smallest distance movable by a magnitude of 1.
        if (target.sqrMagnitude > direction.sqrMagnitude)
        {
            target = direction;
        }
        else if (target.sqrMagnitude == 0)
        {
            return;
        }

        actorCoroutine = Move(transform.position + (Vector3)target);
        StartCoroutine(actorCoroutine);
    }

    private IEnumerator Move(Vector3 target)
    {
        state = State.Moving;
        animator.SetBool("isMoving", true);

        float sqdRemainingDistance = (transform.position - target).sqrMagnitude;

        while (sqdRemainingDistance > 0.1)
        {
            Vector3 nextPos = Vector3.MoveTowards(transform.position, target, 0.1f);
            Vector2 diff = nextPos - transform.position;
            rigidBody2d.MovePosition(nextPos);
            animator.SetFloat("moveX", diff.x);
            animator.SetFloat("moveY", diff.y);

            sqdRemainingDistance = (transform.position - target).sqrMagnitude;
            yield return null;
        }

        rigidBody2d.MovePosition(target);
        animator.SetBool("isMoving", false);
        moveEvent.RaiseOnChange();
    }

    /// <summary>
    ///     Toggles the minimap.
    /// </summary>
    public void ToggleMap()
    {
        map.SetActive(!map.activeSelf);
    }

    /// <summary>
    ///     Ranks up the skill
    /// </summary>
    /// <param name="ID">Skill ID in database</param>
    public void RankUpSkill(int ID)
    {
        if (skillManager.IsLearned(ID))
        {
            RankUpSkill(skillManager.Get(ID));
        }
    }

    /// <summary>
    ///     Ranks up the skill
    /// </summary>
    /// <param name="skill">Skill instance</param>
    public void RankUpSkill(Skill skill)
    {
        int apCost = (int)skill.GetStatForwardDiff("ap_cost");

        if (skillManager.IsLearned(skill) && skill.CanRankUp() && actorAP.Value >= apCost)
        {
            actorAP.Value -= apCost;
            skill.RankUp();
            actorLevel.Value += apCost;

            foreach(KeyValuePair<string, StatManager> stat in primaryStats)
            {
                int statAdd = (int)skill.GetStatBackwardDiff(stat.Key);
                stat.Value.Value += statAdd;
            }
        }        
    }

    /// <summary>
    ///     Adds xp to the player.
    /// </summary>
    /// <param name="x">Amount of xp to add.</param>
    public void AddXP(float x)
    {
        actorXP.Value += x;

        if (actorXP.Value >= actorXP.Maximum)
        {
            actorAP.Value++;
            actorXP.Value -= actorXP.Maximum;
        }
    }

    /// <summary>
    ///     Calculates the player's life skill success rate bonus
    /// </summary>
    /// <returns>Bonus rate as percentage</returns>
    public float CalculateLifeSkillSuccessRate()
    {
        return Math.Min(actorDex.Value / lifeSkillDexFactor, lifeSkillSuccessCap);
    }

    /// <summary>
    ///     Calculates the lucky resource gain factor
    /// </summary>
    /// <returns>Resource gain multiplier</returns>
    public int CalculateLuckyGainMultiplier() 
    {
        float lucky = (float)actorLuck.Value / luckyFactor;
        float hugeLucky = (float)actorLuck.Value / hugeLuckyFactor;
        float roll = UnityEngine.Random.Range(0f, 1f);

        if (hugeLucky >= roll) 
        {
            return hugeLuckyGain;
        }
        else if (lucky >= roll) 
        {
            return luckyGain;
        }

        return 1;
    }

    public void MapResourceRaiseOnChange(MapResourceResultHandler sender)
    {
        trainingEvent(sender);
    }
}