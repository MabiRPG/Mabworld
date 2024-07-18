using System.Collections;
using UnityEngine;

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