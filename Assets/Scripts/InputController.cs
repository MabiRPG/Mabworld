using System;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public static InputController Instance = null;

    public Dictionary<KeyCode, Action> buttonKeybinds = new Dictionary<KeyCode, Action>();

    private void Awake()
    {
        // Singleton recipe so only one instance is active at a time.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }        

        Reset();
    }

    public void AddButtonBind(KeyCode key, Action action, bool forceReplace = true)
    {
        Add(key, action, forceReplace, buttonKeybinds);
    }

    private void Add(KeyCode key, Action action, bool forceReplace, Dictionary<KeyCode, Action> dict)
    {
        if (dict.ContainsKey(key))
        {
            Debug.Log(string.Format("Identical keybind found for {0}", key));

            if (!forceReplace)
            {
                return;
            }

            Debug.Log("Replacing old keybind with new.");
        }
        
        dict[key] = action;
    }

    public void RemoveButtonBind(KeyCode key)
    {
        buttonKeybinds.Remove(key);
    }

    private void Update()
    {
        if (!Input.anyKey && !Input.anyKeyDown)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Player.Instance.InterruptAction();
        }
        
        foreach (KeyValuePair<KeyCode, Action> pair in buttonKeybinds)
        {
            if (Input.GetKeyDown(pair.Key))
            {
                pair.Value.Invoke();
                break;
            }
        }
    }

    public void Reset()
    {
        AddButtonBind(KeyCode.Z, () => WindowSkill.Instance.ToggleVisible());
        AddButtonBind(KeyCode.C, () => WindowCharacter.Instance.ToggleVisible());
        AddButtonBind(KeyCode.I, () => WindowInventory.Instance.ToggleVisible());
        AddButtonBind(KeyCode.M, () => Player.Instance.ToggleMap());
    }
}
