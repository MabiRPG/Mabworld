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
    public Action nameButtonAction;
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
    ///     Called when the object becomes disabled and inactive.
    /// </summary>
    private void OnDisable()
    {
        // Removes all event listeners
        Clear();
    }

    /// <summary>
    ///     Called at the start of a mouse drag.
    /// </summary>
    /// <param name="pointerData"></param>
    public void OnBeginDrag(PointerEventData pointerData)
    {
        GameObject iconObj = icon.gameObject;

        // If object selected was icon, change cursor and allow dragging
        if (pointerData.pointerEnter == iconObj)
        {
            Cursor.SetCursor(skill.sprite.texture, Vector2.zero, CursorMode.Auto);
            draggingIcon = true;
        }       
    }

    /// <summary>
    ///     Called during a mouse drag.
    /// </summary>
    /// <param name="pointerData"></param>
    public void OnDrag(PointerEventData pointerData)
    {
    }

    /// <summary>
    ///     Called at the end of a mouse drag.
    /// </summary>
    /// <param name="pointerData"></param>
    public void OnEndDrag(PointerEventData pointerData)
    {
        if (draggingIcon)
        {
            // Reset cursor to default
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            draggingIcon = false;

            // Check if valid skill slot destination, if so, set skill info.
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
    public void SetSkill(Skill newSkill, Action nameButtonAction, Action advanceButtonAction)
    {
        Clear();
        skill = newSkill;

        skillName.text = skill.info["name"].ToString();
        icon.sprite = skill.sprite;
        UpdateRank();
        UpdateXp();

        skill.indexEvent.OnChange += UpdateRank;
        skill.xpEvent.OnChange += UpdateXp;
        skill.xpMaxEvent.OnChange += UpdateXp;

        this.nameButtonAction = nameButtonAction;
        nameButton.onClick.AddListener(delegate {nameButtonAction();});
        advanceButton.onClick.AddListener(delegate {advanceButtonAction();});
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
        
        nameButton.onClick.RemoveAllListeners();
        advanceButton.onClick.RemoveAllListeners();
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