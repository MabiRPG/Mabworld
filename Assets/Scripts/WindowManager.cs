using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WindowManager : MonoBehaviour
{
    public static WindowManager Instance { get; private set; }

    private List<Window> windows = new List<Window>();
    private GraphicRaycaster raycaster;

    private Window activeWindow;
    private bool isDraggingWindow;
    private Vector2 pos;

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

        raycaster = GameManager.Instance.raycaster;
    }

    public void AddWindow(Window window)
    {
        windows.Add(window);
    }

    // private void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         // Stores all the results of our raycasts
    //         List<RaycastResult> hits = new List<RaycastResult>();
    //         // Create a new pointer data for our raycast manipulation
    //         PointerEventData pointerData = new PointerEventData(GetComponent<EventSystem>());
    //         pointerData.position = Input.mousePosition;
    //         raycaster.Raycast(pointerData, hits);

    //         foreach (RaycastResult hit in hits)
    //         {
    //             if (hit.gameObject.TryGetComponent(out Window window))
    //             {
    //                 activeWindow = window;
    //                 window.Focus();
    //             }
    //         }

    //         // Since raycast results yield an unsorted array, there is no guarantee
    //         // that the active window will be found first. Therefore, iterate twice over array.
    //         // then perform any checks.
    //         foreach (RaycastResult hit in hits)
    //         {
    //             if (activeWindow && hit.gameObject == activeWindow.header)
    //             {
    //                 pos = Input.mousePosition;
    //                 isDraggingWindow = true;
    //             }
    //         }
    //     }
    //     else if (Input.GetMouseButton(0))
    //     {
    //         if (isDraggingWindow)
    //         {
    //             Vector2 delta = (Vector2)Input.mousePosition - pos;
    //             activeWindow.rectTransform.anchoredPosition += delta / GameManager.Instance.canvas.scaleFactor;
    //             pos = Input.mousePosition;
    //         }
    //     }
    //     else if (Input.GetMouseButtonUp(0))
    //     {
    //         isDraggingWindow = false;
    //     }
        
    //     if (Input.GetKeyDown(KeyCode.Escape))
    //     {
    //         if (activeWindow != null)
    //         {
    //             activeWindow.HideWindow();
    //             activeWindow = null;
    //         }
    //         else
    //         {
    //             int lastSiblingIndex = 0;
    //             Window lastWindow = null;

    //             foreach (Window window in windows)
    //             {
    //                 if (window.gameObject.activeSelf && lastSiblingIndex < window.transform.GetSiblingIndex())
    //                 {
    //                     lastSiblingIndex = window.transform.GetSiblingIndex();
    //                     lastWindow = window;
    //                 }
    //             }

    //             if (lastWindow != null)
    //             {
    //                 lastWindow.HideWindow();
    //             }
    //         }
    //     }
    // }

    public bool GetWindowHit(List<RaycastResult> hits, out Window foundWindow)
    {
        foreach (RaycastResult hit in hits)
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
        activeWindow.Focus();
    }

    public void HandleMouseInput(List<RaycastResult> hits, RaycastHit2D sceneHits)
    {
        if (Input.GetMouseButtonDown(0))
        {
            foreach (RaycastResult hit in hits)
            {
                if (hit.gameObject.GetComponent<Selectable>())
                {
                    break;
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
    }
}