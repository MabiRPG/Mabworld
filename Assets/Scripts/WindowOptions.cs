using UnityEngine;

public class WindowOptions : Window
{
    public static WindowOptions Instance = null;

    protected override void Awake()
    {
        base.Awake();

        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}