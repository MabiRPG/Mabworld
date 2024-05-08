using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance {get; private set;}

    public Dictionary<KeyCode, Action> keybinds = new Dictionary<KeyCode, Action>();

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

    public void AddKey(KeyCode key, Action action, bool replace)
    {
        if (keybinds.ContainsKey(key))
        {
            Debug.Log(string.Format("Identical keybind found for {0}", key));

            if (!replace)
            {
                return;
            }

            Debug.Log("Replacing old keybind with new.");
        }
        
        keybinds[key] = action;
    }

    public void AddSkillSlot(KeyCode key)
    {

    }

    private void Update()
    {
        if (!Input.anyKey)
        {
            return;
        }

        foreach (KeyValuePair<KeyCode, Action> pair in keybinds)
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
        // keybinds.Add(KeyCode.W, () => Player.Instance.MoveUp());
        // keybinds.Add(KeyCode.A, () => Player.Instance.MoveLeft());
        // keybinds.Add(KeyCode.S, () => Player.Instance.MoveDown());
        // keybinds.Add(KeyCode.D, () => Player.Instance.MoveRight());
        // keybinds.Add(KeyCode.Z, () => WindowSkill.Instance.ToggleVisible());
        // keybinds.Add(KeyCode.M, () => Player.Instance.Map.SetActive(!Player.Instance.Map.activeSelf));
    }
}
