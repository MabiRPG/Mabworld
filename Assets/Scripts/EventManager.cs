using System;

public class ValueEventManager
{
    public event Action OnValueChange;

    public void RaiseOnValueChange()
    {
        OnValueChange?.Invoke();
    }
}

public class StatEventManager : ValueEventManager
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