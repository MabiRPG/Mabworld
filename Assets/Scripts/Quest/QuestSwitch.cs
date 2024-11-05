using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class QuestSwitch : MonoBehaviour, IInputHandler
{
    public int questID;
    public int stateID;

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (stateID == 0)
            {
                Player.Instance.AddQuest(questID);
            }
            else
            {
                if (Player.Instance.IsQuestStarted(questID))
                {

                }
            }
        }
    }

    public void HandleKeyboardInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        throw new System.NotImplementedException();
    }
}