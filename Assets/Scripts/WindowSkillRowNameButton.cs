using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowSkillRowNameButton : MonoBehaviour, IInputHandler
{
    public Skill skill;
    private TMP_Text tName;

    private void Awake()
    {
        tName = GetComponentInChildren<TMP_Text>();
    }

    public void SetSkill(Skill skill)
    {
        this.skill = skill;
        tName.text = skill.name;
    }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        GameObject obj = WindowSkill.Instance.detailedPrefabFactory.GetFree(skill, 
            GameManager.Instance.canvas.transform);
        WindowSkillDetailed window = obj.GetComponent<WindowSkillDetailed>();
        window.SetSkill(skill, () => { });
        WindowManager.Instance.SetActive(window);
    }

    public void HandleKeyboardInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        throw new System.NotImplementedException();
    }
}