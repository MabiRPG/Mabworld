using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Window_Skill_Detailed : Window
{
    public GameObject statPrefab;
    public GameObject trainingMethodPrefab;

    private Skill skill;
    private List<GameObject> statList = new List<GameObject>();
    private List<GameObject> trainingMethodList = new List<GameObject>();

    public void Init(Skill newSkill)
    {
        skill = newSkill;
        PopulateWindow();
    }

    private void PopulateWindow()
    {
        // Finds the skill name and reassigns it.
        TMP_Text name = gameObject.transform.Find("Name").GetComponent<TMP_Text>();
        name.text = "Rank " + skill.rank + " " + skill.info["name"];

        // Finds the skill icon sprite and reassigns it.
        Image img = gameObject.transform.Find("Icon Parent").GetComponentInChildren<Image>();
        string dir = "Sprites/Skill Icons/" + skill.info["icon_name"].ToString();
        img.sprite = Resources.Load<Sprite>(dir);

        int index = skill.index;
        Transform statTransform = transform.Find("Stats");

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

        Transform trainingMethodsTransform = transform.Find("Training Methods");

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
    }
}
