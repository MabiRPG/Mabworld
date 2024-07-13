using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class State
{
    public abstract void OnEnter();
    public abstract IEnumerator Main();
    public abstract void OnExit();

    public Action enterAction;
    public Action mainAction;
    public Action exitAction;

    public IEnumerator Run()
    {
        OnEnter();
        enterAction?.Invoke();
        
        yield return Main();
        mainAction?.Invoke();

        OnExit();
        exitAction?.Invoke();
    }
}

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

public class SkillLoadState : State
{
    private SkillStateMachine machine;
    private Skill skill;

    public SkillLoadState(SkillStateMachine machine, Skill skill)
    {
        this.machine = machine;
        this.skill = skill;
    }

    public override void OnEnter()
    {
        machine.State = this;
    }

    public override IEnumerator Main()
    {
        yield return machine.bubble.Pulse(skill.icon, skill.GetLoadTime());
    }

    public override void OnExit()
    {
        machine.Task = null;
        machine.SetState(new SkillUseState(machine, skill));
    }
}

public class SkillUseState : State
{
    private SkillStateMachine machine;
    private Skill skill;

    public SkillUseState(SkillStateMachine machine, Skill skill)
    {
        this.machine = machine;
        this.skill = skill;
    }

    public override void OnEnter()
    {
        machine.State = this;
    }

    public override IEnumerator Main()
    {
        yield return skill.Use(machine.handler);
    }

    public override void OnExit()
    {
        machine.bubble.Hide();
        skill.Cooldown(skill.GetCooldownTime());
        machine.Task = null;
        machine.SetState(machine.DefaultState);
    }
}

public class SkillCancelState : State
{
    private SkillStateMachine machine;

    public SkillCancelState(SkillStateMachine machine)
    {
        this.machine = machine;
    }

    public override void OnEnter()
    {
        machine.State = this;
    }

    public override IEnumerator Main()
    {
        yield return machine.bubble.Fade();
    }

    public override void OnExit()
    {
        machine.Task = null;
        machine.SetState(machine.DefaultState);
    }
}

public abstract class StateMachine : MonoBehaviour
{
    public State State;
    public abstract State DefaultState { get; }
    public IEnumerator Task;
    public bool PauseUpdate;

    public bool SetState(State state)
    {
        if (Task != null)
        {
            return false;
        }

        Task = state.Run();
        StartCoroutine(Task);
        return true;
    }

    public void Interrupt()
    {
        if (Task != null)
        {
            StopCoroutine(Task);
            Task = null;
        }
    }

    public void Reset()
    {
        Interrupt();
        SetState(DefaultState);
    }

    public virtual void Update()
    {
        if (PauseUpdate)
        {
            return;
        }
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
}

public class PlayerMovementController : MovementStateMachine
{
    public override void Update()
    {
        if (NotClickable())
        {
            return;
        }

        if (MovingWithLeftClick())
        {
            Reset();
            PathToCursor();
        }

        if (State != DefaultState)
        {
            return;
        }

        if (RequiresPath())
        {
            if (Input.GetMouseButtonDown(0))
            {
                PathToCursor();
            }
            // TODO : Add WASD
        }
        else if (actor.navMeshAgent.hasPath)
        {
            SetState(new MoveState(this));
        }
    }

    private bool NotClickable()
    {
        return !GameManager.Instance.isCanvasEmptyUnderMouse || !GameManager.Instance.isSceneEmptyUnderMouse;
    }

    private bool MovingWithLeftClick()
    {
        return State is MoveState && Input.GetMouseButton(0);
    }

    private bool RequiresPath()
    {
        return !actor.navMeshAgent.pathPending && !actor.navMeshAgent.hasPath;
    }

    /// <summary>
    ///     Sets the NavMeshAgent destination to the mouse cursor.
    /// </summary>
    private void PathToCursor()
    {
        PathToPosition(GameManager.Instance.canvas.worldCamera.ScreenToWorldPoint(Input.mousePosition));
    }

    public void PathToPosition(Vector3 position)
    {
        position.z = 0f;
        actor.navMeshAgent.SetDestination(position);
    }

    public void PathToPosition(Vector2 position)
    {
        actor.navMeshAgent.SetDestination(position);
    }
}

public abstract class SkillStateMachine : StateMachine
{
    public Actor actor;
    public SkillBubble bubble;
    public ResultHandler handler;

    public IdleState idleState;
    public override State DefaultState { get => idleState; }

    public void SetActor(Actor actor)
    {
        this.actor = actor;
        bubble = actor.bubble;

        idleState = new IdleState(this);

        SetState(DefaultState);
    }

    public void SetResultHandler(ResultHandler resultHandler)
    {
        handler = resultHandler;
    }
}

public class PlayerSkillController : SkillStateMachine
{
    public override void Update()
    {
        return;
    }
}