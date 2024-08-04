using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
///     Player state machine for controlling movement.
/// </summary>
public class PlayerMovementMachine : MovementStateMachine
{
    /// <summary>
    ///     Default movement function to be called on update.
    /// </summary>
    public void Move()
    {
        if (!GameManager.Instance.EmptyAt())
        {
            return;
        }

        if (LeftClick())
        {
            Reset();
            PathToCursor();
        }

        // Debug.Log(actor.navMeshAgent.pathStatus);

        // Since the path calculation is async, we call move state whenever it is done.
        if (actor.navMeshAgent.hasPath)
        {
            if (actor.navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                SetState(new MoveState(this));
            }
            else
            {
                actor.navMeshAgent.ResetPath();
            }
        }
    }

    /// <summary>
    ///     Checks if the left mouse button was pressed.
    /// </summary>
    /// <returns></returns>
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

/// <summary>
///     Player state machine for skills.
/// </summary>
public class PlayerSkillMachine : SkillStateMachine
{
    // Override behaviour to do skill cancelling when cancelled.
    public override void Reset()
    {
        Interrupt();
        SetState(new SkillCancelState(this));
    }
}

/// <summary>
///     Controller for all player state machines 
/// </summary>
public class PlayerController : MonoBehaviour
{
    public PlayerMovementMachine movementMachine;
    public PlayerSkillMachine skillMachine;
    public Player player;
    public IEnumerator Task;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    /// <param name="player"></param>
    public void Init(Player player)
    {
        movementMachine = gameObject.AddComponent<PlayerMovementMachine>();
        movementMachine.SetActor(player);

        skillMachine = gameObject.AddComponent<PlayerSkillMachine>();
        skillMachine.SetActor(player);

        this.player = player;
    }

    /// <summary>
    ///     Called on every frame.
    /// </summary>
    private void Update()
    {
        if (Task != null)
        {
            return;
        }

        movementMachine.Move();
    }

    /// <summary>
    ///     Sets a new task that overrides the default behaviour of the controller.
    /// </summary>
    /// <param name="task">Coroutine to be run.</param>
    /// <returns>True if assigned, False otherwise</returns>
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

    /// <summary>
    ///     Interrupts the current task and resets all machines to their default states.
    /// </summary>
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

    /// <summary>
    ///     Coroutine to harvest a resource. First, move the player to the resource, then
    ///     run the skill load, skill use states.
    /// </summary>
    /// <param name="position">Position of the resource</param>
    /// <param name="skill">Skill to be used</param>
    /// <param name="handler"></param>
    /// <returns>Coroutine to be run.</returns>
    public IEnumerator HarvestResource(Vector3 position, Skill skill, ResultHandler handler)
    {
        SkillLoadState loadState = new SkillLoadState(skillMachine, skill);
        MoveState moveState = new MoveState(movementMachine);
        moveState.exitAction += () =>
        {
            skillMachine.SetResultHandler(handler);
            skillMachine.SetState(loadState);
        };

        NavMesh.SamplePosition(position, out NavMeshHit hit, 5, NavMesh.AllAreas);
        movementMachine.PathToPosition(hit.position);

        while (player.navMeshAgent.pathPending)
        {
            yield return null;
        }

        if (player.navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            movementMachine.SetState(moveState);

            while (movementMachine.State != movementMachine.DefaultState ||
                skillMachine.State != skillMachine.DefaultState)
            {
                yield return null;
            }
        }
        else
        {
            player.navMeshAgent.ResetPath();
        }

        Task = null;
    }
}