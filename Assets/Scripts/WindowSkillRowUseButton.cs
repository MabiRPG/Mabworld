using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WindowSkillRowUseButton : MonoBehaviour, IInputHandler
{
    private Skill skill;
    private Button button;
    private TMP_Text buttonText;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TMP_Text>();
    }

    public void SetSkill(Skill skill)
    {
        this.skill = skill;

        if (skill.isPassive)
        {
            button.interactable = false;
            buttonText.text = "Passive";
        }
        else
        {
            button.interactable = true;
            buttonText.text = "Use";
        }
    }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        throw new System.NotImplementedException();
    }

    public void HandleKeyboardInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        throw new System.NotImplementedException();
    }
}