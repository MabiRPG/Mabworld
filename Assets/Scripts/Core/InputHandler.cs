using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IInputHandler
{
    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits);
    public void HandleKeyboardInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits);
}