using System.Collections;
using UnityEngine;

/// <summary>
///     State for loading a skill.
/// </summary>
public class SkillLoadState : State
{
    private SkillStateMachine machine;
    private Skill skill;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    public SkillLoadState(SkillStateMachine machine, Skill skill)
    {
        this.machine = machine;
        this.skill = skill;
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
        yield return machine.bubble.Pulse(skill.icon, skill.GetLoadTime());
    }

    /// <summary>
    ///     Called when machine exits state.
    /// </summary>
    public override void OnExit()
    {
        machine.Task = null;
        // Transition to using the skill immediately.
        machine.SetState(new SkillUseState(machine, skill));
    }
}

/// <summary>
///     State for using the skill.
/// </summary>
public class SkillUseState : State
{
    private SkillStateMachine machine;
    private Skill skill;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    public SkillUseState(SkillStateMachine machine, Skill skill)
    {
        this.machine = machine;
        this.skill = skill;
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
        yield return skill.Use(machine.handler);
        yield return skill.Cooldown(skill.GetCooldownTime());
    }

    /// <summary>
    ///     Called when machine exits state.
    /// </summary>
    public override void OnExit()
    {
        machine.bubble.Hide();
        machine.Task = null;
        machine.SetState(machine.DefaultState);
    }
}

/// <summary>
///     State for cancelling a skill.
/// </summary>
public class SkillCancelState : State
{
    private SkillStateMachine machine;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    public SkillCancelState(SkillStateMachine machine)
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
        yield return machine.bubble.Fade();
    }

    /// <summary>
    ///     Called when machine exits state.
    /// </summary>
    public override void OnExit()
    {
        machine.Task = null;
        machine.SetState(machine.DefaultState);
    }
}

/// <summary>
///     Actor skill state machine abstract class.
/// </summary>
public abstract class SkillStateMachine : StateMachine
{
    public Actor actor;
    public SkillBubble bubble;
    public ResultHandler handler;

    public IdleState idleState;
    public override State DefaultState { get => idleState; }

    /// <summary>
    ///     Sets the actor for the machine to control.
    /// </summary>
    /// <param name="actor"></param>
    public void SetActor(Actor actor)
    {
        this.actor = actor;
        bubble = actor.bubble;

        idleState = new IdleState(this);

        SetState(DefaultState);
    }

    /// <summary>
    ///     Sets the result handler for skill completions.
    /// </summary>
    /// <param name="resultHandler"></param>
    public void SetResultHandler(ResultHandler resultHandler)
    {
        handler = resultHandler;
    }
}