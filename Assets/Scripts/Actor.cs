public class Actor : Movement
{
    public class Stat
    {
        public int current;
        public int maximum;
        public int baseMaximum;

        public Stat (int value)
        {
            current = value;
            maximum = value;
            baseMaximum = value;
        }
    }

    public string actorName;
    public int actorLevel = 1;

    public Stat actorHP = new(100);
    public Stat actorMP = new(100);

    public Stat actorStr = new(10);
    public Stat actorInt = new(10);
    public Stat actorDex = new(10);
    public Stat actorLuck = new(10);

    public Stat actorDefense = new(0);
    public Stat actorProt = new(0);
    public Stat actorMDefense = new(0);
    public Stat actorMProt = new(0);
}
