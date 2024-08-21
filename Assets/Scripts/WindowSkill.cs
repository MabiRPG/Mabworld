using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
///     This class handles the skill window processing.
/// </summary>
public class WindowSkill : Window
{
    // Global reference.
    public static WindowSkill Instance = null;
    
    // Prefab for extra skill detail window.
    [SerializeField]
    private GameObject windowSkillDetailedPrefab;
    // Prefab for skill advancement.
    [SerializeField]
    private GameObject windowSkillAdvancePrefab;
    
    // Prefab manager instances.
    public PrefabFactory detailedPrefabFactory;
    public PrefabFactory advancePrefabFactory;

    public IntManager categoryIndex = new IntManager();

    /// <summary>
    ///     Initializes the object.
    /// </summary>
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

        detailedPrefabFactory = ScriptableObject.CreateInstance<PrefabFactory>();
        detailedPrefabFactory.SetPrefab(windowSkillDetailedPrefab);
        advancePrefabFactory = ScriptableObject.CreateInstance<PrefabFactory>();
        advancePrefabFactory.SetPrefab(windowSkillAdvancePrefab);
    }

    /// <summary>
    ///     Called after all Awakes.
    /// </summary>
    private void Start()
    {
        //Draw();
        categoryIndex.Value = 1;
        // Hides the object at start
        // gameObject.SetActive(false);
    }

    public void CreateDetailedWindow(Skill skill)
    {
        GameObject obj = detailedPrefabFactory.GetFree(skill, GameManager.Instance.canvas.transform);
        WindowSkillDetailed window = obj.GetComponent<WindowSkillDetailed>();
        window.SetSkill(skill, () => { });
        WindowManager.Instance.AddWindow(window);
        WindowManager.Instance.ToggleWindow(window);        
    }

    public void CreateAdvanceWindow(Skill skill)
    {
        GameObject obj = advancePrefabFactory.GetFree(skill, GameManager.Instance.canvas.transform);
        WindowSkillAdvance window = obj.GetComponent<WindowSkillAdvance>();
        window.SetSkill(skill);
        WindowManager.Instance.AddWindow(window);
        WindowManager.Instance.ToggleWindow(window);        
    }
}
