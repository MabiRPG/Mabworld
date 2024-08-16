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
        input.onSelect.AddListener(delegate { InputController.Instance.SetBlockKeyboard(true); });
        input.onDeselect.AddListener(delegate { InputController.Instance.SetBlockKeyboard(false); });
    }

    private void OnDisable()
    {
        input.onValueChanged.RemoveAllListeners();
        input.onSelect.RemoveAllListeners();
        input.onDeselect.RemoveAllListeners();
    }

    public void SetSearchAction(Action<string> onChangeAction)
    {
        this.onChangeAction = onChangeAction;
    }
}
