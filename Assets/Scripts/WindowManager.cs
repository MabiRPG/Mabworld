using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WindowManager : MonoBehaviour
{
    private List<Window> windows = new List<Window>();
    private GraphicRaycaster raycaster;
    private RectTransform canvasTransform;
    private Camera canvasCamera;

    private Window activeWindow;
    private bool isDraggingWindow;

    private void Awake()
    {
        raycaster = GameManager.Instance.raycaster;
        canvasTransform = GameManager.Instance.canvas.GetComponent<RectTransform>();
        canvasCamera = GameManager.Instance.canvas.GetComponent<Canvas>().worldCamera;
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
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;
            raycaster.Raycast(pointerData, hits);

            foreach (RaycastResult hit in hits)
            {
                if (hit.gameObject.TryGetComponent(out Window window))
                {
                    activeWindow = window;
                    window.Focus();
                }

                if (activeWindow && hit.gameObject == activeWindow.header)
                {
                    isDraggingWindow = true;
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (isDraggingWindow)
            {

                // activeWindow.rectTransform.localPosition = Input.mousePositionDelta / GameManager.Instance.canvas.scaleFactor;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDraggingWindow = false;
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