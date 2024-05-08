using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
///     This class handles the detailed skill window processing.
/// </summary>
public class WindowSkillDetailed : Window
{
    // Prefab for every skill stat rendering
    [SerializeField]
    private GameObject statPrefab;
    // Prefab for every training method rendering.
    [SerializeField]
    private GameObject trainingMethodPrefab;

    // Skill of window
    private Skill skill;
    // Prefab manager instances
    private PrefabManager statPrefabs;
    private PrefabManager trainingMethodPrefabs;

    // List of prefab object references.
    private TMP_Text skillName;
    private Image icon;
    private Transform statTransform;
    private Transform trainingMethodsTransform;
    private GameObject xpBar;
    private ProgressBar xpBarScript;
    private GameObject overXpBar;
    private ProgressBar overXpBarScript;
    private Button advanceButton;
    private Button closeButton;

    // Event handlers
    public EventManager advanceButtonEvent = new EventManager();

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        skillName = body.transform.Find("Name").GetComponent<TMP_Text>();
        icon = body.transform.Find("Icon Parent").GetComponentInChildren<Image>();
        statTransform = body.transform.Find("Stats");
        trainingMethodsTransform = body.transform.Find("Training Methods");
        xpBar = body.transform.Find("Bars Parent").Find("Bars").Find("XP Bar").gameObject;
        xpBarScript = xpBar.GetComponent<ProgressBar>();
        overXpBar = body.transform.Find("Bars Parent").Find("Bars").Find("Overfill XP Bar").gameObject;
        overXpBarScript = overXpBar.GetComponent<ProgressBar>();
        advanceButton = body.transform.Find("Advance Parent").Find("Advance Button").GetComponent<Button>();
        closeButton = body.transform.Find("Close Button").GetComponent<Button>();

        statPrefabs = ScriptableObject.CreateInstance<PrefabManager>();
        statPrefabs.SetPrefab(statPrefab);
        trainingMethodPrefabs = ScriptableObject.CreateInstance<PrefabManager>();
        trainingMethodPrefabs.SetPrefab(trainingMethodPrefab);
    }

    /// <summary>
    ///     Called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        advanceButton.onClick.AddListener(delegate {advanceButtonEvent.RaiseOnChange();});
        closeButton.onClick.AddListener(CloseWindow);
    }

    /// <summary>
    ///     Called when the object becomes disabled and inactive.
    /// </summary>
    private void OnDisable()
    {
        Clear();

        advanceButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveListener(CloseWindow);
    }

    /// <summary>
    ///     Sets the skill instance.
    /// </summary>
    /// <param name="newSkill">Skill instance for window.</param>
    public void SetSkill(Skill newSkill, Action advanceButtonAction)
    {
        Clear();
        skill = newSkill;

        // Finds the skill icon sprite and reassigns it.
        string dir = "Sprites/Skill Icons/" + skill.info["icon_name"].ToString();
        icon.sprite = Resources.Load<Sprite>(dir);
        UpdateRank();
        UpdateXp();

        skill.indexEvent.OnChange += UpdateRank;
        skill.xpEvent.OnChange += UpdateXp;
        skill.xpMaxEvent.OnChange += UpdateXp;
        advanceButtonEvent.OnChange += advanceButtonAction;

        ShowWindow();
    }

    /// <summary>
    ///     Clears the window.
    /// </summary>
    private void Clear()
    {
        statPrefabs.SetActiveAll(false);
        trainingMethodPrefabs.SetActiveAll(false);

        if (skill != null)
        {
            skill.indexEvent.OnChange -= UpdateRank;
            skill.xpEvent.OnChange -= UpdateXp;
            skill.xpMaxEvent.OnChange -= UpdateXp;
            skill = null;
        }
    }

    /// <summary>
    ///     Updates the rank.
    /// </summary>
    private void UpdateRank()
    {
        // Finds the skill name and reassigns it.
        skillName.text = "Rank " + skill.rank + " " + skill.info["name"];

        int index = skill.index;

        // For every stat, create a new stat field prefab and populate it.
        foreach (KeyValuePair<string, float[]> stat in skill.stats)
        {
            if (stat.Key == "ap_cost" || stat.Value[index] == 0)
            {
                continue;
            }

            GameObject obj = statPrefabs.GetFree(stat.Key, statTransform);
            WindowSkillStat script = obj.GetComponent<WindowSkillStat>();
            script.SetText(stat.Key, stat.Value[index]);
        }

        // For every training method, create a new training method prefab and populate it.
        foreach (SkillTrainingMethod method in skill.methods)
        {
            GameObject obj = trainingMethodPrefabs.GetFree(method.method["name"], trainingMethodsTransform);
            WindowSkillTrainingMethod script = obj.GetComponent<WindowSkillTrainingMethod>();
            script.SetText(method);
        }
    }
    
    /// <summary>
    ///     Updates the xp bar progress.
    /// </summary>
    private void UpdateXp()
    {
        advanceButton.gameObject.transform.parent.gameObject.SetActive(false);

        // If < 100, use normal bar, else use overfill bar.
        if (skill.xp <= 100) 
        {
            xpBar.SetActive(true);
            xpBarScript.SetCurrent(skill.xp);
            xpBarScript.SetMaximum(100);
            overXpBar.SetActive(false);

            if (skill.xp == 100)
            {
                advanceButton.gameObject.transform.parent.gameObject.SetActive(true);
            }
        }
        else 
        {
            xpBar.SetActive(false);
            overXpBarScript.SetCurrent(skill.xp);
            overXpBarScript.SetMaximum(skill.xpMax);
            overXpBar.SetActive(true);
            advanceButton.gameObject.transform.parent.gameObject.SetActive(true);
        }       
    }
}
