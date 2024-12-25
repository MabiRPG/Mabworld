using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public Canvas worldCanvas;
    public GraphicRaycaster raycaster;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        raycaster = worldCanvas.GetComponent<GraphicRaycaster>();
    }
}