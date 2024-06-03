using UnityEngine;

public class WindowInventorySlot : MonoBehaviour
{
    GameObject overlay;
    public bool isUsed = false;

    private void Awake()
    {
        overlay = transform.Find("Overlay").gameObject;
    }

    public void SetOverlay(bool state)
    {
        overlay.SetActive(state);
    }
}