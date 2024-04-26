using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WindowSkillAdvance : Window 
{
    public GameObject statPrefab;
    
    private Skill skill;

    // List of prefab objects.
    private List<GameObject> statList = new List<GameObject>();

    public void Init(Skill newSkill)
    {
        skill = newSkill;
        skill.rankUpEvent.AddListener(CloseWindow);
        Draw();
    }

    private void Draw() 
    {
        // Finds the skill name and reassigns it.
        TMP_Text name = body.transform.Find("Name").GetComponent<TMP_Text>();
        name.text = "Rank " + skill.rank + " " + skill.info["name"];

        // Finds the skill icon sprite and reassigns it.
        Image img = body.transform.Find("Icon Parent").GetComponentInChildren<Image>();
        string dir = "Sprites/Skill Icons/" + skill.info["icon_name"].ToString();
        img.sprite = Resources.Load<Sprite>(dir);

        TMP_Text advance = body.transform.Find("Advance Text").GetComponent<TMP_Text>();
        advance.text = advance.text.Replace("{Rank}", "Rank " + skill.rank);

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

        TMP_Text ap = body.transform.Find("AP Text Parent").Find("AP Text").GetComponent<TMP_Text>();
        ap.text = ap.text.Replace("{Skill.AP}", skill.stats["ap_cost"][index].ToString());
        ap.text = ap.text.Replace("{Player.AP}", Player.instance.actorAP.ToString());

        Button advanceButton = body.transform.Find("Button Parent").Find("Advance Button").GetComponent<Button>();
        advanceButton.onClick.AddListener(delegate {Player.instance.RankUpSkill(skill); CloseWindow();});

        Button cancelButton = body.transform.Find("Button Parent").Find("Cancel Button").GetComponent<Button>();
        cancelButton.onClick.AddListener(CloseWindow);
    }
}