using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NPC : MonoBehaviour, IInputHandler
{
    private NPCModel model;

    private void Awake()
    {
        string name = gameObject.name;

        foreach (NPCModel model in GameManager.Instance.Database.npcModels)
        {
            if (name.StartsWith(model.name))
            {
                this.model = model;
                break;
            }
        }

        if (model == null)
        {
            Destroy(this);
        }
    }

    public void HandleKeyboardInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        // throw new System.NotImplementedException();
    }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetMouseButtonDown(0))
        {
            ActionMoveController action = new ActionMoveController(Player.Instance, this,
                transform.TransformPoint(Vector3.zero));
            action.OnSuccess += () =>
            {
                ActionNPCInteractController npcAction = new ActionNPCInteractController(
                    Player.Instance,
                    this,
                    model.ID,
                    transform.TransformPoint(Vector3.zero)
                );
                npcAction.Handle();
            };
            action.Handle();
        }
    }
}
