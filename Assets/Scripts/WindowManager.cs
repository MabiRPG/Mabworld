using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WindowManager : MonoBehaviour, IInputHandler
{
    public static WindowManager Instance { get; private set; }

    private HashSet<Window> windows = new HashSet<Window>();
    public Window mainWindow;
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

    public void ToggleWindow(Window window)
    {
        if (!window.isActiveAndEnabled || window != mainWindow)
        {
            mainWindow = window;
            mainWindow.ShowWindow();
            mainWindow.Focus();
        }
        else if (window == mainWindow)
        {
            mainWindow.HideWindow();
            mainWindow = FindNextOpenWindow();
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

    private Window FindNextOpenWindow()
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

        return lastWindow;
    }

    public bool MouseHovering()
    {
        return !Input.GetMouseButtonDown(0) && !Input.GetMouseButton(0) && !Input.GetMouseButtonUp(0);
    }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetMouseButtonDown(0))
        {
            GetWindowHit(graphicHits, out mainWindow);
            mainWindow.ShowWindow();
            mainWindow.Focus();

            foreach (RaycastResult hit in graphicHits)
            {
                // This prevents it from calling itself infinitely in stack overflow...
                if (!hit.gameObject.transform.IsChildOf(mainWindow.transform))
                {
                    continue;
                }

                if (hit.gameObject.TryGetComponent(out IInputHandler handler))
                {
                    handler.HandleMouseInput(graphicHits, sceneHits);
                }
                else if (hit.gameObject == mainWindow.header)
                {
                    isDraggingWindow = true;
                    break;
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (isDraggingWindow)
            {
                mainWindow.rectTransform.anchoredPosition += InputController.Instance.mouseDelta
                    / GameManager.Instance.canvas.scaleFactor;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDraggingWindow = false;
        }
        else if (MouseHovering())
        {
            foreach (RaycastResult hit in graphicHits)
            {
                // This prevents it from calling itself infinitely in stack overflow...
                if (!hit.gameObject.transform.IsChildOf(mainWindow.transform))
                {
                    continue;
                }

                if (hit.gameObject.TryGetComponent(out IInputHandler handler))
                {
                    handler.HandleMouseInput(graphicHits, sceneHits);
                }
            }
        }
    }

    public void HandleKeyboardInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (mainWindow != null)
            {
                mainWindow.HideWindow();
            }

            mainWindow = FindNextOpenWindow();
        }
    }
}