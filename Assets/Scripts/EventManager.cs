using System;

/// <summary>
///     Handles a single float value and event management.
/// </summary>
public class ValueManager
{
    // Event handler object
    public event Action OnChange;
    // Backstored private value
    protected float _value;
    // Publicly exposed property
    public float Value
    {
        get {return _value;}
        set {
            // Assign the value and inform all subscribers
            _value = value;
            RaiseOnChange();
        }
    }
    public int ValueInt
    {
        // Returns a special int cast as necessary (indexer for arrays, etc)
        get {return (int)_value;}
    }

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    /// <param name="Value">Starting value.</param>
    public ValueManager(float Value = 0)
    {
        this.Value = Value;
    }

    /// <summary>
    ///     Invokes all stored methods in delegate.
    /// </summary>
    public void RaiseOnChange()
    {
        OnChange?.Invoke();
    }

    /// <summary>
    ///     Clears all stored methods.
    /// </summary>
    public virtual void Clear()
    {
        OnChange = null;
    }

}

/// <summary>
///     Handles all triple float (actor Stats) and event management.
/// </summary>
public class StatManager : ValueManager
{
    // Event handler objects
    public event Action OnMaximumValueChange;
    public event Action OnBaseMaximumValueChange;
    // Current maximum value of stat (modified by buffs/debuffs, etc)
    protected float _maximum;
    public float Maximum
    {
        get {return _maximum;}
        set {
            _maximum = value;
            RaiseOnMaximumValueChange();
        }
    }
    // Permanent base maximum of stat (calculated from skills, etc).
    protected float _baseMaximum;
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