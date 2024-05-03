using UnityEngine;
using System;

//==============================================================================
// ** Player
//------------------------------------------------------------------------------
//  This class handles all player & input processing. Refer to Player.instance
//  for the global instance.
//==============================================================================

public class Player : Actor
{
    // Global instance of player
    public static Player instance = null;

    public int actorAP = 1000;
    public ValueEventManager actorAPEvent = new ValueEventManager();

    // How much our life skill success rates scale with dex.
    public int lifeSkillDexFactor = 10;
    // What the maximize success rate increase is.
    public int lifeSkillSuccessCap = 18;

    // How much our lucky gathers scale with luck stat
    public int luckyFactor = 20;
    // How much resource multiplier is applied on trigger lucky
    public int luckyGain = 2;
    public int hugeLuckyFactor = 50000;
    public int hugeLuckyGain = 20;

    public GameObject Map;

    //--------------------------------------------------------------------------
    // * Initializes the object
    //--------------------------------------------------------------------------
    private void Awake()
    {
        // Singleton recipe
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    //--------------------------------------------------------------------------
    // * Called before any frame updates.
    //--------------------------------------------------------------------------
    protected override void Start()
    {
        base.Start();

        // Debug purposes...
        LearnSkill(1);
        LearnSkill(2);
        LearnSkill(3);
        RankUpSkill(1);
        skills[1].AddXP(50);
        skills[2].AddXP(100);
        skills[3].AddXP(150);
    }

    //--------------------------------------------------------------------------
    // * Called every frame
    //--------------------------------------------------------------------------
    private void Update()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");
        bool callSkillWindow = Input.GetButtonDown("Skill Window");
        bool callMap = Input.GetButtonDown("Map");

        if (moveHorizontal != 0 || moveVertical != 0) 
        {
            AttemptMove(moveHorizontal, moveVertical);
        }

        if (callSkillWindow)
        {
            WindowSkill.instance.ToggleVisible();
        }
        if (callMap)
        {
            Map.SetActive(!Map.activeSelf);
        }
    }

    //--------------------------------------------------------------------------
    // * Checks if the skill has been learned
    //      int id : id of the skill
    //--------------------------------------------------------------------------
    public bool IsSkillLearned(int id)
    {
        return skills.ContainsKey(id);
    }

    public bool IsSkillLearned(Skill skill)
    {
        return skills.ContainsValue(skill);
    }

    //--------------------------------------------------------------------------
    // * Learns the skill
    //      int id : id of the skill
    //--------------------------------------------------------------------------
    public void LearnSkill(int id)
    {
        if (IsSkillLearned(id))
        {
            return;
        }

        skills.Add(id, new Skill(id));
    }

    //--------------------------------------------------------------------------
    // * Ranks up the skill
    //      int id : id of the skill
    //--------------------------------------------------------------------------
    public void RankUpSkill(int id)
    {
        int apCost = (int)skills[id].stats["ap_cost"][skills[id].index + 1];

        if (IsSkillLearned(id) && skills[id].CanRankUp() && actorAP >= apCost)
        {
            actorAP -= apCost;
            skills[id].RankUp();
        }
    }

    public void RankUpSkill(Skill skill)
    {
        int apCost = (int)skill.stats["ap_cost"][skill.index + 1];

        if (IsSkillLearned(skill) && skill.CanRankUp() && actorAP >= apCost)
        {
            actorAP -= apCost;
            skill.RankUp();
        }        
    }

    //--------------------------------------------------------------------------
    // * Returns the player's life skill success rate bonus
    //--------------------------------------------------------------------------
    public float LifeSkillSuccessRate()
    {
        return Math.Min(actorDex.current / lifeSkillDexFactor, lifeSkillSuccessCap);
    }

    //--------------------------------------------------------------------------
    // * Returns the lucky resource gain multiplier
    //--------------------------------------------------------------------------
    public int IsLucky() 
    {
        float lucky = actorLuck.current / luckyFactor;
        float hugeLucky = actorLuck.current / hugeLuckyFactor;
        float roll = (float)rnd.NextDouble();

        if (roll <= hugeLucky) 
        {
            return hugeLuckyFactor;
        }
        else if (roll <= lucky) 
        {
            return luckyFactor;
        }

        return 1;
    }
}