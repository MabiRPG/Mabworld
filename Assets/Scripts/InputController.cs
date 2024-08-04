using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Handles the input control scheme for the player, aside from movement.
/// </summary>
public class InputController : MonoBehaviour
{
    // Global instance of InputController
    public static InputController Instance { get; private set; }
    // Dictionary of all button key binds
    public Dictionary<KeyCode, Action> buttonKeybinds = new Dictionary<KeyCode, Action>();

    /// <summary>
    ///     Initializes the object.
    /// </summary>
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

        // Assigns default control scheme
        Reset();
    }

    /// <summary>
    ///     Adds a new button and corresponding action to controller.
    /// </summary>
    /// <param name="key">KeyCode of button</param>
    /// <param name="action">Action to trigger when pressed</param>
    /// <param name="forceReplace">True to replace existing keybind, False otherwise</param>
    public void AddButtonBind(KeyCode key, Action action, bool forceReplace = true)
    {
        Add(key, action, forceReplace, buttonKeybinds);
    }

    /// <summary>
    ///     Adds a new button and action to the input dictionary.
    /// </summary>
    /// <param name="key">KeyCode of button</param>
    /// <param name="action">Action to trigger when pressed</param>
    /// <param name="forceReplace">True to replace existing keybind, False otherwise</param>
    /// <param name="dict">Dictionary of input keys and actions</param>
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

    /// <summary>
    ///     Removes the key from the controller.
    /// </summary>
    /// <param name="key">KeyCode of button</param>
    public void RemoveButtonBind(KeyCode key)
    {
        buttonKeybinds.Remove(key);
    }

    /// <summary>
    ///     Called on every frame.
    /// </summary>
    private void Update()
    {
        if (NoButtonPressed())
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Player.Instance.controller.Interrupt();
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

    /// <summary>
    ///     Checks if any buttons are pressed or held down
    /// </summary>
    /// <returns></returns>
    private bool NoButtonPressed()
    {
        return !Input.anyKey && !Input.anyKeyDown;
    }

    /// <summary>
    ///     Resets the control scheme
    /// </summary>
    public void Reset()
    {
        AddButtonBind(KeyCode.Z, () => WindowSkill.Instance.ToggleVisible());
        AddButtonBind(KeyCode.C, () => WindowCharacter.Instance.ToggleVisible());
        AddButtonBind(KeyCode.I, () => WindowInventory.Instance.ToggleVisible());
        AddButtonBind(KeyCode.F, () => WindowCrafting.Instance.ToggleVisible());
        AddButtonBind(KeyCode.M, () => Player.Instance.ToggleMap());
    }
}
