using System.Collections;

public class QuestInProgressState : State
{
    private QuestStateMachine machine;

    public override void OnEnter()
    {
        throw new System.NotImplementedException();
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
    public override State DefaultState => throw new System.NotImplementedException();
}