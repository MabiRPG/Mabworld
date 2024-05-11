using UnityEngine;
using System;
using System.Collections;

/// <summary>
///     Handles all player & input processing.
/// </summary>
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

    // Flag for player if busy
    public bool isBusy = false;
    private IEnumerator playerCoroutine;

    // Result instance of player
    public Result result = new Result();

    public GameObject Map;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
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

    /// <summary>
    ///     Called after all Awakes.
    /// </summary>
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

    /// <summary>
    ///     Triggered whenever colliding with another collider 2D.
    /// </summary>
    /// <param name="other"></param>
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
    // private void Update()
    // {
    //     float moveHorizontal = Input.GetAxisRaw("Horizontal");
    //     float moveVertical = Input.GetAxisRaw("Vertical");
    //     bool callSkillWindow = Input.GetButtonDown("Skill Window");
    //     bool callCharacterWindow = Input.GetButtonDown("Character Window");
    //     bool callMap = Input.GetButtonDown("Map");

    //     if (moveHorizontal != 0 || moveVertical != 0) 
    //     {
    //         AttemptMove(moveHorizontal, moveVertical);
    //     }

    //     if (callSkillWindow)
    //     {
    //         WindowSkill.Instance.ToggleVisible();
    //     }

    //     if (callCharacterWindow)
    //     {
    //         WindowCharacter.Instance.ToggleVisible();
    //     }

    //     if (callMap)
    //     {
    //         Map.SetActive(!Map.activeSelf);
    //     }

    //     // if (Input.GetKeyDown(KeyCode.F))
    //     // {
    //     //     actorMP.current--;
    //     //     actorMPEvent.RaiseOnChange();
    //     // }
    // }

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
        Map.SetActive(!Map.activeSelf);
    }

    /// <summary>
    ///     Checks if the skill has been learned
    /// </summary>
    /// <param name="ID">Skill ID in database</param>
    /// <returns>True if learned, False otherwise</returns>
    public bool IsSkillLearned(int ID)
    {
        return skills.ContainsKey(ID);
    }

    /// <summary>
    ///     Checks if the skill has been learned
    /// </summary>
    /// <param name="skill">Skill instance to check</param>
    /// <returns>True if learned, False otherwise</returns>
    public bool IsSkillLearned(Skill skill)
    {
        return skills.ContainsValue(skill);
    }

    /// <summary>
    ///     Learns the skill
    /// </summary>
    /// <param name="ID">Skill ID in database</param>
    public void LearnSkill(int ID)
    {
        if (IsSkillLearned(ID))
        {
            return;
        }

        skills.Add(ID, new Skill(ID));
    }

    /// <summary>
    ///     Ranks up the skill
    /// </summary>
    /// <param name="ID">Skill ID in database</param>
    public void RankUpSkill(int ID)
    {
        int apCost = (int)skills[ID].stats["ap_cost"][skills[ID].index + 1];

        if (IsSkillLearned(ID) && skills[ID].CanRankUp() && actorAP >= apCost)
        {
            actorAP -= apCost;
            skills[ID].RankUp();
        }
    }

    /// <summary>
    ///     Ranks up the skill
    /// </summary>
    /// <param name="skill">Skill instance</param>
    public void RankUpSkill(Skill skill)
    {
        int apCost = (int)skill.stats["ap_cost"][skill.index + 1];

        if (IsSkillLearned(skill) && skill.CanRankUp() && actorAP >= apCost)
        {
            actorAP -= apCost;
            skill.RankUp();
        }        
    }

    /// <summary>
    ///     Uses the skill
    /// </summary>
    /// <param name="ID"></param>
    public void UseSkill(int ID)
    {
        if (IsSkillLearned(ID) && !isBusy)
        {
            playerCoroutine = skills[ID].Use();
            StartCoroutine(skills[ID].Use());
        }
    }

    /// <summary>
    ///     Calculates the player's life skill success rate bonus
    /// </summary>
    /// <returns>Bonus rate as percentage</returns>
    public float LifeSkillSuccessRate()
    {
        return Math.Min(actorDex.current / lifeSkillDexFactor, lifeSkillSuccessCap);
    }

    /// <summary>
    ///     Calculates the lucky resource gain factor
    /// </summary>
    /// <returns>Resource gain multiplier</returns>
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

    public void StartAction(Skill skill)
    {
        if (IsSkillLearned(skill) && !isBusy)
        {
            playerCoroutine = skill.Use();
            StartCoroutine(playerCoroutine);
        }
    }

    public void InterruptAction()
    {
        if (playerCoroutine != null)
        {
            StopCoroutine(playerCoroutine);
            playerCoroutine = null;
            isBusy = false;
        }
    }
}