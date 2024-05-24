using System;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance {get; private set;}

    public Dictionary<KeyCode, Action> movementKeybinds = new Dictionary<KeyCode, Action>();
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

    public void AddMovementBind(KeyCode key, Action action, bool forceReplace = true)
    {
        Add(key, action, forceReplace, movementKeybinds);
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
        else if (Player.Instance.isBusy)
        {
            return;
        }

        foreach (KeyValuePair<KeyCode, Action> pair in buttonKeybinds)
        {
            if (Input.GetKeyDown(pair.Key))
            {
                pair.Value.Invoke();
                break;
            }
        }

        foreach (KeyValuePair<KeyCode, Action> pair in movementKeybinds)
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
        AddMovementBind(KeyCode.W, () => Player.Instance.MoveUp());
        AddMovementBind(KeyCode.A, () => Player.Instance.MoveLeft());
        AddMovementBind(KeyCode.S, () => Player.Instance.MoveDown());
        AddMovementBind(KeyCode.D, () => Player.Instance.MoveRight());

        AddButtonBind(KeyCode.Z, () => WindowSkill.Instance.ToggleVisible());
        AddButtonBind(KeyCode.C, () => WindowCharacter.Instance.ToggleVisible());
        AddButtonBind(KeyCode.I, () => WindowInventory.Instance.ToggleVisible());
        AddButtonBind(KeyCode.M, () => Player.Instance.ToggleMap());
    }
}
