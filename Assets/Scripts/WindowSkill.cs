using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
///     This class handles the skill window processing.
/// </summary>
public class WindowSkill : Window
{
    // Global reference.
    public static WindowSkill instance = null;
    
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
    private PrefabManager skillPrefabs;
    private PrefabManager detailedPrefabs;
    private PrefabManager advancePrefabs;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    protected override void Awake() 
    {
        base.Awake();

        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Hides the object at start
        gameObject.SetActive(false);

        skillPrefabs = ScriptableObject.CreateInstance<PrefabManager>();
        skillPrefabs.SetPrefab(skillPrefab);
        detailedPrefabs = ScriptableObject.CreateInstance<PrefabManager>();
        detailedPrefabs.SetPrefab(windowSkillDetailedPrefab);
        advancePrefabs = ScriptableObject.CreateInstance<PrefabManager>();
        advancePrefabs.SetPrefab(windowSkillAdvancePrefab);
    }

    /// <summary>
    ///     Draws the window.
    /// </summary>
    private void Draw()
    {
        // For every skill, create a new prefab to display skill info in window.
        foreach (KeyValuePair<int, Skill> skill in Player.instance.skills)
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
    }

    /// <summary>
    ///     Toggles visibility of the object.
    /// </summary>
    public void ToggleVisible()
    {
        // Changes visibilty of object.
        gameObject.SetActive(!gameObject.activeSelf);

        if (gameObject.activeSelf) 
        {
            Clear();
            Draw();
        }
    }

    private void CreateAdvanceSkillWindow(Skill skill)
    {
        GameObject obj = advancePrefabs.GetFree(skill, transform.parent);
        WindowSkillAdvance window = obj.GetComponent<WindowSkillAdvance>();
        window.SetSkill(skill);
    }

    /// <summary>
    ///     Creates a new detailed skill window for the skill.
    /// </summary>
    /// <param name="skill">Skill to populate window.</param>
    private void CreateDetailedSkillWindow(Skill skill)
    {
        GameObject obj = detailedPrefabs.GetFree(skill, transform.parent);
        WindowSkillDetailed window = obj.GetComponent<WindowSkillDetailed>();
        window.SetSkill(skill, () => CreateAdvanceSkillWindow(skill));
    }

    private void Clear()
    {
        skillPrefabs.SetActiveAll(false);
    }
}
