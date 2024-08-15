using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WindowSkillAdvanceButton : MonoBehaviour, IInputHandler
{
    public Skill skill;

    public void SetSkill(Skill skill)
    {
        if (this.skill != null)
        {
            this.skill.xp.OnChange -= Draw;
        }

        this.skill = skill;
        skill.xp.OnChange += Draw;
        Draw();
    }

    private void Draw()
    {
        if (skill.xp.Value >= 100)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        GameObject obj = WindowSkill.Instance.advancePrefabFactory.GetFree(skill,
            GameManager.Instance.canvas.transform);
        WindowSkillAdvance window = obj.GetComponent<WindowSkillAdvance>();
        window.SetSkill(skill);
        WindowManager.Instance.SetActive(window);
    }

    public void HandleKeyboardInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
    }
}