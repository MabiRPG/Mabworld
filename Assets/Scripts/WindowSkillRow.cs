using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
///     Handles rendering the skill rows in the skill window.
/// </summary>
public class WindowSkillRow : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Skill instance
    private Skill skill;
    private string iconDir;

    // List of prefab object references
    private TMP_Text skillName;
    private Button nameButton;
    private Image icon;
    private TMP_Text rank;
    private GameObject xpBar;
    private ProgressBar xpBarScript;
    private GameObject overXpBar;
    private ProgressBar overXpBarScript;
    private Button advanceButton;

    // Event handlers
    public EventManager nameButtonEvent = new EventManager();
    private Action nameButtonAction;
    public EventManager advanceButtonEvent = new EventManager();
    private Action advanceButtonAction;
    public bool draggingIcon = false;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    private void Awake()
    {
        skillName = gameObject.transform.Find("Name Button").GetComponentInChildren<TMP_Text>();
        nameButton = gameObject.transform.Find("Name Button").GetComponent<Button>();
        icon = gameObject.transform.Find("Icon").GetComponent<Image>();
        rank = gameObject.transform.Find("Rank").GetComponent<TMP_Text>();
        xpBar = gameObject.transform.Find("XP Bar").gameObject;
        xpBarScript = xpBar.GetComponent<ProgressBar>();
        overXpBar = gameObject.transform.Find("Overfill XP Bar").gameObject;
        overXpBarScript = overXpBar.GetComponent<ProgressBar>();     
        advanceButton = gameObject.transform.Find("Advance Button").GetComponent<Button>(); 
    }

    /// <summary>
    ///     Called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        nameButton.onClick.AddListener(delegate {nameButtonEvent.RaiseOnChange();});
        advanceButton.onClick.AddListener(delegate {advanceButtonEvent.RaiseOnChange();});
    }

    /// <summary>
    ///     Called when the object becomes disabled and inactive.
    /// </summary>
    private void OnDisable()
    {
        // Removes all event listeners
        Clear();

        nameButton.onClick.RemoveAllListeners();
        nameButtonEvent.Clear();

        advanceButton.onClick.RemoveAllListeners();
        advanceButtonEvent.Clear();
    }

    public void OnBeginDrag(PointerEventData pointerData)
    {
        GameObject iconObj = icon.gameObject;

        if (pointerData.pointerEnter == iconObj)
        {
            Cursor.SetCursor(Resources.Load<Sprite>(iconDir).texture, Vector2.zero, CursorMode.Auto);
            draggingIcon = true;
        }       
    }

    public void OnDrag(PointerEventData pointerData)
    {
    }

    public void OnEndDrag(PointerEventData pointerData)
    {
        if (draggingIcon)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            draggingIcon = false;

            if (pointerData.pointerEnter == null)
            {
                return;
            }

            SkillSlot slot = pointerData.pointerEnter.transform.parent.GetComponent<SkillSlot>();

            if (slot != null)
            {
                slot.SetSkill(skill, null, nameButtonAction);
            }
        }
    }

    /// <summary>
    ///     Sets the skill instance.
    /// </summary>
    /// <param name="newSkill">Skill instance for window.</param>
    /// <param name="nameButtonAction">Function to call when name button is triggered.</param>
    /// <param name="advanceButtonAction">Function to call when advance button is triggered.</param>
    public void SetSkill(Skill newSkill, Action newNameButtonAction, Action newAdvanceButtonAction)
    {
        Clear();
        skill = newSkill;

        skillName.text = skill.info["name"].ToString();
        iconDir = "Sprites/Skill Icons/" + skill.info["icon_name"].ToString();
        icon.sprite = Resources.Load<Sprite>(iconDir);     
        UpdateRank();
        UpdateXp();

        skill.indexEvent.OnChange += UpdateRank;
        skill.xpEvent.OnChange += UpdateXp;
        skill.xpMaxEvent.OnChange += UpdateXp;

        nameButtonAction = newNameButtonAction;
        advanceButtonAction = newAdvanceButtonAction;
        nameButtonEvent.OnChange += nameButtonAction;
        advanceButtonEvent.OnChange += advanceButtonAction;
    }

    /// <summary>
    ///     Clears the window.
    /// </summary>
    private void Clear()
    {
        if (skill != null)
        {
            skill.indexEvent.OnChange -= UpdateRank;
            skill.xpEvent.OnChange -= UpdateXp;
            skill.xpMaxEvent.OnChange -= UpdateXp;
            skill = null;
        }
    }

    /// <summary>
    ///     Updates the rank text.
    /// </summary>
    private void UpdateRank()
    {
        rank.text = "Rank " + skill.rank;
    }

    /// <summary>
    ///     Updates the xp bar progress.
    /// </summary>
    private void UpdateXp()
    {
        advanceButton.gameObject.SetActive(false);

        // If < 100, use normal bar, else use overfill bar.
        if (skill.xp <= 100) 
        {
            xpBar.SetActive(true);
            xpBarScript.SetCurrent(skill.xp);
            xpBarScript.SetMaximum(100);
            overXpBar.SetActive(false);

            if (skill.xp == 100) 
            {
                advanceButton.gameObject.SetActive(true);
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