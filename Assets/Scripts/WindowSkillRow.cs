using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowSkillRow : MonoBehaviour
{
    private Skill skill;

    private TMP_Text name;
    private Button nameButton;
    private Image icon;
    private TMP_Text rank;
    private GameObject xpBar;
    private ProgressBar xpBarScript;
    private GameObject overXpBar;
    private ProgressBar overXpBarScript;
    private Button advanceButton;

    public EventManager nameButtonEvent = new EventManager();
    public bool nameButtonSubscribed;
    public EventManager advanceButtonEvent = new EventManager();
    public bool advanceButtonSubscribed;

    private void Awake()
    {
        name = gameObject.transform.Find("Name Button").GetComponentInChildren<TMP_Text>();
        nameButton = gameObject.transform.Find("Name Button").GetComponent<Button>();
        icon = gameObject.GetComponentInChildren<Image>();
        rank = gameObject.transform.Find("Rank").GetComponent<TMP_Text>();
        xpBar = gameObject.transform.Find("XP Bar").gameObject;
        xpBarScript = xpBar.GetComponent<ProgressBar>();
        overXpBar = gameObject.transform.Find("Overfill XP Bar").gameObject;
        overXpBarScript = overXpBar.GetComponent<ProgressBar>();     
        advanceButton = gameObject.transform.Find("Advance Button").GetComponent<Button>();  
    }

    private void OnEnable()
    {
        nameButton.onClick.AddListener(delegate {nameButtonEvent.RaiseOnChange();});
        advanceButton.onClick.AddListener(delegate {advanceButtonEvent.RaiseOnChange();});
    }

    private void OnDisable()
    {
        nameButton.onClick.RemoveAllListeners();
        advanceButton.onClick.RemoveAllListeners();

        if (skill != null)
        {
            skill.indexEvent.OnChange -= UpdateRank;
            skill.xpEvent.OnChange -= UpdateXp;
        }
    }

    public void SetSkill(Skill newSkill)
    {
        skill = newSkill;

        name.text = skill.info["name"].ToString();
        string dir = "Sprites/Skill Icons/" + skill.info["icon_name"].ToString();
        icon.sprite = Resources.Load<Sprite>(dir);     
        UpdateRank();
        UpdateXp();

        skill.indexEvent.OnChange += UpdateRank;
        skill.xpEvent.OnChange += UpdateXp;
    }

    private void UpdateRank()
    {
        rank.text = "Rank " + skill.rank;
    }

    private void UpdateXp()
    {
        if (skill.xp <= 100) 
        {
            xpBar.SetActive(true);
            xpBarScript.SetCurrent(skill.xp);
            xpBarScript.SetMaximum(100);
            overXpBar.SetActive(false);

            // Remove the rank up button if cannot rank up.
            if (skill.xp < 100) 
            {
                advanceButton.gameObject.SetActive(false);
            }
        }
        else 
        {
            xpBar.SetActive(false);
            overXpBarScript.SetCurrent(skill.xp);
            overXpBarScript.SetMaximum(skill.xpMax);
            overXpBar.SetActive(true);
            advanceButton.gameObject.SetActive(true);
        }
    }
}