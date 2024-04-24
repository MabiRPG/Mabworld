using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Window : MonoBehaviour {
    protected GameObject header;
    protected GameObject body;

    protected virtual void Awake()
    {
        header = transform.Find("Header").gameObject;
        body = transform.Find("Body").gameObject;

        Button minimizeButton = header.transform.Find("Minimize Button").GetComponent<Button>();
        minimizeButton.onClick.AddListener(MinimizeWindow);

        Button maximizeButton = header.transform.Find("Maximize Button").GetComponent<Button>();
        maximizeButton.onClick.AddListener(MaximizeWindow);

        Button closeButton = header.transform.Find("Close Button").GetComponent<Button>();
        closeButton.onClick.AddListener(CloseWindow);
    }

    protected void ChangeTitle(string name)
    {
        TMP_Text windowName = header.transform.Find("Title").GetComponent<TMP_Text>();
        windowName.text = name;      
    } 

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

    protected string ToCapitalize(string x)
    {
        return Regex.Replace(x, @"\b([a-z])", m => m.Value.ToUpper());
    }

    protected void MinimizeWindow()
    {
        body.SetActive(false);
        CanvasGroup minimizeCanvas = header.transform.Find("Minimize Button").GetComponent<CanvasGroup>();
        minimizeCanvas.alpha = 0;
        minimizeCanvas.interactable = false;
        minimizeCanvas.blocksRaycasts = false;
        CanvasGroup maximizeCanvas = header.transform.Find("Maximize Button").GetComponent<CanvasGroup>();
        maximizeCanvas.alpha = 1;
        maximizeCanvas.interactable = true;
        maximizeCanvas.blocksRaycasts = true;
    }

    protected void MaximizeWindow()
    {
        body.SetActive(true);
        CanvasGroup minimizeCanvas = header.transform.Find("Minimize Button").GetComponent<CanvasGroup>();
        minimizeCanvas.alpha = 1;
        minimizeCanvas.interactable = true;
        minimizeCanvas.blocksRaycasts = true;
        CanvasGroup maximizeCanvas = header.transform.Find("Maximize Button").GetComponent<CanvasGroup>();
        maximizeCanvas.alpha = 0;
        maximizeCanvas.interactable = false;
        maximizeCanvas.blocksRaycasts = false;
    }

    protected void CloseWindow()
    {
        gameObject.SetActive(false);
    }
}