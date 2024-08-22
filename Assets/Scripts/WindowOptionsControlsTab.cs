using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowOptionsControlsTab : MonoBehaviour
{
    [SerializeField]
    private GameObject inputRowPrefab;
    public PrefabFactory inputRowPrefabFactory;

    private void Awake()
    {
        inputRowPrefabFactory = ScriptableObject.CreateInstance<PrefabFactory>();
        inputRowPrefabFactory.SetPrefab(inputRowPrefab);
    }

    private void Start()
    {
        foreach (KeyValuePair<KeyCode, InputSettings> pair in InputController.Instance.buttonKeybinds)
        {
            GameObject obj = inputRowPrefabFactory.GetFree(pair.Key, transform);
            WindowOptionsControlsInputRow script = obj.GetComponent<WindowOptionsControlsInputRow>();
            script.SetKey(pair.Key, pair.Value);
        }
    }
}