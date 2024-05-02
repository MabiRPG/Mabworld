using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    private Button closeButton;

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
        closeButton.onClick.AddListener(CloseWindow);
    }

    /// <summary>
    ///     Called when the object becomes disabled and inactive.
    /// </summary>
    private void OnDisable()
    {
        Clear();

        if (skill != null)
        {
            skill.indexEvent.OnValueChange -= Draw;
            skill.xpEvent.OnValueChange -= DrawBars;
            skill.xpMaxEvent.OnValueChange -= DrawBars;
        }

        closeButton.onClick.RemoveListener(CloseWindow);
    }

    /// <summary>
    ///     Initializes the object manually with parameters.
    /// </summary>
    /// <param name="newSkill">Skill instance for window.</param>
    public void Setup(Skill newSkill)
    {
        ShowWindow();
        
        skill = newSkill;
        skill.indexEvent.OnValueChange += Draw;
        skill.xpEvent.OnValueChange += DrawBars;
        skill.xpMaxEvent.OnValueChange += DrawBars;

        Clear();
        Draw();
    }

    /// <summary>
    ///     Clears the window.
    /// </summary>
    private void Clear()
    {
        statPrefabs.SetActiveAll(false);
        trainingMethodPrefabs.SetActiveAll(false);
    }

    /// <summary>
    ///     Draws the window.
    /// </summary>
    private void Draw()
    {
        // Finds the skill name and reassigns it.
        skillName.text = "Rank " + skill.rank + " " + skill.info["name"];

        // Finds the skill icon sprite and reassigns it.
        string dir = "Sprites/Skill Icons/" + skill.info["icon_name"].ToString();
        icon.sprite = Resources.Load<Sprite>(dir);

        int index = skill.index;

        // For every stat, create a new stat field prefab and populate it.
        foreach (KeyValuePair<string, float[]> stat in skill.stats)
        {
            string statName = stat.Key.ToString();
            string statValue = stat.Value[index].ToString();

            if (statName.Equals("ap_cost") || float.Parse(statValue).Equals(0))
            {
                continue;
            }
            else if (statName.Contains("rate"))
            {
                statValue = "+" + statValue;
                statValue += "%";
            }
            else if (statName.Contains("time"))
            {
                statValue += "s";
            }

            statName = ToCapitalize(statName.Replace("_", " "));
            GameObject obj = statPrefabs.GetFree(statName, statTransform);
            obj.SetActive(true);

            // Generates the stat field for every skill stat.
            TMP_Text field = obj.GetComponent<TMP_Text>();
            field.text = statName + ": " + statValue;
        }

        // For every training method, create a new training method prefab and populate it.
        foreach (TrainingMethod method in skill.methods)
        {
            string methodName = method.method["name"].ToString();
            string methodValue = string.Format("+{0:0.00} (<color=\"yellow\">{1}<color=\"white\">/{2})",
                float.Parse(method.method["xp_gain_each"].ToString()), method.method["count"],
                method.method["count_max"].ToString());

            GameObject obj = trainingMethodPrefabs.GetFree(methodName, trainingMethodsTransform);
            obj.SetActive(true);
            
            TMP_Text name = obj.transform.Find("Method Name").GetComponent<TMP_Text>();
            TMP_Text field = obj.transform.Find("Method Values").GetComponent<TMP_Text>();
            name.text = methodName;
            field.text = methodValue;
        }

        DrawBars();
    }
    
    /// <summary>
    ///  Draws the xp bars for the skill.
    /// </summary>
    private void DrawBars()
    {
        // If < 100, use normal bar, else use overfill bar.
        if (skill.xp <= 100) 
        {
            xpBarScript.SetCurrent(skill.xp);
            xpBarScript.SetMaximum(100);
            overXpBar.SetActive(false);

            // Remove the rank up button if cannot rank up.
            // if (skill.xp < 100) 
            // {
            //     rankUpButton.gameObject.SetActive(false);
            // }
        }
        else 
        {
            xpBar.SetActive(false);
            overXpBarScript.SetCurrent(skill.xp);
            overXpBarScript.SetMaximum(skill.xpMax);
        }       
    }
}
