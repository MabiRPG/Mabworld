using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using System.Data;
using Newtonsoft.Json;
using System.Collections;

/// <summary>
///     Handles all player & input processing.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class Player : Actor, IInputHandler
{
    // Global instance of player
    [JsonIgnore]
    public static Player Instance = null;

    // Experience
    [JsonProperty]
    public StatManager actorXP = new StatManager(0, 100, 100);
    // Inventory
    [JsonProperty]
    public InventoryManager inventoryManager = new InventoryManager();
    // Quests
    [JsonProperty]
    public Dictionary<int, Quest> quests = new Dictionary<int, Quest>();

    // How much our life skill success rates scale with dex.
    [JsonProperty]
    private int lifeSkillDexFactor = 10;
    // What the maximize success rate increase is.
    [JsonProperty]
    private int lifeSkillSuccessCap = 18;

    // How much our lucky gathers scale with luck stat
    [JsonProperty]
    private int luckyFactor = 2000;
    // How much resource multiplier is applied on trigger lucky
    [JsonProperty]
    private int luckyGain = 2;
    [JsonProperty]
    private int hugeLuckyFactor = 50000;
    [JsonProperty]
    private int hugeLuckyGain = 20;

    [JsonIgnore]
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

        UpdateStats();
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
        actorStage.OnChange += UpdateStats;
        actorSubstage.OnChange += UpdateStats;
        skillManager.learnEvent.OnChange += UpdateStats;

        actorSubstage.OnChange += () =>
        {
            foreach (Quest quest in quests.Values)
            {
                quest.Update(this);
            }
        };
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        actorStage.OnChange -= UpdateStats;
        actorSubstage.OnChange -= UpdateStats;
        skillManager.learnEvent.OnChange -= UpdateStats;
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

        if (actorHP.Value < actorHP.Maximum && !actorHPIsRegening)
        {
            StartCoroutine(Regenerate());
        }

        // Buff/regen system placeholder
        if (actorStr.Value != actorStr.Maximum)
        {
            actorStr.Value = actorStr.Maximum;
        }

        if (actorDex.Value != actorDex.Maximum)
        {
            actorDex.Value = actorDex.Maximum;
        }

        if (actorInt.Value != actorInt.Maximum)
        {
            actorInt.Value = actorInt.Maximum;
        }

        if (actorLuck.Value != actorLuck.Maximum)
        {
            actorLuck.Value = actorLuck.Maximum;
        }

        if (actorAttack.Value != actorAttack.Maximum)
        {
            actorAttack.Value = actorAttack.Maximum;
        }

        if (actorDefense.Value != actorDefense.Maximum)
        {
            actorDefense.Value = actorDefense.Maximum;
        }
    }

    private IEnumerator Regenerate()
    {
        actorHPIsRegening = true;

        while (actorHP.Value < actorHP.Maximum)
        {
            yield return new WaitForSeconds(actorHPRegenInterval);
            actorHP.Value = Math.Min(
                actorHP.Maximum,
                actorHP.Value + actorHPRegenPerInterval
            );
        }

        actorHPIsRegening = false;
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
        if (skillManager.IsLearned(skill) && skill.CanRankUp())
        {
            skill.RankUp();

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

            AudioController.Instance.PlayLevelUpSFX();
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

    public void UpdateStats()
    {
        Dictionary<string, float> cultivationStat = SumCultivationStats();
        Dictionary<string, float> skillStat = SumSkillStats();

        foreach ((string statName, StatManager stat) in primaryStats)
        {
            stat.Maximum = 0;
        }

        foreach ((string statName, StatManager stat) in secondaryStats)
        {
            stat.Maximum = 0;
        }

        foreach ((string statName, float value) in cultivationStat)
        {
            primaryStats[statName].Maximum += value;
        }

        foreach ((string statName, float value) in skillStat)
        {
            primaryStats[statName].Maximum += value;
        }

        foreach ((string statName, float value) in WindowCharacterEquipmentSlot.statAccumulator)
        {
            if (primaryStats.ContainsKey(statName))
            {
                primaryStats[statName].Maximum += value;
            }
            else if (secondaryStats.ContainsKey(statName))
            {
                secondaryStats[statName].Maximum += value;
            }
        }
    }

    private Dictionary<string, float> SumCultivationStats()
    {
        CultivationStageModel stage = CultivationStageModel.stages
            [((int)actorStage.Value, (int)actorSubstage.Value)];

        return new Dictionary<string, float>
        {
            {"HP", stage.hp },
            {"MP", stage.mp },
            {"STR", stage.strength },
            {"INT", stage.intelligence },
            {"DEX", stage.dexterity },
            {"Luck", stage.luck }
        };
    }

    private Dictionary<string, float> SumSkillStats()
    {
        Dictionary<string, float> statAccumulator = new Dictionary<string, float>
        {
            {"HP", 0f },
            {"MP", 0f },
            {"STR", 0f },
            {"INT", 0f },
            {"DEX", 0f },
            {"Luck", 0f }
        };

        foreach (Skill skill in skillManager.Skills.Values)
        {
            foreach ((int statID, SkillStatModel stat) in skill.model.stats)
            {
                string statName = SkillStatTypeModel.FindByID(statID);

                if (statAccumulator.ContainsKey(statName))
                {
                    statAccumulator[statName] += stat.values[skill.index.Value];
                }
            }
        }

        return statAccumulator;
    }

    public void Init()
    {
        // Debug purposes...
        actorName.Value = "Test";

        DataTable dt = GameManager.Instance.Database.Read(@"SELECT id FROM skill
            WHERE is_starting_with = 1;");

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            ActionSkillController action = new ActionSkillController(this, this, ID);
            action.Handle();
        }

        dt = GameManager.Instance.Database.Read(@"SELECT id FROM quest;");

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            quests.Add(ID, new Quest(ID));
        }

        inventoryManager.AddBag(1);
    }
}