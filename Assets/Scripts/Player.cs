using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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

    /// <summary>
    ///     Called on every frame.
    /// </summary>
    private void Update()
    {
        // Cancels the current movement input and recalculates a new path to the position
        // on left mouse button press
        if (state == State.Moving && Input.GetMouseButtonDown(0))
        {
            navMeshAgent.SetDestination(transform.position);
            StopCoroutine(actorCoroutine);
            moveEvent.RaiseOnChange();

            MoveToCursor();
            return;
        }
        else if (state != State.Idle)
        {
            return;
        }

        if (!navMeshAgent.pathPending && !navMeshAgent.hasPath)
        {
            // float horizontal = Input.GetAxisRaw("Horizontal");
            // float vertical = Input.GetAxisRaw("Vertical");

            // if (horizontal != 0 || vertical != 0)
            // {
            //     Vector3 targetPosition = transform.position + new Vector3(horizontal, vertical, 0f);

            //     if (Mathf.Abs(transform.position.x - targetPosition.x) < 0.0001f)
            //     {
            //         targetPosition += new Vector3(0.0001f, 0.0001f, 0f);
            //     }

            //     NavMeshPath path = new NavMeshPath();
            //     navMeshAgent.CalculatePath(targetPosition, path);
            //     navMeshAgent.SetPath(path);
            //     actorCoroutine = Move();
            //     StartCoroutine(actorCoroutine);

            //     // if (!navMeshAgent.Raycast(targetPosition, out _))
            //     // {
            //     //     actorCoroutine = Move(targetPosition);
            //     //     StartCoroutine(actorCoroutine);
            //     // }
            // }

            if (Input.GetMouseButtonDown(0))
            {
                MoveToCursor();
            }
        }
        // Perform auto-pathing if a path exists.
        else if (navMeshAgent.hasPath)
        {
            actorCoroutine = Move();
            StartCoroutine(actorCoroutine);
        }
    }

    /// <summary>
    ///     Sets the NavMeshAgent destination to the mouse cursor.
    /// </summary>
    private void MoveToCursor()
    {
        Vector3 target = GameManager.Instance.canvas.worldCamera.ScreenToWorldPoint(Input.mousePosition);
        target.z = 0f;
        navMeshAgent.SetDestination(target);        
    }

    /// <summary>
    ///     Moves the NavMeshAgent according to the preset path, and changes the animator states 
    ///     accordingly.
    /// </summary>
    /// <returns>Coroutine to be run.</returns>
    private IEnumerator Move()
    {
        state = State.Moving;
        animator.SetBool("isMoving", true);

        while (navMeshAgent.hasPath)
        {
            Vector2 nextPos = navMeshAgent.nextPosition;
            Vector2 diff = nextPos - (Vector2)transform.position;
            transform.position = nextPos;
            // Set the animator to the relative movement vector
            animator.SetFloat("moveX", diff.x);
            animator.SetFloat("moveY", diff.y);

            yield return null;
        }

        animator.SetBool("isMoving", false);
        // Set the final position exactly to the destination, with the z=0 
        // due to some bug that causes it to be non-zero.
        transform.position = new Vector3(navMeshAgent.destination.x, navMeshAgent.destination.y, 0f);
        // Change states.
        moveEvent.RaiseOnChange();
    }

    private IEnumerator Move(Vector3 targetPosition)
    {
        state = State.Moving;
        animator.SetBool("isMoving", true);

        float sqdRemainingDistance = (transform.position - targetPosition).sqrMagnitude;

        while (sqdRemainingDistance > navMeshAgent.speed * Time.deltaTime)
        {
            Vector3 nextPos = Vector3.MoveTowards(transform.position, targetPosition, navMeshAgent.speed * Time.deltaTime);
            Vector2 diff = nextPos - transform.position;
            transform.position = nextPos;
            // navMeshAgent.Move(diff);
            // Offset required for moving an agent directly through code
            // nextPos.y -= navMeshAgent.baseOffset;
            // navMeshAgent.Warp(nextPos);
            // Set the difference for our animator
            animator.SetFloat("moveX", diff.x);
            animator.SetFloat("moveY", diff.y);

            sqdRemainingDistance = (transform.position - targetPosition).sqrMagnitude;
            yield return null;
        }

        // Set the final positions exactly
        transform.position = targetPosition;
        // target.y -= navMeshAgent.baseOffset;
        // navMeshAgent.Warp(target);
        //navMeshAgent.destination = transform.position;
        animator.SetBool("isMoving", false);
        // Change states
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