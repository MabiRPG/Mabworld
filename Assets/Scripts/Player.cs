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
    }

    // Update is called once per frame
    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        AttemptMove(horizontal, vertical);
    }

    public float LifeSkillSuccessRate()
    {
        return Math.Min(actorDex.current / lifeSkillDexFactor, lifeSkillSuccessCap);
    }

    public int IsLucky() 
    {
        float lucky = (float)actorLuck.current / (float)luckyFactor;
        float hugeLucky = (float)actorLuck.current / (float)hugeLuckyFactor;
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