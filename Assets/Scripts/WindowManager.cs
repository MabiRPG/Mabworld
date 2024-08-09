using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WindowManager : MonoBehaviour
{
    private List<Window> windows = new List<Window>();
    private GraphicRaycaster raycaster;

    private Window activeWindow;

    private void Start()
    {
        raycaster = GameManager.Instance.raycaster;
    }

    public void AddWindow(Window window)
    {
        windows.Add(window);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Stores all the results of our raycasts
            List<RaycastResult> hits = new List<RaycastResult>();
            // Create a new pointer data for our raycast manipulation
            PointerEventData pointerData = new PointerEventData(GetComponent<EventSystem>());
            pointerData.position = Input.mousePosition;
            raycaster.Raycast(pointerData, hits);

            foreach (RaycastResult hit in hits)
            {
                if (hit.gameObject.TryGetComponent(out Window window))
                {
                    activeWindow = window;
                    window.Focus();
                    break;
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
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