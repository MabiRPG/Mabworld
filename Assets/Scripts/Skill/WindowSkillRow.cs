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
    public Skill skill;

    // List of prefab object references
    private TMP_Text rank;

    private WindowSkillRowIcon icon;
    private WindowSkillRowNameButton nameButton;
    private WindowSkillRowUseButton useButton;
    private WindowSkillAdvanceButton advanceButton;
    private WindowSkillXPBar[] xpBars;

    // Event handlers
    public Action nameButtonAction;
    public bool isDragging = false;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    private void Awake()
    {
        rank = transform.Find("Rank").GetComponent<TMP_Text>();

        icon = GetComponentInChildren<WindowSkillRowIcon>();
        nameButton = GetComponentInChildren<WindowSkillRowNameButton>();
        useButton = GetComponentInChildren<WindowSkillRowUseButton>();
        advanceButton = GetComponentInChildren<WindowSkillAdvanceButton>();
        xpBars = GetComponentsInChildren<WindowSkillXPBar>(true);
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
            Cursor.SetCursor(skill.model.icon.texture, Vector2.zero, CursorMode.Auto);
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
                slot.SetSkill(skill, nameButtonAction);
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
        this.skill = skill;
        UpdateRank();

        this.skill.index.OnChange += UpdateRank;

        icon.SetSkill(skill);
        nameButton.SetSkill(skill);
        useButton.SetSkill(skill);
        advanceButton.SetSkill(skill);

        foreach (WindowSkillXPBar bar in xpBars)
        {
            bar.SetSkill(skill);
        }
    }

    /// <summary>
    ///     Updates the rank text.
    /// </summary>
    private void UpdateRank()
    {
        rank.text = "Rank " + SkillModel.ranks[skill.index.Value];
    }
}