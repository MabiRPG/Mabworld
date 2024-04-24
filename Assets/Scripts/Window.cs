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

    protected void CloseWindow()
    {
        gameObject.SetActive(false);
    }
}