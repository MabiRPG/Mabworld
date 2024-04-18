using UnityEngine;

public class GameManager : MonoBehaviour 
{
    public static GameManager instance = null;

    public string databaseName;

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
}