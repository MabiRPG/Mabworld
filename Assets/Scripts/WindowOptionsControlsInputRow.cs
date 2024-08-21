using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WindowOptionsControlsInputRow : MonoBehaviour
{
    private KeyCode key;
    private InputSettings settings;
    private TMP_Text description;
    private TMP_InputField keyInputField1;
    private TMP_InputField keyInputField2;

    private void Awake()
    {
        description = transform.Find("Key Action Text").GetComponent<TMP_Text>();
        keyInputField1 = transform.Find("Key Input Field 1").GetComponent<TMP_InputField>();
        keyInputField2 = transform.Find("Key Input Field 2").GetComponent<TMP_InputField>();
    }

    public void SetKey(KeyCode key, InputSettings settings)
    {
        this.key = key;
        this.settings = settings;
        description.text = settings.name;
        keyInputField1.text = key.ToString();
    }
}