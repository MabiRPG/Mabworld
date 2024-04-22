using UnityEngine;
using System;

public class Player : Actor
{
    public static Player instance = null;

    public int lifeSkillDexFactor = 10;
    public int lifeSkillSuccessCap = 18;

    public int luckyFactor = 20;
    public int luckyGain = 2;
    public int hugeLuckyFactor = 50000;
    public int hugeLuckyGain = 20;

    private void Awake()
    {
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

    protected override void Start()
    {
        base.Start();

        LearnSkill(1);
        LearnSkill(2);
        LearnSkill(3);
        RankUpSkill(1);
        skills[1].AddXP(50);
        skills[2].AddXP(100);
        skills[3].AddXP(150);
    }

    // Update is called once per frame
    private void Update()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");
        bool callSkillWindow = Input.GetButtonDown("Skill Window");

        if (moveHorizontal != 0 || moveVertical != 0) 
        {
            AttemptMove(moveHorizontal, moveVertical);
        }

        if (callSkillWindow)
        {
            Window_Skill.instance.ToggleVisible();
        }
    }

    public bool IsSkillLearned(int id)
    {
        return skills.ContainsKey(id);
    }

    public void LearnSkill(int id)
    {
        if (IsSkillLearned(id))
        {
            return;
        }

        skills.Add(id, new Skill(id));
    }

    public void RankUpSkill(int id)
    {
        if (!IsSkillLearned(id)) {
            return;
        }

        skills[id].RankUp();
    }

    public float LifeSkillSuccessRate()
    {
        return Math.Min(actorDex.current / lifeSkillDexFactor, lifeSkillSuccessCap);
    }

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