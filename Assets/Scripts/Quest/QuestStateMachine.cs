using System.Collections;

public class QuestPrerequisiteState : State
{
    private QuestStateMachine machine;

    public QuestPrerequisiteState(QuestStateMachine machine)
    {
        this.machine = machine;
    }

    public override void OnEnter()
    {
        machine.State = this;
    }

    public override IEnumerator Main()
    {
        throw new System.NotImplementedException();
    }

    public override void OnExit()
    {
        throw new System.NotImplementedException();
    }
}

public class QuestInProgressState : State
{
    private QuestStateMachine machine;

    public QuestInProgressState(QuestStateMachine machine)
    {
        this.machine = machine;
    }

    public override void OnEnter()
    {
        machine.State = this;
    }

    public override IEnumerator Main()
    {
        throw new System.NotImplementedException();
    }

    public override void OnExit()
    {
        throw new System.NotImplementedException();
    }
}

public class QuestStateMachine : StateMachine
{
    public QuestPrerequisiteState prerequisiteState;
    public override State DefaultState { get => prerequisiteState; }
}