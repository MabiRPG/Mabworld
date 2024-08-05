using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WindowCraftingSkillDropdown : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    private Action<string> onChangeAction;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    private void OnEnable()
    {
        dropdown.onValueChanged.AddListener(delegate { onChangeAction?.Invoke(GetCurrentOption()); });
    }

    private void OnDisable()
    {
        dropdown.onValueChanged.RemoveAllListeners();
    }

    public void PopulateOptions(List<string> options, Action<string> onChangeAction)
    {
        this.onChangeAction = onChangeAction;

        dropdown.ClearOptions();
        options.Sort();
        dropdown.AddOptions(options);

        dropdown.value = 0;
        onChangeAction?.Invoke(GetCurrentOption());
    }

    public string GetCurrentOption()
    {
        return dropdown.captionText.text;
    }
}