using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
///     Default idle state for movement machine. Does nothing.
/// </summary>
public class IdleState : State
{
    private MovementStateMachine machine;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    public IdleState(MovementStateMachine machine)
    {
        this.machine = machine;
    }

    /// <summary>
    ///     Called when machine enters state.
    /// </summary>
    public override void OnEnter()
    {
        machine.State = this;
    }

    /// <summary>
    ///     Called when machine processes state.
    /// </summary>
    /// <returns>Coroutine to be run.</returns>
    public override IEnumerator Main()
    {
        // Does nothing in idle state
        yield return null;
    }

    /// <summary>
    ///     Called when machine exits state.
    /// </summary>
    public override void OnExit()
    {
        machine.Task = null;
    }
}

/// <summary>
///     Handles whenever the actor is moving.
/// </summary>
public class MoveState : State
{
    private MovementStateMachine machine;
    private Actor actor;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    public MoveState(MovementStateMachine machine)
    {
        this.machine = machine;
        actor = machine.actor;
    }

    /// <summary>
    ///     Called when machine enters state.
    /// </summary>
    public override void OnEnter()
    {
        machine.State = this;
        actor.animator.SetBool("isMoving", true);
    }

    /// <summary>
    ///     Called when machine processes state.
    /// </summary>
    /// <returns>Coroutine to be run.</returns>
    public override IEnumerator Main()
    {
        while (actor.navMeshAgent.hasPath)
        {
            Vector2 nextPos = actor.navMeshAgent.nextPosition;
            Vector2 diff = nextPos - (Vector2)actor.transform.position;
            actor.transform.position = nextPos;
            // Set the animator to the relative movement vector
            actor.animator.SetFloat("moveX", diff.x);
            actor.animator.SetFloat("moveY", diff.y);

            yield return null;            
        }
    }

    /// <summary>
    ///     Called when machine exits state.
    /// </summary>
    public override void OnExit()
    {
        actor.animator.SetBool("isMoving", false);
        // Set the final position exactly to the destination, with the z=0 
        // due to some bug that causes it to be non-zero.
        actor.transform.position = new Vector3(
            actor.navMeshAgent.destination.x, 
            actor.navMeshAgent.destination.y, 
            0f
        );

        machine.Task = null;
        machine.SetState(machine.DefaultState);
    }
}

/// <summary>
///     Actor movement state machine abstract class to derive machines from.
/// </summary>
public abstract class MovementStateMachine : StateMachine
{
    public Actor actor;

    public IdleState idleState;
    public override State DefaultState { get => idleState; }

    /// <summary>
    ///     Sets the actor for the machine to control.
    /// </summary>
    /// <param name="actor"></param>
    public void SetActor(Actor actor)
    {
        this.actor = actor;
        idleState = new IdleState(this);
        SetState(DefaultState);
    }
    
    /// <summary>
    ///     Calculates a path for the actor.
    /// </summary>
    /// <param name="position"></param>
    public void PathToPosition(Vector3 position)
    {
        PathToPosition(new Vector2(position.x, position.y));
    }

    /// <summary>
    ///     Calculates a path for the actor.
    /// </summary>
    /// <param name="position"></param>
    public void PathToPosition(Vector2 position)
    {
        actor.navMeshAgent.SetDestination(position);
    }
}