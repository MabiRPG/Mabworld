using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Skill slot for player skill hotkeys
/// </summary>
public class SkillSlot : MonoBehaviour 
{
    // Keycode for slot
    public KeyCode key = KeyCode.F1;

    private Skill skill;
    // References to prefab objects
    private Image icon;
    private TMP_Text text;
    private Button button;

    // Actions to invoke when triggered
    private Action useAction;
    private Action openWindowAction;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    private void Awake()
    {
        icon = GetComponentInChildren<Image>();
        text = transform.Find("Key").GetComponentInChildren<TMP_Text>();
        text.text = key.ToString();
        button = GetComponentInChildren<Button>();
    }

    /// <summary>
    ///     Called when the object becomes disabled and inactive.
    /// </summary>
    private void OnDisable()
    {
        button.onClick.RemoveAllListeners();
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
        button.onClick.AddListener(UseSkill);
    }

    private void UseSkill()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            openWindowAction.Invoke();
        }
        else
        {
            //useAction.Invoke();
        }
    }
}