using System;

public class EventManager
{
    public event Action OnChange;

    public void RaiseOnChange()
    {
        OnChange?.Invoke();
    }

    public void Clear()
    {
        OnChange = null;
    }
}

public class StatEventManager : EventManager
{
    public event Action OnMaximumValueChange;
    public event Action OnBaseMaximumValueChange;

    public void RaiseOnMaximumValueChange()
    {
        OnMaximumValueChange?.Invoke();
    }

    public void RaiseOnBaseMaximumValueChange()
    {
        OnBaseMaximumValueChange?.Invoke();
    }
}