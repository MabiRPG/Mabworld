using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowSkillRowNameButton : MonoBehaviour
{
    public Skill skill;
    private Button button;
    private TMP_Text tName;

    private void Awake()
    {
        button = GetComponent<Button>();
        tName = GetComponentInChildren<TMP_Text>();
    }

    public void SetSkill(Skill skill)
    {
        button.onClick.RemoveAllListeners();
        this.skill = skill;
        tName.text = skill.name;
        button.onClick.AddListener(delegate { WindowSkill.Instance.CreateDetailedWindow(skill); });
    }
}