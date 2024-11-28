using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using System.Data;
using TypeExtension;

[Serializable]
/// <summary>
///     Handles all player & input processing.
/// </summary>
public class Player : Actor, IInputHandler
{
    // Global instance of player
    public static Player Instance = null;

    // Ability points and experience
    // public IntManager actorAP = new IntManager(0);
    public StatManager actorXP = new StatManager(0, 100, 100);
    // Inventory
    public InventoryManager inventoryManager = new InventoryManager();
    // Quests
    public SerializableDictionary<int, Quest> quests = new SerializableDictionary<int, Quest>();

    [SerializeField]
    // How much our life skill success rates scale with dex.
    private int lifeSkillDexFactor = 10;
    [SerializeField]
    // What the maximize success rate increase is.
    private int lifeSkillSuccessCap = 18;

    [SerializeField]
    // How much our lucky gathers scale with luck stat
    private int luckyFactor = 2000;
    [SerializeField]
    // How much resource multiplier is applied on trigger lucky
    private int luckyGain = 2;
    [SerializeField]
    private int hugeLuckyFactor = 50000;
    [SerializeField]
    private int hugeLuckyGain = 20;

    [NonSerialized]
    public PlayerController controller;

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
            return;
        }

        DontDestroyOnLoad(gameObject);

        controller = gameObject.AddComponent<PlayerController>();
        controller.Init(this);
        GameManager.Instance.audioController.SetPlayer(this);
    }

    /// <summary>
    ///     Called after all Awakes.
    /// </summary>
    protected void Start()
    {
        DataTable dt = GameManager.Instance.Database.Read(@"SELECT id FROM skill
            WHERE is_starting_with = 1");

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            ActionSkillController action = new ActionSkillController(this, this, ID);
            action.Handle();
        }

        Debug();
    }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0))
        {
            if (controller.Task == null)
            {
                controller.movementMachine.Reset();
                controller.movementMachine.PathToCursor();
            }
        }
    }

    public void HandleKeyboardInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        actorSubstage.OnChange += () =>
        {
            foreach (Quest quest in quests.Values)
            {
                quest.Update(this);
            }
        };
    }

    public void Update()
    {
        if (navMeshAgent.hasPath)
        {
            //Debug.Log($"{navMeshAgent.destination} {navMeshAgent.pathEndPosition} {navMeshAgent.pathStatus} {controller.movementMachine.State.GetType()}");

            if (navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                controller.movementMachine.SetState(new MoveState(controller.movementMachine));
            }
            else
            {
                animator.SetBool("isMoving", false);
                navMeshAgent.SetDestination(transform.position);
                controller.movementMachine.Reset();
            }
        }
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
        // int apCost = (int)skill.GetStatForwardDiff("ap_cost");

        if (skillManager.IsLearned(skill) && skill.CanRankUp())// && actorAP.Value >= apCost)
        {
            // actorAP.Value -= apCost;
            skill.RankUp();
            // actorLevel.Value += apCost;

            foreach (KeyValuePair<string, StatManager> stat in primaryStats)
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
            actorXP.Value -= actorXP.Maximum;

            if (actorSubstage.Value + 1 > actorSubstage.Maximum)
            {
                actorStage.Value++;
                actorSubstage.Value = 1;
            }
            else
            {
                actorSubstage.Value++;
            }
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

    public bool IsQuestStarted(int ID)
    {
        if (quests.ContainsKey(ID))
        {
            return true;
        }

        return false;
    }

    public void AddQuest(int ID)
    {
        if (!IsQuestStarted(ID))
        {
            quests.Add(ID, new Quest(ID));
        }
    }

    private void Debug()
    {
        // Debug purposes...
        actorName.Value = "Test";

        Quest quest = new Quest(1);
        quests.Add(1, quest);
        quests.Add(2, new Quest(2));

        skillManager.Skills[1].AddXP(100);
        skillManager.Skills[2].AddXP(150);

        ActionItemController actionItem = new ActionItemController(this, this, 1, 50);
        actionItem.Handle();
    }
}