using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IInputHandler : IMouseInputHandler, IKeyboardInputHandler { }

public interface IMouseEnterHandler
{
    public void HandleMouseEnter(List<RaycastResult> graphicHits, RaycastHit2D sceneHits);
}

public interface IMouseInputHandler
{
    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits);
}

public interface IMouseExitHandler
{
    public void HandleMouseExit(List<RaycastResult> graphicHits, RaycastHit2D sceneHits);
}

public interface IKeyboardEnterHandler
{
    public void HandleKeyboardEnter(List<RaycastResult> graphicHits, RaycastHit2D sceneHits);
}

public interface IKeyboardInputHandler
{
    public void HandleKeyboardInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits);
}

public interface IKeyboardExitHandler
{
    public void HandleKeyboardExit(List<RaycastResult> graphicHits, RaycastHit2D sceneHits);
}