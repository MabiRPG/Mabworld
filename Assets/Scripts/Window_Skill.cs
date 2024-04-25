using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
///     This class handles the skill window processing.
/// </summary>
public class Window_Skill : Window
{
    // Global reference.
    public static Window_Skill instance = null;
    
    // Prefab for every skill row in window.
    public GameObject skillPrefab;
    // Prefab for extra skill detail window.
    public GameObject windowSkillDetailedPrefab;
    // Lists of prefabs.
    private List<GameObject> skillList = new List<GameObject>();
    private List<GameObject> windowSkillDetailedList = new List<GameObject>(); 

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    protected override void Awake() 
    {
        base.Awake();
        SetTitle("Skills");

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
            GameObject obj = Instantiate(skillPrefab, body.transform);
            
            // Finds the skill name field and reassigns it.
            GameObject nameObj = obj.transform.Find("Name Button").gameObject;
            TMP_Text name = nameObj.GetComponentInChildren<TMP_Text>();
            name.text = skill.Value.info["name"].ToString();
            Button nameButton = nameObj.GetComponent<Button>();
            nameButton.onClick.AddListener(delegate {CreateDetailedSkillWindow(skill.Value);});

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
                xpBarScript.SetCurrent(skill.Value.xp);
                xpBarScript.SetMaximum(100);
                overXpBar.SetActive(false);

                // Remove the rank up button if cannot rank up.
                if (skill.Value.xp < 100) 
                {
                    rankUpButton.gameObject.SetActive(false);
                }
            }
            else 
            {
                xpBar.SetActive(false);
                overXpBarScript.SetCurrent(skill.Value.xp);
                overXpBarScript.SetMaximum(skill.Value.xpMax);
            }

            // Adds it to the list.
            skillList.Add(obj);
        }
    }

    /// <summary>
    ///     Toggles visibility of the object.
    /// </summary>
    public void ToggleVisible()
    {
        // Changes visibilty of object.
        gameObject.SetActive(!gameObject.activeSelf);
        ClearPrefabs(skillList);

        if (gameObject.activeSelf) 
        {
            Draw();
        }
    }

    /// <summary>
    ///     Ranks up the skill.
    /// </summary>
    /// <param name="skill">Skill instance to rank up.</param>
    private void RankUpSkill(Skill skill)
    {
        skill.RankUp();
        ClearPrefabs(skillList);
        Draw();
    }

    /// <summary>
    ///     Creates a new detailed skill window for the skill.
    /// </summary>
    /// <param name="skill">Skill to populate window.</param>
    private void CreateDetailedSkillWindow(Skill skill)
    {
        GameObject obj = Instantiate(windowSkillDetailedPrefab, transform.parent);
        Window_Skill_Detailed window = obj.GetComponent<Window_Skill_Detailed>();
        window.Init(skill);
        windowSkillDetailedList.Add(obj);
    }
}
