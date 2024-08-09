using System.Collections.Generic;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
    private Window[] windows;

    public void Start()
    {
        windows = (Window[])FindObjectsOfType(typeof(Window));
    }
}