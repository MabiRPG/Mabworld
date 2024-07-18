using System;
using System.Collections;
using UnityEngine;

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

public abstract class StateMachine : MonoBehaviour
{
    public State State;
    public abstract State DefaultState { get; }
    public IEnumerator Task;

    public bool SetState(State state)
    {
        if (!Free())
        {
            return false;
        }

        Task = state.Run();
        StartCoroutine(Task);
        return true;
    }

    public void Interrupt()
    {
        if (!Free())
        {
            StopCoroutine(Task);
            Task = null;
        }
    }

    public bool Free()
    {
        return Task == null;
    }

    public virtual void Reset()
    {
        Interrupt();
        SetState(DefaultState);
    }
}

