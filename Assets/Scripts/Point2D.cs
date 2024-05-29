public class Point2D
{
    private float x;
    private float y;
    private (float, float) point;

    public float X
    {
        get {return x;}
        set {
            x = value;
            point.Item1 = value;
        }
    }

    public float Y
    {
        get {return y;}
        set {
            y = value;
            point.Item2 = value;
        }
    }

    public (float, float) Point
    {
        get {return point;}
        set {
            point = value;
            x = value.Item1;
            y = value.Item2;
        }
    }
}