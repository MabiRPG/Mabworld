using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WindowCraftingSkillDropdown : MonoBehaviour
{
    private TMP_Dropdown dropdown;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    private void Start()
    {
        dropdown.ClearOptions();
        List<string> names = new List<string>();

        foreach (Skill skill in Player.Instance.skillManager.Skills.Values)
        {
            names.Add(skill.name);
        }

        names.Sort();

        dropdown.AddOptions(names);
    }
}