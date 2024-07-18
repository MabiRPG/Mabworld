using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class IdleState : State
{
    private StateMachine machine;

    public IdleState(StateMachine machine)
    {
        this.machine = machine;
    }

    public override void OnEnter()
    {
        machine.State = this;
    }

    public override IEnumerator Main()
    {
        yield return null;
    }

    public override void OnExit()
    {
        machine.Task = null;
    }
}

public class MoveState : State
{
    private MovementStateMachine machine;
    private Actor actor;

    public MoveState(MovementStateMachine machine)
    {
        this.machine = machine;
        actor = machine.actor;
    }

    public override void OnEnter()
    {
        machine.State = this;
    }

    public override IEnumerator Main()
    {
        actor.animator.SetBool("isMoving", true);

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

        actor.animator.SetBool("isMoving", false);
        // Set the final position exactly to the destination, with the z=0 
        // due to some bug that causes it to be non-zero.
        actor.transform.position = new Vector3(
            actor.navMeshAgent.destination.x, 
            actor.navMeshAgent.destination.y, 
            0f
        );
    }

    public override void OnExit()
    {
        machine.Task = null;
        machine.SetState(machine.DefaultState);
    }
}

public abstract class MovementStateMachine : StateMachine
{
    public Actor actor;

    public IdleState idleState;
    public override State DefaultState { get => idleState; }

    public void SetActor(Actor actor)
    {
        this.actor = actor;
        idleState = new IdleState(this);
        SetState(DefaultState);
    }
    
    public void PathToPosition(Vector3 position)
    {
        PathToPosition(new Vector2(position.x, position.y));
    }

    public void PathToPosition(Vector2 position)
    {
        NavMeshPath path = new NavMeshPath();
        actor.navMeshAgent.CalculatePath(position, path);
        actor.navMeshAgent.SetPath(path);
    }
}