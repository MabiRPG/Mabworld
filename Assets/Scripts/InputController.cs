using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
///     Handles the input control scheme for the player, aside from movement.
/// </summary>
public class InputController : MonoBehaviour
{
    // Global instance of InputController
    public static InputController Instance { get; private set; }
    // Dictionary of all button key binds
    public Dictionary<KeyCode, Action> buttonKeybinds = new Dictionary<KeyCode, Action>();

    private Vector2 prevMousePosition;
    public Vector2 mouseDelta;
    private bool blockMouse = false;
    private bool blockKeyboard = false;

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

        prevMousePosition = Input.mousePosition;
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
        List<RaycastResult> graphicHits = GraphicsRaycast(Input.mousePosition);
        RaycastHit2D sceneHits = SceneRaycast(Input.mousePosition);

        mouseDelta = (Vector2)Input.mousePosition - prevMousePosition;

        if (!blockMouse)
        {
            HandleMouseInput(graphicHits, sceneHits);
        }

        if (!blockKeyboard)
        {
            HandleKeyboardInput(graphicHits, sceneHits);
        }

        prevMousePosition = Input.mousePosition;
    }

    public void SetBlockMouse(bool state)
    {
        blockMouse = state;
    }

    public void SetBlockKeyboard(bool state)
    {
        blockKeyboard = state;
    }

    private void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
        {
            // If the user has hit any UI windows...
            if (WindowManager.Instance.GetWindowHit(graphicHits, out _))
            {
                WindowManager.Instance.HandleMouseInput(graphicHits, sceneHits);
            }
            // If the user has hit any scene objects...
            else if (sceneHits.transform != null)
            {
                WindowManager.Instance.SetActive(null);
                GameObject obj = sceneHits.transform.gameObject;
                IInputHandler[] handlers = obj.GetComponents<IInputHandler>();

                foreach (IInputHandler handler in handlers)
                {
                    handler.HandleMouseInput(graphicHits, sceneHits);
                }
            }
            // Otherwise, default to moving the player
            else
            {
                WindowManager.Instance.SetActive(null);
                Player.Instance.HandleMouseInput(graphicHits, sceneHits);
            }
        }
    }

    private void HandleKeyboardInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Player.Instance.controller.Task != null)
            {
                Player.Instance.controller.Interrupt();
            }
            else if (WindowManager.Instance.AnyWindowsOpen())
            {
                WindowManager.Instance.HandleKeyboardInput(graphicHits, sceneHits);
            }
            else
            {
                Debug.Log("hit last escape");
            }
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

    private List<RaycastResult> GraphicsRaycast(Vector2 position)
    {
        // Stores all the results of our raycasts
        List<RaycastResult> hits = new List<RaycastResult>();
        // Create a new pointer data for our raycast manipulation
        PointerEventData pointerData = new PointerEventData(GetComponent<EventSystem>());
        pointerData.position = position;
        GameManager.Instance.raycaster.Raycast(pointerData, hits);
        return hits;
    }

    private RaycastHit2D SceneRaycast(Vector2 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit2D hits = Physics2D.GetRayIntersection(ray);
        return hits;
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
        AddButtonBind(KeyCode.M, () => Player.Instance.ToggleMap());
    }
}
