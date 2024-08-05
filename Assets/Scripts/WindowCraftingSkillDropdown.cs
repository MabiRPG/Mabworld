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

    public void PopulateOptions(List<string> options)
    {
        dropdown.ClearOptions();
        options.Sort();
        dropdown.AddOptions(options);
    }

    public string GetCurrentOption()
    {
        return dropdown.captionText.text;
    }
}