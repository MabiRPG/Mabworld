using System;
using System.Collections;
using UnityEngine;

/// <summary>
///     Base class for all states.
/// </summary>
public abstract class State
{
    // Called before the main execution
    public abstract void OnEnter();
    // Called during main execution
    public abstract IEnumerator Main();
    // Called after main execution
    public abstract void OnExit();

    // These actions are called immediately after their respective functions
    public Action enterAction;
    public Action mainAction;
    public Action exitAction;

    /// <summary>
    ///     Runs the state.
    /// </summary>
    /// <returns>Coroutine to be run.</returns>
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

/// <summary>
///     Base class for all state machines.
/// </summary>
public abstract class StateMachine : MonoBehaviour
{
    public State State;
    public abstract State DefaultState { get; }
    public IEnumerator Task;

    /// <summary>
    ///     Sets the state.
    /// </summary>
    /// <param name="state"></param>
    /// <returns>True if successful, False otherwise</returns>
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

    /// <summary>
    ///     Interrupts the current task.
    /// </summary>
    public void Interrupt()
    {
        if (!Free())
        {
            StopCoroutine(Task);
            Task = null;
        }
    }

    /// <summary>
    ///     Checks if there are any running tasks.
    /// </summary>
    /// <returns></returns>
    public bool Free()
    {
        return Task == null;
    }

    /// <summary>
    ///     Resets the machine to default state.
    /// </summary>
    public virtual void Reset()
    {
        Interrupt();

        if (State != DefaultState)
        {
            SetState(DefaultState);
        }
    }
}

