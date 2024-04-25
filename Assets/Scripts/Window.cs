using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VisualScripting;

/// <summary>
///     This super class handles all basic window UI processing. Uses the Window prefab in unity.
/// </summary>
public class Window : MonoBehaviour, IDragHandler, IPointerDownHandler
{

    // GameObject references to header and body content in window prefab.
    protected GameObject header;
    protected GameObject body;

    private Canvas canvas;
    private RectTransform rectTransform;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    protected virtual void Awake()
    {
        header = transform.Find("Header").gameObject;
        body = transform.Find("Body").gameObject;
        canvas = transform.parent.GetComponent<Canvas>();
        rectTransform = GetComponent<RectTransform>();

        // Sets up on click listeners for header buttons.
        Button minimizeButton = header.transform.Find("Minimize Button").GetComponent<Button>();
        minimizeButton.onClick.AddListener(MinimizeWindow);

        Button maximizeButton = header.transform.Find("Maximize Button").GetComponent<Button>();
        maximizeButton.onClick.AddListener(MaximizeWindow);

        Button closeButton = header.transform.Find("Close Button").GetComponent<Button>();
        closeButton.onClick.AddListener(CloseWindow);
    }

    /// <summary>
    ///     Destroys and clears supplied prefab list.
    /// </summary>
    /// <param name="lst">List of prefab GameObjects.</param>
    protected void ClearPrefabs(List<GameObject> lst)
    {
        // Destroys all objects
        foreach (GameObject obj in lst)
        {
            Destroy(obj);
        }

        // Clears the list for update
        lst.Clear();
    }

    /// <summary>
    ///     Capitalizes every word in string for formatting.
    /// </summary>
    /// <param name="x">String to be formatted.</param>
    /// <returns>Formatted string.</returns>
    protected string ToCapitalize(string x)
    {
        return Regex.Replace(x, @"\b([a-z])", m => m.Value.ToUpper());
    }

    /// <summary>
    ///     OnDrag interface implementation to move window.
    /// </summary>
    /// <param name="pointerData">Event payload associated with pointer (mouse / touch) events.</param>
    public void OnDrag(PointerEventData pointerData)
    {
        rectTransform.anchoredPosition += pointerData.delta / canvas.scaleFactor;
    }

    /// <summary>
    ///     OnPointerDown interface implementation to set as focus when clicked.
    /// </summary>
    /// <param name="pointerData">Event payload associated with pointer (mouse / touch) events.</param>
    public void OnPointerDown(PointerEventData pointerData)
    {
        rectTransform.SetAsLastSibling();
    }

    /// <summary>
    ///     Sets the title of the window.
    /// </summary>
    /// <param name="name">New window name.</param>
    protected void SetTitle(string name)
    {
        TMP_Text windowName = header.transform.Find("Title").GetComponent<TMP_Text>();
        windowName.text = name;      
    } 

    /// <summary>
    ///     Minimizes the window.
    /// </summary>
    protected void MinimizeWindow()
    {
        // Hides the body
        body.SetActive(false);
        // Hides the minimize button and disables clicking.
        CanvasGroup minimizeCanvas = header.transform.Find("Minimize Button").GetComponent<CanvasGroup>();
        minimizeCanvas.alpha = 0;
        minimizeCanvas.blocksRaycasts = false;
        // Shows the maximize button and allows clicking.
        CanvasGroup maximizeCanvas = header.transform.Find("Maximize Button").GetComponent<CanvasGroup>();
        maximizeCanvas.alpha = 1;
        maximizeCanvas.blocksRaycasts = true;
    }

    /// <summary>
    ///     Maximizes the window.
    /// </summary>
    protected void MaximizeWindow()
    {
        // Shows the body
        body.SetActive(true);
        // Shows the minimize button
        CanvasGroup minimizeCanvas = header.transform.Find("Minimize Button").GetComponent<CanvasGroup>();
        minimizeCanvas.alpha = 1;
        minimizeCanvas.blocksRaycasts = true;
        // Hides the maximize button
        CanvasGroup maximizeCanvas = header.transform.Find("Maximize Button").GetComponent<CanvasGroup>();
        maximizeCanvas.alpha = 0;
        maximizeCanvas.blocksRaycasts = false;
    }

    /// <summary>
    ///     Closes (hides) the window.
    /// </summary>
    protected void CloseWindow()
    {
        gameObject.SetActive(false);
    }
}