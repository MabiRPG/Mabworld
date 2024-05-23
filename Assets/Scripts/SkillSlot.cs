using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
///     Skill slot for player skill hotkeys
/// </summary>
public class SkillSlot : MonoBehaviour, IPointerClickHandler
{
    // Keycode for slot
    public KeyCode key = KeyCode.F1;

    private Skill skill;
    // References to prefab objects
    private Image icon;
    private Sprite defaultIcon;
    private TMP_Text text;
    private GameObject slotItem;

    // Actions to invoke when triggered
    private Action openWindowAction;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    private void Awake()
    {
        icon = GetComponentInChildren<Image>();
        defaultIcon = icon.sprite;
        text = transform.Find("Key").GetComponentInChildren<TMP_Text>();
        text.text = key.ToString();
        slotItem = transform.Find("Icon").gameObject;
    }

    /// <summary>
    ///     Called during a mouse click.
    /// </summary>
    /// <param name="pointerData"></param>
    public void OnPointerClick(PointerEventData pointerData)
    {
        // Only call if the slot image has been pressed and skill is present
        if (pointerData.pointerEnter != slotItem || skill == null)
        {
            return;
        }

        // If left mouse button pressed
        if (pointerData.pointerId == -1)
        {
            // If left shift or right shift, open detailed skill window, otherwise use skill
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                openWindowAction.Invoke();
            }
            else
            {
                Player.Instance.StartAction(skill);
            }
        }
        // If right mouse button pressed
        else if (pointerData.pointerId == -2)
        {
            Clear();
        }
    }

    /// <summary>
    ///     Sets the skill slot
    /// </summary>
    /// <param name="skill">Skill instance</param>
    /// <param name="newUseAction">Action triggered when skill is used</param>
    /// <param name="openWindowAction">Action triggered when skill details is pressed</param>
    public void SetSkill(Skill skill, Action newUseAction, Action openWindowAction)
    {
        this.skill = skill;
        icon.sprite = this.skill.icon;

        InputManager.Instance.AddButtonBind(key, () => Player.Instance.StartAction(this.skill));
        this.openWindowAction = openWindowAction;
    }

    /// <summary>
    ///     Clears the slot
    /// </summary>
    private void Clear()
    {
        skill = null;
        icon.sprite = defaultIcon;
        InputManager.Instance.RemoveButtonBind(key);
    }
}