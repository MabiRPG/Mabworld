using System;

public class ValueManager
{
    public event Action OnChange;
    protected float _value;
    public float Value
    {
        get {return _value;}
        set {
            _value = value;
            RaiseOnChange();
        }
    }
    public int ValueInt
    {
        get {return (int)_value;}
    }

    public ValueManager(float Value = 0)
    {
        this.Value = Value;
    }

    public void RaiseOnChange()
    {
        OnChange?.Invoke();
    }

    public void Clear()
    {
        OnChange = null;
    }

}

public class StatManager : ValueManager
{
    public event Action OnMaximumValueChange;
    public event Action OnBaseMaximumValueChange;
    protected float _maximum;
    public float Maximum
    {
        get {return _maximum;}
        set {
            _maximum = value;
            RaiseOnMaximumValueChange();
        }
    }
    protected float _baseMaximum;
    public float BaseMaximum
    {
        get {return _baseMaximum;}
        set {
            _baseMaximum = value;
            RaiseOnBaseMaximumValueChange();
        }
    }

    public StatManager(float Value = 0, float Maximum = 0, float BaseMaximum = 0) : base(Value)
    {
        this.Value = Value;
        this.Maximum = Maximum;
        this.BaseMaximum = BaseMaximum;
    }

    public void RaiseOnMaximumValueChange()
    {
        OnMaximumValueChange?.Invoke();
    }

    public void RaiseOnBaseMaximumValueChange()
    {
        OnBaseMaximumValueChange?.Invoke();
    }
}