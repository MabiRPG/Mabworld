using UnityEngine;

public class WindowCrafting : Window
{
    public static WindowCrafting Instance { get; private set; }

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

    private void Start()
    {
        gameObject.SetActive(false);
    }
}