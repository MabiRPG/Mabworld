using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowSkillRowIcon : MonoBehaviour
{
    private Skill skill;
    private Image icon;
    private TMP_Text cooldown;

    private void Awake()
    {
        icon = GetComponent<Image>();
        cooldown = GetComponentInChildren<TMP_Text>();
    }

    public void SetSkill(Skill skill)
    {
        if (this.skill != null)
        {
            this.skill.cooldown.OnChange -= Draw;
        }

        this.skill = skill;
        icon.sprite = skill.icon;
        skill.cooldown.OnChange += Draw;
        Draw();
    }

    private void OnDisable()
    {
        skill.cooldown.OnChange -= Draw;
    }

    private void Draw()
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