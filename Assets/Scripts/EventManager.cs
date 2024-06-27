using System;

public class EventManager
{
    public event Action OnChange;

    public void RaiseOnChange()
    {
        OnChange?.Invoke();
    }

    public virtual void Clear()
    {
        OnChange = null;
    }
}

public class IntManager : EventManager
{
    private int _value;
    public int Value
    {
        get {return _value;}
        set {_value = value; RaiseOnChange();}
    }

    public IntManager(int Value = 0)
    {
        this.Value = Value;
    }
}

public class FloatManager : EventManager
{
    private float _value;
    public float Value
    {
        get {return _value;}
        set {_value = value; RaiseOnChange();}
    }

    public FloatManager(float Value = 0f)
    {
        this.Value = Value;
    }
}

public class StringManager : EventManager
{
    private string _value;
    public string Value
    {
        get {return _value;}
        set {_value = value; RaiseOnChange();}
    }

    public StringManager(string Value = "")
    {
        this.Value = Value;
    }
}

public class BoolManager : EventManager
{
    private bool _value;
    public bool Value
    {
        get {return _value;}
        set {_value = value; RaiseOnChange();}
    }

    public BoolManager(bool Value = false)
    {
        this.Value = Value;
    }
}

/// <summary>
///     Handles all triple float (actor Stats) and event management.
/// </summary>
public class StatManager : FloatManager
{
    // Event handler objects
    public event Action OnMaximumValueChange;
    public event Action OnBaseMaximumValueChange;
    // Current maximum value of stat (modified by buffs/debuffs, etc)
    private float _maximum;
    public float Maximum
    {
        get {return _maximum;}
        set {
            _maximum = value;
            RaiseOnMaximumValueChange();
        }
    }
    // Permanent base maximum of stat (calculated from skills, etc).
    private float _baseMaximum;
    public float BaseMaximum
    {
        get {return _baseMaximum;}
        set {
            _baseMaximum = value;
            RaiseOnBaseMaximumValueChange();
        }
    }

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    /// <param name="Value">Starting value.</param>
    /// <param name="Maximum">Starting maximum.</param>
    /// <param name="BaseMaximum">Starting base maximum.</param>
    public StatManager(float Value = 0, float Maximum = 0, float BaseMaximum = 0) : base(Value)
    {
        this.Value = Value;
        this.Maximum = Maximum;
        this.BaseMaximum = BaseMaximum;
    }

    /// <summary>
    ///     Invokes all stored methods in delegate.
    /// </summary>
    public void RaiseOnMaximumValueChange()
    {
        OnMaximumValueChange?.Invoke();
    }

    /// <summary>
    ///     Invokes all stored methods in delegate.
    /// </summary>
    public void RaiseOnBaseMaximumValueChange()
    {
        OnBaseMaximumValueChange?.Invoke();
    }

    /// <summary>
    ///     Clears all stored methods.
    /// </summary>
    public override void Clear()
    {
        base.Clear();
        OnMaximumValueChange = null;
        OnBaseMaximumValueChange = null;
    }
}