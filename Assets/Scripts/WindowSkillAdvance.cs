using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
///     Class for skill advancement window confirmations.
/// </summary>
public class WindowSkillAdvance : Window 
{
    // Prefab for the skill stat text.
    [SerializeField]
    private GameObject statPrefab;
    // Manager of all stat prefab instances.
    private PrefabManager statPrefabs;
    
    // Skill instance to render
    private Skill skill;

    // List of prefab object references.
    private TMP_Text skillName;
    private Image icon;
    private TMP_Text rank;
    private Transform statTransform;
    private TMP_Text ap;
    private Button advanceButton;
    private Button cancelButton;
    
    /// <summary>
    ///     Initializes the object.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        skillName = body.transform.Find("Name").GetComponent<TMP_Text>();
        icon = body.transform.Find("Icon Parent").GetComponentInChildren<Image>();
        rank = body.transform.Find("Advance Text").GetComponent<TMP_Text>();
        statTransform = body.transform.Find("Stats");
        ap = body.transform.Find("AP Text Parent").Find("AP Text").GetComponent<TMP_Text>();
        advanceButton = body.transform.Find("Button Parent").Find("Advance Button").GetComponent<Button>();
        cancelButton = body.transform.Find("Button Parent").Find("Cancel Button").GetComponent<Button>();

        statPrefabs = ScriptableObject.CreateInstance<PrefabManager>();
        statPrefabs.SetPrefab(statPrefab);
    }

    /// <summary>
    ///     Called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        advanceButton.onClick.AddListener(delegate {AdvanceSkill(skill);});
        cancelButton.onClick.AddListener(CloseWindow);
    }

    /// <summary>
    ///     Called when the object becomes disabled and inactive.
    /// </summary>
    private void OnDisable()
    {
        Clear();

        advanceButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveListener(CloseWindow);
    }

    /// <summary>
    ///     Sets the skill instance.
    /// </summary>
    /// <param name="newSkill">Skill instance for window.</param>
    public void SetSkill(Skill newSkill)
    {
        Clear();
        
        skill = newSkill;
        skill.indexEvent.OnChange += CloseWindow;

        Draw();
        ShowWindow();
    }

    /// <summary>
    ///     Clears the window.
    /// </summary>
    private void Clear()
    {
        statPrefabs.SetActiveAll(false);

        if (skill != null)
        {
            skill.indexEvent.OnChange -= CloseWindow;
            skill = null;
        }
    }

    /// <summary>
    ///     Called when advancement button is pressed.
    /// </summary>
    /// <param name="skill">Skill to be advanced.</param>
    private void AdvanceSkill(Skill skill)
    {
        Player.Instance.RankUpSkill(skill);
        CloseWindow();
    }

    /// <summary>
    ///     Draws the window.
    /// </summary>
    private void Draw() 
    {
        int index = skill.index + 1;

        // Finds the skill name and reassigns it.
        skillName.text = "Rank " + skill.rank + " " + skill.info["name"];

        // Finds the skill icon sprite and reassigns it.
        string dir = "Sprites/Skill Icons/" + skill.info["icon_name"].ToString();
        icon.sprite = Resources.Load<Sprite>(dir);

        rank.text = string.Format("Advance to Rank {0} available", skill.ranks[index]);

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

        ap.text = string.Format("{0} AP required.\n({1} AP remaining)",
            skill.stats["ap_cost"][index].ToString(), 
            Player.Instance.actorAP.ToString());
    }
}