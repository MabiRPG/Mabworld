using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance {get; private set;}

    public Dictionary<KeyCode, Action> keyBinds = new Dictionary<KeyCode, Action>();
    public Dictionary<KeyCode, Action> buttonBinds = new Dictionary<KeyCode, Action>();

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

    public void AddKey(KeyCode key, Action action, bool forceReplace = true)
    {
        Add(key, action, forceReplace, keyBinds);
    }

    public void AddButton(KeyCode key, Action action, bool forceReplace = true)
    {
        Add(key, action, forceReplace, buttonBinds);
    }

    public void Add(KeyCode key, Action action, bool forceReplace, Dictionary<KeyCode, Action> dict)
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

    private void Update()
    {
        if (!Input.anyKey && !Input.anyKeyDown)
        {
            return;
        }

        foreach (KeyValuePair<KeyCode, Action> pair in buttonBinds)
        {
            if (Input.GetKeyDown(pair.Key))
            {
                pair.Value.Invoke();
                break;
            }
        }

        foreach (KeyValuePair<KeyCode, Action> pair in keyBinds)
        {
            if (Input.GetKey(pair.Key))
            {
                pair.Value.Invoke();
                break;
            }
        }
    }

    public void Reset()
    {
        AddKey(KeyCode.W, () => Player.Instance.MoveUp());
        AddKey(KeyCode.A, () => Player.Instance.MoveLeft());
        AddKey(KeyCode.S, () => Player.Instance.MoveDown());
        AddKey(KeyCode.D, () => Player.Instance.MoveRight());

        AddButton(KeyCode.Z, () => WindowSkill.Instance.ToggleVisible());
        AddButton(KeyCode.C, () => WindowCharacter.Instance.ToggleVisible());
        AddButton(KeyCode.M, () => Player.Instance.ToggleMap());
    }
}
