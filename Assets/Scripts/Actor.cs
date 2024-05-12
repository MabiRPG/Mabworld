using System.Collections.Generic;

public class Actor : Movement
{
    public class Stat : StatManager
    {
        public Stat (int value)
        {
            _value = value;
            _maximum = value;
            _baseMaximum = value;
        }
    }

    public string actorName;
    public int actorLevel = 1;

    public Stat actorHP = new Stat(100);
    public Stat actorMP = new Stat(100);

    public Stat actorStr = new Stat(10);
    public Stat actorInt = new Stat(10);
    public Stat actorDex = new Stat(10);
    public Stat actorLuck = new Stat(10);

    public Dictionary<string, Stat> primaryStats = new Dictionary<string, Stat>();
    public Dictionary<string, Stat> secondaryStats = new Dictionary<string, Stat>();

    public Stat actorDefense = new Stat(0);
    public Stat actorProt = new Stat(0);
    public Stat actorMDefense = new Stat(0);
    public Stat actorMProt = new Stat(0);

    //public ValueManager actorNameEvent = new ValueManager();
    //public ValueManager actorLevelEvent = new ValueManager();

    public Dictionary<int, Skill> skills = new Dictionary<int, Skill>();

    protected virtual void Awake()
    {
        primaryStats.Add("hp", actorHP);
        primaryStats.Add("mp", actorMP);
        primaryStats.Add("str", actorStr);
        primaryStats.Add("int", actorInt);
        primaryStats.Add("dex", actorDex);
        primaryStats.Add("luck", actorLuck);

        secondaryStats.Add("defense", actorDefense);
        secondaryStats.Add("protection", actorProt);
        secondaryStats.Add("m_defense", actorMDefense);
        secondaryStats.Add("m_protection", actorMProt);
    }
}
