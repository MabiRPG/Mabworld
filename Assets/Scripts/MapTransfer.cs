using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapTransfer : MonoBehaviour, IMouseInputHandler
{
    [SerializeField]
    private string targetSceneName;
    public int targetPointID;

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameManager.Instance.ChangeScene(targetSceneName, targetPointID);
        }
    }
}