using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovementMachine : MovementStateMachine
{
    public void Move()
    {
        if (!GameManager.Instance.EmptyAt())
        {
            return;
        }

        if (LeftClick())
        {
            PathToCursor();
            SetState(new MoveState(this));
        }
    }

    private bool LeftClick()
    {
        return Input.GetMouseButton(0);
    }

    /// <summary>
    ///     Sets the NavMeshAgent destination to the mouse cursor.
    /// </summary>
    private void PathToCursor()
    {
        PathToPosition(GameManager.Instance.canvas.worldCamera.ScreenToWorldPoint(Input.mousePosition));
    }
}

public class PlayerSkillMachine : SkillStateMachine
{
    public override void Reset()
    {
        Interrupt();
        SetState(new SkillCancelState(this));
    }
}

public class PlayerController : MonoBehaviour
{
    public PlayerMovementMachine movementMachine;
    public PlayerSkillMachine skillMachine;
    public IEnumerator Task;

    public void Init(Player player)
    {
        movementMachine = gameObject.AddComponent<PlayerMovementMachine>();
        movementMachine.SetActor(player);

        skillMachine = gameObject.AddComponent<PlayerSkillMachine>();
        skillMachine.SetActor(player);
    }

    private void Update()
    {
        if (Task != null)
        {
            return;
        }

        movementMachine.Move();
    }

    public bool SetTask(IEnumerator task)
    {
        if (Task != null)
        {
            return false;
        }

        Task = task;
        StartCoroutine(Task);
        return true;
    }

    public void Interrupt()
    {
        if (Task != null)
        {
            movementMachine.Reset();
            skillMachine.Reset();
            StopCoroutine(Task);
            Task = null;
        }
    }

    public IEnumerator HarvestResource(Vector3 position, Skill skill, ResultHandler handler)
    {
        NavMesh.SamplePosition(position, out NavMeshHit hit, 5, NavMesh.AllAreas);
        MoveState moveState = new MoveState(movementMachine);
        SkillLoadState loadState = new SkillLoadState(skillMachine, skill);
        moveState.exitAction += () =>
        {
            skillMachine.SetResultHandler(handler);
            skillMachine.SetState(loadState);
        };

        movementMachine.PathToPosition(hit.position);
        movementMachine.SetState(moveState);

        while (movementMachine.State != movementMachine.DefaultState || 
            skillMachine.State != skillMachine.DefaultState)
        {
            yield return null;
        }

        Task = null;
    }
}