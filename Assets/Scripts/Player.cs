using UnityEngine;
using System;
using UnityEngine.UIElements;

//==============================================================================
// ** Player
//------------------------------------------------------------------------------
//  This class handles all player & input processing. Refer to Player.instance
//  for the global instance.
//==============================================================================

public class Player : Actor
{
    // Global instance of player
    public static Player Instance = null;

    public int actorAP = 1000;
    public EventManager actorAPEvent = new EventManager();

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

    public GameObject Map;

    //--------------------------------------------------------------------------
    // * Initializes the object
    //--------------------------------------------------------------------------
    private void Awake()
    {
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        InteractableObject target = other.gameObject.GetComponent<InteractableObject>();

        if (target != null)
        {
            int ID = int.Parse(target.info["skill_id"].ToString());
            UseSkill(ID);
        }
    }

    //--------------------------------------------------------------------------
    // * Called every frame
    //--------------------------------------------------------------------------
    private void Update()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");
        bool callSkillWindow = Input.GetButtonDown("Skill Window");
        bool callCharacterWindow = Input.GetButtonDown("Character Window");
        bool callMap = Input.GetButtonDown("Map");

        if (moveHorizontal != 0 || moveVertical != 0) 
        {
            AttemptMove(moveHorizontal, moveVertical);
        }

        if (callSkillWindow)
        {
            WindowSkill.Instance.ToggleVisible();
        }

        if (callCharacterWindow)
        {
            WindowCharacter.Instance.ToggleVisible();
        }

        if (callMap)
        {
            Map.SetActive(!Map.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            actorMP.current--;
            actorMPEvent.RaiseOnChange();
        }
    }

    public void MoveUp()
    {
        AttemptMove(0, 1);
    }

    public void MoveLeft()
    {
        AttemptMove(-1, 0);
    }

    public void MoveDown()
    {
        AttemptMove(0, -1);
    }

    public void MoveRight()
    {
        AttemptMove(1, 0);
    }

    //--------------------------------------------------------------------------
    // * Checks if the skill has been learned
    //      int id : id of the skill
    //--------------------------------------------------------------------------
    public bool IsSkillLearned(int ID)
    {
        return skills.ContainsKey(ID);
    }

    public bool IsSkillLearned(Skill skill)
    {
        return skills.ContainsValue(skill);
    }

    //--------------------------------------------------------------------------
    // * Learns the skill
    //      int id : id of the skill
    //--------------------------------------------------------------------------
    public void LearnSkill(int ID)
    {
        if (IsSkillLearned(ID))
        {
            return;
        }

        skills.Add(ID, new Skill(ID));
    }

    //--------------------------------------------------------------------------
    // * Ranks up the skill
    //      int id : id of the skill
    //--------------------------------------------------------------------------
    public void RankUpSkill(int ID)
    {
        int apCost = (int)skills[ID].stats["ap_cost"][skills[ID].index + 1];

        if (IsSkillLearned(ID) && skills[ID].CanRankUp() && actorAP >= apCost)
        {
            actorAP -= apCost;
            skills[ID].RankUp();
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

    public void UseSkill(int ID)
    {
        if (IsSkillLearned(ID))
        {
            skills[ID].Use();
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
        float roll = (float)GameManager.Instance.rnd.NextDouble();

        if (roll <= hugeLucky) 
        {
            return hugeLuckyGain;
        }
        else if (roll <= lucky) 
        {
            return luckyGain;
        }

        return 1;
    }
}