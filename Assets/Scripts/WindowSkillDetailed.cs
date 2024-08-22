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
    private PrefabFactory statPrefabs;
    private PrefabFactory trainingMethodPrefabs;

    // List of prefab object references.
    private TMP_Text skillName;
    private Image icon;
    private Transform statTransform;
    private Transform trainingMethodsTransform;
    private GameObject xpBar;
    private UI_ProgressBar xpBarScript;
    private GameObject overXpBar;
    private UI_ProgressBar overXpBarScript;
    private Button advanceButton;
    private Button closeButtonBody;

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
        xpBar = body.transform.Find("Bars Parent/Bars/XP Bar").gameObject;
        xpBarScript = xpBar.GetComponent<UI_ProgressBar>();
        overXpBar = body.transform.Find("Bars Parent/Bars/Overfill XP Bar").gameObject;
        overXpBarScript = overXpBar.GetComponent<UI_ProgressBar>();
        advanceButton = body.transform.Find("Advance Parent/Advance Button").GetComponent<Button>();
        closeButtonBody = body.transform.Find("Close Button").GetComponent<Button>();

        statPrefabs = ScriptableObject.CreateInstance<PrefabFactory>();
        statPrefabs.SetPrefab(statPrefab);
        trainingMethodPrefabs = ScriptableObject.CreateInstance<PrefabFactory>();
        trainingMethodPrefabs.SetPrefab(trainingMethodPrefab);
    }

    /// <summary>
    ///     Called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        advanceButton.onClick.AddListener(delegate {advanceButtonEvent.RaiseOnChange();});
        closeButtonBody.onClick.AddListener(HideWindow);
    }

    /// <summary>
    ///     Called when the object becomes disabled and inactive.
    /// </summary>
    private void OnDisable()
    {
        Clear();

        advanceButton.onClick.RemoveAllListeners();
        closeButtonBody.onClick.RemoveListener(HideWindow);
    }

    /// <summary>
    ///     Sets the skill instance.
    /// </summary>
    /// <param name="skill">Skill instance for window.</param>
    /// <param name="advanceButtonAction">Function to call when advance button is triggered.</param>
    public void SetSkill(Skill skill, Action advanceButtonAction)
    {
        Clear();
        this.skill = skill;

        // Finds the skill icon sprite and reassigns it.
        icon.sprite = this.skill.icon;
        UpdateRank();
        UpdateXp();

        this.skill.index.OnChange += UpdateRank;
        this.skill.xp.OnChange += UpdateXp;
        this.skill.xpMax.OnChange += UpdateXp;
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
            skill.index.OnChange -= UpdateRank;
            skill.xp.OnChange -= UpdateXp;
            skill.xpMax.OnChange -= UpdateXp;
            skill = null;
        }

        advanceButtonEvent.Clear();
    }

    /// <summary>
    ///     Updates the rank.
    /// </summary>
    private void UpdateRank()
    {
        // Finds the skill name and reassigns it.
        skillName.text = "Rank " + skill.ranks[skill.index.Value] + " " + skill.name;

        int index = skill.index.Value;

        // For every stat, create a new stat field prefab and populate it.
        statPrefabs.SetActiveAll(false);
        body.transform.Find("Stats").gameObject.SetActive(true);
        int i = 0;

        foreach (KeyValuePair<string, float[]> stat in skill.stats)
        {
            if (stat.Key == "ap_cost" || stat.Value[index] == 0)
            {
                continue;
            }

            GameObject obj = statPrefabs.GetFree(stat.Key, statTransform);
            WindowSkillStat script = obj.GetComponent<WindowSkillStat>();
            script.SetText(stat.Key, stat.Value[index]);
            i++;
        }

        if (i == 0)
        {
            body.transform.Find("Stats").gameObject.SetActive(false);
        }

        // For every training method, create a new training method prefab and populate it.
        trainingMethodPrefabs.SetActiveAll(false);
        body.transform.Find("Training Methods").gameObject.SetActive(true);
        i = 0;

        foreach (SkillTrainingMethod method in skill.methods)
        {
            GameObject obj = trainingMethodPrefabs.GetFree(method.name, trainingMethodsTransform);
            WindowSkillTrainingMethod script = obj.GetComponent<WindowSkillTrainingMethod>();
            script.SetMethod(method);
            i++;
        }

        if (i == 0)
        {
            body.transform.Find("Training Methods").gameObject.SetActive(false);
        }

        // Resets the content size fitter.
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)gameObject.transform);
    }
    
    /// <summary>
    ///     Updates the xp bar progress.
    /// </summary>
    private void UpdateXp()
    {
        // If < 100, use normal bar, else use overfill bar.
        if (skill.xp.Value <= 100) 
        {
            xpBar.SetActive(true);
            xpBarScript.SetCurrent(skill.xp.Value);
            xpBarScript.SetMaximum(100);
            overXpBar.SetActive(false);
        }
        else 
        {
            xpBar.SetActive(false);
            overXpBarScript.SetCurrent(skill.xp.Value);
            overXpBarScript.SetMaximum(skill.xpMax.Value);
            overXpBar.SetActive(true);
        }

        if (skill.xp.Value >= 100 && skill.CanRankUp())
        {
            advanceButton.gameObject.SetActive(true);

            int apCost = (int)skill.GetStatForwardDiff("ap_cost");

            if (Player.Instance.actorAP.Value >= apCost)
            {
                advanceButton.interactable = true;
            }
            else
            {
                advanceButton.interactable = false;
            }
        }
        else
        {
            advanceButton.gameObject.SetActive(false);
        }
    }
}
