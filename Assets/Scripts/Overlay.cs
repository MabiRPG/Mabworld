using System.Collections.Generic;
using UnityEngine;

public interface IOverlay
{
    public bool isFullscreenFocus { get; set; }

    public void AddOverlayCaller();
    public void RemoveOverlayCaller();
}

public class Overlay : MonoBehaviour
{
    private HashSet<GameObject> overlayCallers = new HashSet<GameObject>();

    public void AddCaller(GameObject caller)
    {
        overlayCallers.Add(caller);
        Draw();
    }

    public void RemoveCaller(GameObject caller)
    {
        overlayCallers.Remove(caller);
        Draw();
    }

    private void Draw()
    {
        gameObject.SetActive(overlayCallers.Count > 0);
        GameManager.Instance.HUD.SetActive(overlayCallers.Count == 0);
    }
}