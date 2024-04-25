using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
///     This class handles the detailed skill window processing.
/// </summary>
public class Window_Skill_Detailed : Window
{
    // Prefab for every skill stat rendering
    public GameObject statPrefab;
    // Prefab for every training method rendering.
    public GameObject trainingMethodPrefab;

    // Skill of window
    private Skill skill;
    // List of prefab objects.
    private List<GameObject> statList = new List<GameObject>();
    private List<GameObject> trainingMethodList = new List<GameObject>();

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        SetTitle("Skill Info");
    }

    /// <summary>
    ///     Initializes the object manually with parameters.
    /// </summary>
    /// <param name="newSkill">Skill instance for window.</param>
    public void Init(Skill newSkill)
    {
        skill = newSkill;
        Draw();
    }

    /// <summary>
    ///     Draws the window.
    /// </summary>
    private void Draw()
    {
        // Finds the skill name and reassigns it.
        TMP_Text name = body.transform.Find("Name").GetComponent<TMP_Text>();
        name.text = "Rank " + skill.rank + " " + skill.info["name"];

        // Finds the skill icon sprite and reassigns it.
        Image img = body.transform.Find("Icon Parent").GetComponentInChildren<Image>();
        string dir = "Sprites/Skill Icons/" + skill.info["icon_name"].ToString();
        img.sprite = Resources.Load<Sprite>(dir);

        int index = skill.index;
        Transform statTransform = body.transform.Find("Stats");

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

            GameObject obj = Instantiate(statPrefab, statTransform);

            // Generates the stat field for every skill stat.
            TMP_Text field = obj.GetComponent<TMP_Text>();
            field.text = statName + ": " + statValue;

            statList.Add(obj);
        }

        Transform trainingMethodsTransform = body.transform.Find("Training Methods");

        // For every training method, create a new training method prefab and populate it.
        foreach (TrainingMethod method in skill.methods)
        {
            string methodName = method.method["name"].ToString();
            string methodValue = string.Format("+{0:0.00} (<color=\"yellow\">{1}<color=\"white\">/{2})",
                float.Parse(method.method["xp_gain_each"].ToString()), method.method["count"],
                method.method["count_max"].ToString());

            GameObject obj = Instantiate(trainingMethodPrefab, trainingMethodsTransform);

            name = obj.transform.Find("Method Name").GetComponent<TMP_Text>();
            TMP_Text field = obj.transform.Find("Method Values").GetComponent<TMP_Text>();
            name.text = methodName;
            field.text = methodValue;

            trainingMethodList.Add(obj);
        }

        // Finds the xp bar.
        GameObject xpBar = body.transform.Find("Bars Parent").Find("Bars").Find("XP Bar").gameObject;
        Progress_Bar xpBarScript = xpBar.GetComponent<Progress_Bar>();
        GameObject overXpBar = body.transform.Find("Bars Parent").Find("Bars").Find("Overfill XP Bar").gameObject;
        Progress_Bar overXpBarScript = overXpBar.GetComponent<Progress_Bar>();

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

        Button closeButton = body.transform.Find("Close Button").GetComponent<Button>();
        closeButton.onClick.AddListener(CloseWindow);
    }
}