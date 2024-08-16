using UnityEngine;
using UnityEngine.UI;

public class WindowSkillAdvanceButton : MonoBehaviour
{
    public Skill skill;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void SetSkill(Skill skill)
    {
        if (this.skill != null)
        {
            this.skill.xp.OnChange -= Draw;
            this.skill.index.OnChange -= Draw;
            button.onClick.RemoveAllListeners();
        }

        this.skill = skill;
        skill.xp.OnChange += Draw;
        skill.index.OnChange += Draw;

        button.onClick.AddListener(OpenAdvanceWindow);
        Draw();
    }

    private void Draw()
    {
        if (skill.xp.Value >= 100)
        {
            gameObject.SetActive(true);

            if (HasEnoughAP())
            {
                button.interactable = true;
            }
            else
            {
                button.interactable = false;
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private bool HasEnoughAP()
    {
        return Player.Instance.actorAP.Value >= skill.GetStatForwardDiff("ap_cost");
    }

    private void OpenAdvanceWindow()
    {
        if (HasEnoughAP())
        {
            WindowSkill.Instance.CreateAdvanceWindow(skill);
        }
    }
}