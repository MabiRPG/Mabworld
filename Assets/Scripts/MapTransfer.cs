using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapTransfer : MonoBehaviour, IMouseInputHandler
{
    [SerializeField]
    private string targetSceneName;
    public int targetPointID;

    public bool canSend;
    public bool canReceive;

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetMouseButtonDown(0) && canSend)
        {
            GameManager.Instance.ChangeScene(targetSceneName, targetPointID);
        }
    }
}