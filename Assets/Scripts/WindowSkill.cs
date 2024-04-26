using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
///     This class handles the skill window processing.
/// </summary>
public class WindowSkill : Window
{
    // Global reference.
    public static WindowSkill instance = null;
    
    // Prefab for every skill row in window.
    public GameObject skillPrefab;
    // Prefab for extra skill detail window.
    public GameObject windowSkillDetailedPrefab;
    // Prefab for skill advancement.
    public GameObject windowSkillAdvancePrefab;
    // Dict of prefabs.
    private Dictionary<Skill, GameObject> skillPrefabs = new Dictionary<Skill, GameObject>();
    private Dictionary<Skill, GameObject> windowSkillDetailedPrefabs = new Dictionary<Skill, GameObject>();

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
            GameObject obj;
            
            if (!skillPrefabs.ContainsKey(skill.Value))
            {
                obj = Instantiate(skillPrefab, body.transform);
                skillPrefabs.Add(skill.Value, obj);
            }
            else
            {
                obj = skillPrefabs[skill.Value];
            }
            
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
            Button advanceButton = obj.transform.Find("Advance Button").GetComponent<Button>();
            //rankUpButton.onClick.AddListener(delegate {RankUpSkill(skill.Value);});
            advanceButton.onClick.AddListener(delegate {CreateAdvanceSkillWindow(skill.Value);});

            // If < 100, use normal bar, else use overfill bar.
            if (skill.Value.xp <= 100) 
            {
                xpBarScript.SetCurrent(skill.Value.xp);
                xpBarScript.SetMaximum(100);
                overXpBar.SetActive(false);

                // Remove the rank up button if cannot rank up.
                if (skill.Value.xp < 100) 
                {
                    advanceButton.gameObject.SetActive(false);
                }
            }
            else 
            {
                xpBar.SetActive(false);
                overXpBarScript.SetCurrent(skill.Value.xp);
                overXpBarScript.SetMaximum(skill.Value.xpMax);
            }

            skill.Value.rankUpEvent.AddListener(Draw);
        }
    }

    /// <summary>
    ///     Toggles visibility of the object.
    /// </summary>
    public void ToggleVisible()
    {
        // Changes visibilty of object.
        gameObject.SetActive(!gameObject.activeSelf);
        //ClearPrefabs(skillPrefabs);

        if (gameObject.activeSelf) 
        {
            Draw();
        }
    }

    private void CreateAdvanceSkillWindow(Skill skill)
    {
        GameObject obj = Instantiate(windowSkillAdvancePrefab, transform.parent);
        WindowSkillAdvance window = obj.GetComponent<WindowSkillAdvance>();
        window.Init(skill);
    }

    /// <summary>
    ///     Creates a new detailed skill window for the skill.
    /// </summary>
    /// <param name="skill">Skill to populate window.</param>
    private void CreateDetailedSkillWindow(Skill skill)
    {
        GameObject obj;

        if (!windowSkillDetailedPrefabs.ContainsKey(skill))
        {
            obj = Instantiate(windowSkillDetailedPrefab, transform.parent);
            windowSkillDetailedPrefabs.Add(skill, obj);
            WindowSkillDetailed window = obj.GetComponent<WindowSkillDetailed>();
            window.Init(skill);
        }
        else
        {
            obj = windowSkillDetailedPrefabs[skill];
            WindowSkillDetailed window = obj.GetComponent<WindowSkillDetailed>();
            window.ShowWindow();
        }
    }
}
