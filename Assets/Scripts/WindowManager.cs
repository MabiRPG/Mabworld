using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WindowManager : MonoBehaviour, IInputHandler
{
    public static WindowManager Instance { get; private set; }

    private List<Window> windows = new List<Window>();
    public Window activeWindow;
    private bool isDraggingWindow;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddWindow(Window window)
    {
        windows.Add(window);
    }

    public bool GetWindowHit(List<RaycastResult> graphicHits, out Window foundWindow)
    {
        foreach (RaycastResult hit in graphicHits)
        {
            if (hit.gameObject.TryGetComponent(out Window window))
            {
                foundWindow = window;
                return true;
            }
        }

        foundWindow = null;
        return false;
    }

    public void SetActive(Window window)
    {
        activeWindow = window;

        if (window != null)
        {
            activeWindow.Focus();
        }
    }

    public bool AnyWindowsOpen()
    {
        foreach (Window window in windows)
        {
            if (window.isActiveAndEnabled)
            {
                return true;
            }
        }

        return false;
    }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetMouseButtonDown(0))
        {
            GetWindowHit(graphicHits, out Window foundWindow);
            SetActive(foundWindow);

            foreach (RaycastResult hit in graphicHits)
            {
                // This prevents it from calling itself infinitely in stack overflow...
                if (!hit.gameObject.transform.IsChildOf(activeWindow.transform))
                {
                    continue;
                }

                if (hit.gameObject.TryGetComponent(out IInputHandler handler))
                {
                    handler.HandleMouseInput(graphicHits, sceneHits);
                }
                else if (hit.gameObject == activeWindow.header)
                {
                    isDraggingWindow = true;
                    break;
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (isDraggingWindow)
            {
                activeWindow.rectTransform.anchoredPosition += InputController.Instance.mouseDelta
                    / GameManager.Instance.canvas.scaleFactor;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDraggingWindow = false;
        }

        IInputHandler[] handlers = activeWindow.gameObject.GetComponents<IInputHandler>();

        foreach (IInputHandler handler in handlers)
        {
            handler.HandleMouseInput(graphicHits, sceneHits);
        }
    }

    public void HandleKeyboardInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (activeWindow != null)
            {
                activeWindow.HideWindow();
                activeWindow = null;
            }
            else
            {
                int lastSiblingIndex = 0;
                Window lastWindow = null;

                foreach (Window window in windows)
                {
                    if (window.gameObject.activeSelf && lastSiblingIndex < window.transform.GetSiblingIndex())
                    {
                        lastSiblingIndex = window.transform.GetSiblingIndex();
                        lastWindow = window;
                    }
                }

                if (lastWindow != null)
                {
                    lastWindow.HideWindow();
                }
            }            
        }
    }
}