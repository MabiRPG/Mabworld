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
    private TMP_Text cooldown;
    private Sprite defaultIcon;
    private TMP_Text keyText;
    private GameObject slotItem;

    // Actions to invoke when triggered
    private Action openWindowAction;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    private void Awake()
    {
        icon = transform.Find("Icon").GetComponent<Image>();
        cooldown = transform.Find("Icon").Find("Cooldown Timer").GetComponent<TMP_Text>();
        defaultIcon = icon.sprite;
        keyText = transform.Find("Key").GetComponent<TMP_Text>();
        keyText.text = key.ToString();
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
                //Player.Instance.StartAction(skill);
                Player.Instance.LoadSkill(Player.Instance.skillManager.Get(skill.ID));
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
    /// <param name="openWindowAction">Action triggered when skill details is pressed</param>
    public void SetSkill(Skill skill, Action openWindowAction)
    {
        this.skill = skill;
        icon.sprite = this.skill.icon;
        this.skill.cooldown.OnChange += UpdateCooldown;
        UpdateCooldown();

        InputManager.Instance.AddButtonBind(key, () => Player.Instance.LoadSkill(skill));
        this.openWindowAction = openWindowAction;
    }

    /// <summary>
    ///     Clears the slot
    /// </summary>
    private void Clear()
    {
        skill.cooldown.OnChange -= UpdateCooldown;
        icon.fillAmount = 1f;
        cooldown.gameObject.SetActive(false);
        skill = null;
        icon.sprite = defaultIcon;
        InputManager.Instance.RemoveButtonBind(key);
    }

    /// <summary>
    ///     Updates the cooldown timers for the skill.
    /// </summary>
    private void UpdateCooldown()
    {
        if (skill.cooldown.Value > 0)
        {
            float iconFillAmount = (skill.GetCooldownTime() - skill.cooldown.Value) / skill.GetCooldownTime();
            icon.fillAmount = iconFillAmount;
            cooldown.gameObject.SetActive(true);

            if (skill.cooldown.Value > 60)
            {
                cooldown.text = string.Format("{0:0}m", (int)skill.cooldown.Value / 60);
            }
            else if (skill.cooldown.Value > 10)
            {
                cooldown.text = string.Format("{0:0}s", skill.cooldown.Value);
            }
            else
            {
                cooldown.text = string.Format("{0:0.0}s", skill.cooldown.Value);
            }
        }
        else
        {
            icon.fillAmount = 1f;
            cooldown.gameObject.SetActive(false);
        }
    }
}