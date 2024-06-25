using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     This class handles the skill window processing.
/// </summary>
public class WindowSkill : Window
{
    // Global reference.
    public static WindowSkill Instance = null;
    
    // Prefab for every skill row in window.
    [SerializeField]
    private GameObject skillPrefab;
    // Prefab for extra skill detail window.
    [SerializeField]
    private GameObject windowSkillDetailedPrefab;
    // Prefab for skill advancement.
    [SerializeField]
    private GameObject windowSkillAdvancePrefab;
    
    // Prefab manager instances.
    private PrefabFactory skillPrefabs;
    private PrefabFactory detailedPrefabs;
    private PrefabFactory advancePrefabs;

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

        // Hides the object at start
        gameObject.SetActive(false);

        skillPrefabs = ScriptableObject.CreateInstance<PrefabFactory>();
        skillPrefabs.SetPrefab(skillPrefab);
        detailedPrefabs = ScriptableObject.CreateInstance<PrefabFactory>();
        detailedPrefabs.SetPrefab(windowSkillDetailedPrefab);
        advancePrefabs = ScriptableObject.CreateInstance<PrefabFactory>();
        advancePrefabs.SetPrefab(windowSkillAdvancePrefab);
    }

    /// <summary>
    ///     Called after all Awakes.
    /// </summary>
    private void Start()
    {
        Draw();
    }

    /// <summary>
    ///     Called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        skillPrefabs.SetActiveAll(false);
        Draw();
    }

    /// <summary>
    ///     Draws the window.
    /// </summary>
    private void Draw()
    {
        // For every skill, create a new prefab to display skill info in window.
        foreach (KeyValuePair<int, Skill> skill in Player.Instance.skills)
        {
            // Instantiates the prefab in the window. Parent window has
            // a vertical layout group to control children components.
            GameObject obj = skillPrefabs.GetFree(skill.Key, body.transform);
            // Gets the script, sets the skill and button call functions.
            WindowSkillRow row = obj.GetComponent<WindowSkillRow>();
            row.SetSkill(
                skill.Value, 
                () => CreateDetailedSkillWindow(skill.Value), 
                () => CreateAdvanceSkillWindow(skill.Value)
            );
        }
        
        // Resets the content size fitter.
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)gameObject.transform);
    }

    /// <summary>
    ///     Creates a new advance skill window.
    /// </summary>
    /// <param name="skill"></param>
    private void CreateAdvanceSkillWindow(Skill skill)
    {
        GameObject obj = advancePrefabs.GetFree(skill, transform.parent);
        WindowSkillAdvance window = obj.GetComponent<WindowSkillAdvance>();
        window.SetSkill(skill);
        window.Focus();
    }

    /// <summary>
    ///     Creates a new detailed skill window.
    /// </summary>
    /// <param name="skill">Skill to populate window.</param>
    private void CreateDetailedSkillWindow(Skill skill)
    {
        GameObject obj = detailedPrefabs.GetFree(skill, transform.parent);
        WindowSkillDetailed window = obj.GetComponent<WindowSkillDetailed>();
        window.SetSkill(skill, () => CreateAdvanceSkillWindow(skill));
        window.Focus();
    }
}
