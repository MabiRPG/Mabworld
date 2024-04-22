using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Window_Skill : MonoBehaviour
{
    public static Window_Skill instance = null;
    
    public GameObject skillPrefab;
    private List<GameObject> skillList = new List<GameObject>(); 
    
    void Awake() 
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        // Hides the object at start
        gameObject.SetActive(false);
    }

    public void PopulateWindow()
    {
        // For every skill, create a new prefab to display skill info in window.
        foreach (KeyValuePair<int, Skill> skill in Player.instance.skills)
        {
            // Instantiates the prefab in the window. Parent window has
            // a vertical layout group to control children components.
            GameObject obj = Instantiate(skillPrefab, transform);
            
            // Finds the skill name field and reassigns it.
            TMP_Text name = obj.transform.Find("Name").GetComponent<TMP_Text>();
            name.text = skill.Value.info["name"].ToString();

            // Finds the skill icon sprite and reassigns it.
            Image img = obj.GetComponentInChildren<Image>();
            string dir = "Sprites/Skill Icons/" + skill.Value.info["icon_name"].ToString();
            img.sprite = Resources.Load<Sprite>(dir);

            // Finds the skill rank field and reassigns it.
            TMP_Text rank = obj.transform.Find("Rank").GetComponent<TMP_Text>();
            rank.text = "Rank " + skill.Value.rank;

            // Finds the xp bar.
            GameObject xpBar = obj.transform.Find("XP Bar").gameObject;
            Progress_Bar xpBarScript = xpBar.GetComponent<Progress_Bar>();
            GameObject overXpBar = obj.transform.Find("Overfill XP Bar").gameObject;
            Progress_Bar overXpBarScript = overXpBar.GetComponent<Progress_Bar>();

            // Finds the advancement button.
            Button rankUpButton = obj.transform.Find("Rank Up Button").GetComponent<Button>();
            rankUpButton.onClick.AddListener(delegate {RankUpSkill(skill.Value);});

            // If < 100, use normal bar, else use overfill bar.
            if (skill.Value.xp <= 100) 
            {
                xpBarScript.current = skill.Value.xp;
                xpBarScript.maximum = 100;
                overXpBar.SetActive(false);

                if (skill.Value.xp < 100) 
                {
                    rankUpButton.gameObject.SetActive(false);
                }
            }
            else 
            {
                xpBar.SetActive(false);
                overXpBarScript.current = skill.Value.xp;
                overXpBarScript.maximum = skill.Value.xpMax;
            }

            // Adds it to the list.
            skillList.Add(obj);
        }
    }

    public void ClearWindow()
    {
        // Destroys all objects
        foreach (GameObject obj in skillList)
        {
            Destroy(obj);
        }

        // Clears the list for update
        skillList.Clear();
    }      

    public void ToggleVisible()
    {
        // Changes visibilty of object.
        gameObject.SetActive(!gameObject.activeSelf);
        ClearWindow();
        PopulateWindow();
    }

    public void RankUpSkill(Skill skill)
    {
        skill.RankUp();
        ClearWindow();
        PopulateWindow();
    }
}
