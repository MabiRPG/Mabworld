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
        int index = skill.index;
        Transform statTransform = transform.Find("Stats");

        // Finds the skill name and reassigns it.
        TMP_Text name = gameObject.transform.Find("Name").GetComponent<TMP_Text>();
        name.text = "Rank " + skill.rank + " " + skill.info["name"];

        // Finds the skill icon sprite and reassigns it.
        Image img = gameObject.GetComponentInChildren<Image>();
        string dir = "Sprites/Skill Icons/" + skill.info["icon_name"].ToString();
        img.sprite = Resources.Load<Sprite>(dir);

        // For every skill, create a new stat field prefab and populate it.
        foreach (KeyValuePair<string, float[]> stat in skill.stats)
        {
            string statName = stat.Key.ToString();
            string statValue = stat.Value[index].ToString();

            if (statName == "ap_cost")
            {
                continue;
            }

            GameObject obj = Instantiate(statPrefab, statTransform);

            // Generates the stat field for every skill stat.
            TMP_Text field = obj.GetComponent<TMP_Text>();
            field.text = statName + ": " + statValue;

            statList.Add(obj);
        }
    }
}
