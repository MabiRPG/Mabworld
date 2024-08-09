using System;
using TMPro;
using UnityEngine;

public class WindowCraftingItemSearchField : MonoBehaviour
{
    private TMP_InputField input;
    private Action<string> onChangeAction;

    private void Awake()
    {
        input = GetComponent<TMP_InputField>();
    }

    private void OnEnable()
    {
        input.onValueChanged.AddListener(delegate { onChangeAction?.Invoke(input.text); });
    }

    private void OnDisable()
    {
        input.onValueChanged.RemoveAllListeners();
    }

    public void SetSearchAction(Action<string> onChangeAction)
    {
        this.onChangeAction = onChangeAction;
    }
}
