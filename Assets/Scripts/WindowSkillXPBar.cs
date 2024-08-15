using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowSkillXPBar : MonoBehaviour, IInputHandler
{
    private Skill skill;
    private ProgressBar bar;
    [SerializeField]
    private bool overflowBar;

    private void Awake()
    {
        bar = GetComponent<ProgressBar>();
    }

    public void SetSkill(Skill skill)
    {
        if (this.skill != null)
        {
            this.skill.xp.OnChange -= Draw;
            this.skill.xpMax.OnChange -= Draw;
        }

        this.skill = skill;
        this.skill.xp.OnChange += Draw;
        this.skill.xpMax.OnChange += Draw;

        Draw();
    }

    private void Draw()
    {
        if ((overflowBar && skill.xp.Value > 100) || (!overflowBar && skill.xp.Value <= 100))
        {
            gameObject.SetActive(true);
            bar.SetCurrent(skill.xp.Value);

            if (overflowBar)
            {
                bar.SetMaximum(skill.xpMax.Value);
            }
            else
            {
                bar.SetMaximum(100f);
            }
        }
        else
        {
            gameObject.SetActive(false);
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