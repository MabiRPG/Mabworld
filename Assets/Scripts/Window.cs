using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Window : MonoBehaviour {
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
}