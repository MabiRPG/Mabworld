using System.Collections;
using System.Collections.Generic;
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
}