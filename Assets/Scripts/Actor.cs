using System.Collections.Generic;

/// <summary>
///     Handles all actor-level info and calculations.
/// </summary>
public class Actor : Movement
{
    // Name and level of actor
    public string actorName;
    public int actorLevel = 1;

    // Health and mana
    public StatManager actorHP = new StatManager(100, 100, 100);
    public StatManager actorMP = new StatManager(100, 100, 100);

    // Strength, Intelligence, Dexterity, and Luck
    public StatManager actorStr = new StatManager(10, 10, 10);
    public StatManager actorInt = new StatManager(10, 10, 10);
    public StatManager actorDex = new StatManager(10, 10, 10);
    public StatManager actorLuck = new StatManager(10, 10, 10);

    // Defense, Protection, Magic Defense, and Magic Protection
    public StatManager actorDefense = new StatManager();
    public StatManager actorProt = new StatManager();
    public StatManager actorMDefense = new StatManager();
    public StatManager actorMProt = new StatManager();

    // Dict of primary and secondary (calculated) stats for easier reference
    public Dictionary<string, StatManager> primaryStats = new Dictionary<string, StatManager>();
    public Dictionary<string, StatManager> secondaryStats = new Dictionary<string, StatManager>();

    //public ValueManager actorNameEvent = new ValueManager();
    //public ValueManager actorLevelEvent = new ValueManager();

    public Dictionary<int, Skill> skills = new Dictionary<int, Skill>();

    // How much defense scales with strength
    protected int defenseStrFactor = 10;
    // Magic defense scale with str
    protected int mDefenseStrFactor = 10;
    // Magic Protection scale with int
    protected int mProtIntFactor = 20; 

    /// <summary>
    ///     Initializes the object.
    /// </summary>
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

    /// <summary>
    ///     Called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        actorStr.OnBaseMaximumValueChange += CalculateDefense;
        actorStr.OnBaseMaximumValueChange += CalculateMDefense;
        actorInt.OnBaseMaximumValueChange += CalculateMProt;
    }

    /// <summary>
    ///     Called when the object becomes disabled and inactive.
    /// </summary>
    private void OnDisable()
    {
        actorStr.OnBaseMaximumValueChange -= CalculateDefense;
        actorStr.OnBaseMaximumValueChange -= CalculateMDefense;
        actorInt.OnBaseMaximumValueChange -= CalculateMProt;
    }

    /// <summary>
    ///     Calculates the defense
    /// </summary>
    private void CalculateDefense()
    {
        actorDefense.BaseMaximum = actorStr.BaseMaximum / defenseStrFactor;
    }

    /// <summary>
    ///     Calculates the magic defense
    /// </summary>
    private void CalculateMDefense()
    {
        actorMDefense.BaseMaximum = actorStr.BaseMaximum / mDefenseStrFactor; 
    }

    /// <summary>
    ///     Calculates the magic protection
    /// </summary>
    private void CalculateMProt()
    {
        actorMProt.BaseMaximum = actorInt.BaseMaximum / mProtIntFactor;
    }
}
