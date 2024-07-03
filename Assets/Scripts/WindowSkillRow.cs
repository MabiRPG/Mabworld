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
    public bool isDragging = false;

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
            Cursor.SetCursor(skill.icon.texture, Vector2.zero, CursorMode.Auto);
            isDragging = true;
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
        if (isDragging)
        {
            // Reset cursor to default
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            isDragging = false;

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
    /// <param name="skill">Skill instance for window.</param>
    /// <param name="nameButtonAction">Function to call when name button is triggered.</param>
    /// <param name="advanceButtonAction">Function to call when advance button is triggered.</param>
    public void SetSkill(Skill skill, Action nameButtonAction, Action advanceButtonAction)
    {
        Clear();
        this.skill = skill;

        skillName.text = this.skill.name;
        icon.sprite = this.skill.icon;
        UpdateRank();
        UpdateXp();

        this.skill.index.OnChange += UpdateRank;
        this.skill.xp.OnChange += UpdateXp;
        this.skill.xpMax.OnChange += UpdateXp;

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
            skill.index.OnChange -= UpdateRank;
            skill.xp.OnChange -= UpdateXp;
            skill.xpMax.OnChange -= UpdateXp;
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
        rank.text = "Rank " + skill.ranks[skill.index.Value];
    }

    /// <summary>
    ///     Updates the xp bar progress.
    /// </summary>
    private void UpdateXp()
    {
        // If < 100, use normal bar, else use overfill bar.
        if (skill.xp.Value <= 100) 
        {
            xpBar.SetActive(true);
            xpBarScript.SetCurrent(skill.xp.Value);
            xpBarScript.SetMaximum(100);
            overXpBar.SetActive(false);
        }
        else 
        {
            xpBar.SetActive(false);
            overXpBarScript.SetCurrent(skill.xp.Value);
            overXpBarScript.SetMaximum(skill.xpMax.Value);
            overXpBar.SetActive(true);
        }

        if (skill.xp.Value >= 100 && skill.CanRankUp())
        {
            advanceButton.gameObject.SetActive(true);

            int apCost = (int)skill.GetStatForwardDiff("ap_cost");

            if (Player.Instance.actorAP.Value >= apCost)
            {
                advanceButton.interactable = true;
            }
            else
            {
                advanceButton.interactable = false;
            }
        }
        else
        {
            advanceButton.gameObject.SetActive(false);
        }
    }
}