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

    // Ability points
    public IntManager actorAP = new IntManager(1000);
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
    protected override void Start()
    {
        base.Start();

        // Debug purposes...
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
            AttemptMove(horizontal, vertical);
        }
    }

    /// <summary>
    ///     Moves the player up.
    /// </summary>
    public void MoveUp()
    {
        AttemptMove(0, 1);
    }

    /// <summary>
    ///     Moves the player left.
    /// </summary>
    public void MoveLeft()
    {
        AttemptMove(-1, 0);
    }

    /// <summary>
    ///     Moves the player down.
    /// </summary>
    public void MoveDown()
    {
        AttemptMove(0, -1);
    }

    /// <summary>
    ///     Moves the player right.
    /// </summary>
    public void MoveRight()
    {
        AttemptMove(1, 0);
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
        int apCost = (int)skill.stats["ap_cost"][skill.index.Value + 1] - (int)skill.stats["ap_cost"][skill.index.Value];

        if (skillManager.IsLearned(skill) && skill.CanRankUp() && actorAP.Value >= apCost)
        {
            actorAP.Value -= apCost;
            skill.RankUp();

            foreach(KeyValuePair<string, StatManager> stat in primaryStats)
            {
                if (skill.stats.ContainsKey(stat.Key))
                {
                    int currRank = (int)skill.stats[stat.Key][skill.index.Value];
                    int prevRank = (int)skill.stats[stat.Key][skill.index.Value - 1];
                    int statDiff = Math.Max(0, currRank - prevRank);
                    stat.Value.Value += statDiff;
                }
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
        float roll = (float)GameManager.Instance.rnd.NextDouble();

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