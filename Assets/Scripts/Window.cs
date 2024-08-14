using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
///     This super class handles all basic window UI processing. Uses the Window prefab in unity.
/// </summary>
public class Window : MonoBehaviour, IDragHandler
{
    // GameObject references to header and body content in window prefab.
    public GameObject header;
    public GameObject body;

    private TMP_Text title;
    private Button minimizeButton;
    private CanvasGroup minimizeCanvas;
    private Button maximizeButton;
    private CanvasGroup maximizeCanvas;
    private Button closeButton;

    public RectTransform rectTransform;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    protected virtual void Awake()
    {
        header = transform.Find("Header").gameObject;
        body = transform.Find("Body").gameObject;
        rectTransform = GetComponent<RectTransform>();

        title = header.GetComponentInChildren<TMP_Text>();
        // Sets up on click listeners for header buttons.
        minimizeButton = header.transform.Find("Minimize Button").GetComponent<Button>();
        minimizeCanvas = header.transform.Find("Minimize Button").GetComponent<CanvasGroup>();
        maximizeButton = header.transform.Find("Maximize Button").GetComponent<Button>();
        maximizeCanvas = header.transform.Find("Maximize Button").GetComponent<CanvasGroup>();
        closeButton = header.transform.Find("Close Button").GetComponent<Button>();

        // BUG: Not in an OnEnable for some weird bug reason that causes it to not execute
        // properly...
        minimizeButton.onClick.AddListener(MinimizeWindow);
        maximizeButton.onClick.AddListener(MaximizeWindow);
        closeButton.onClick.AddListener(HideWindow);

        GameManager.Instance.windowManager.AddWindow(this);
    }

    /// <summary>
    ///     OnDrag interface implementation to move window.
    /// </summary>
    /// <param name="pointerData">Event payload associated with pointer (mouse / touch) events.</param>
    public virtual void OnDrag(PointerEventData pointerData)
    {
        // if (pointerData.pointerEnter == header)
        // {
        //     rectTransform.anchoredPosition += pointerData.delta / GameManager.Instance.canvas.scaleFactor;
        // }
    }

    /// <summary>
    ///     Sets the title of the window.
    /// </summary>
    /// <param name="name">New window name.</param>
    protected void SetTitle(string name)
    {
        title.text = name;      
    } 

    /// <summary>
    ///     Minimizes the window.
    /// </summary>
    protected void MinimizeWindow()
    {
        // Hides the body
        body.SetActive(false);
        // Hides the minimize button and disables clicking.
        minimizeCanvas.alpha = 0;
        minimizeCanvas.blocksRaycasts = false;
        // Shows the maximize button and allows clicking.
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
        minimizeCanvas.alpha = 1;
        minimizeCanvas.blocksRaycasts = true;
        // Hides the maximize button
        maximizeCanvas.alpha = 0;
        maximizeCanvas.blocksRaycasts = false;
    }

    /// <summary>
    ///     Brings the window to focus.
    /// </summary>
    public void Focus()
    {
        rectTransform.SetAsLastSibling();
    }

    /// <summary>
    ///     Shows the window.
    /// </summary>
    public void ShowWindow()
    {
        gameObject.SetActive(true);
        Focus();
    }

    /// <summary>
    ///     Closes (hides) the window.
    /// </summary>
    public void HideWindow()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    ///     Toggles visibility of the object.
    /// </summary>
    public virtual void ToggleVisible()
    {
        gameObject.SetActive(!gameObject.activeSelf);
        Focus();
    }
}