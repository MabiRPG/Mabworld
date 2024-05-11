using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlot : MonoBehaviour 
{
    public KeyCode key = KeyCode.F1;

    private Skill skill;
    private Image icon;
    private TMP_Text text;
    private Button button;

    private Action useAction;
    private Action openWindowAction;

    private void Awake()
    {
        icon = GetComponentInChildren<Image>();
        text = transform.Find("Key").GetComponentInChildren<TMP_Text>();
        text.text = key.ToString();
        button = GetComponentInChildren<Button>();
    }

    private void OnDisable()
    {
        button.onClick.RemoveAllListeners();
    }

    public void SetSkill(Skill newSkill, Action newUseAction, Action newOpenWindowAction)
    {
        skill = newSkill;
        string iconDir = skill.info["icon_name"].ToString();
        icon.sprite = skill.sprite;

        InputManager.Instance.AddButton(key, () => StartCoroutine(skill.Use()));
        openWindowAction = newOpenWindowAction;
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