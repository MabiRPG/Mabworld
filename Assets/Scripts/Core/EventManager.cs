using System;
using Newtonsoft.Json;
using UnityEngine;

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

[JsonObject]
public class IntManager : EventManager
{
    [JsonProperty]
    private int _value;

    [JsonIgnore]
    public int Value
    {
        get { return _value; }
        set { _value = value; RaiseOnChange(); }
    }

    public IntManager(int Value = 0)
    {
        this.Value = Value;
    }

    public void SetValueWithoutNotify(int value)
    {
        _value = value;
    }
}

[JsonObject]
public class FloatManager : EventManager
{
    [JsonProperty]
    private float _value;

    [JsonIgnore]
    public float Value
    {
        get { return _value; }
        set { _value = value; RaiseOnChange(); }
    }

    public FloatManager(float Value = 0f)
    {
        this.Value = Value;
    }
}

[JsonObject]
public class StringManager : EventManager
{
    [JsonProperty]
    private string _value;

    [JsonIgnore]
    public string Value
    {
        get { return _value; }
        set { _value = value; RaiseOnChange(); }
    }

    public StringManager(string Value = "")
    {
        this.Value = Value;
    }
}

[JsonObject]
public class BoolManager : EventManager
{
    [JsonProperty]
    private bool _value;

    [JsonIgnore]
    public bool Value
    {
        get { return _value; }
        set { _value = value; RaiseOnChange(); }
    }

    public BoolManager(bool Value = false)
    {
        this.Value = Value;
    }
}

/// <summary>
///     Handles all triple float (actor Stats) and event management.
/// </summary>
[JsonObject]
public class StatManager : FloatManager
{
    // Event handler objects
    public event Action OnMaximumValueChange;
    public event Action OnBaseMaximumValueChange;

    // Current maximum value of stat (modified by buffs/debuffs, etc)
    [JsonProperty]
    private float _maximum;

    [JsonIgnore]
    public float Maximum
    {
        get { return _maximum; }
        set
        {
            _maximum = value;
            RaiseOnMaximumValueChange();
        }
    }

    // Permanent base maximum of stat (calculated from skills, etc).
    [JsonProperty]
    private float _baseMaximum;

    [JsonIgnore]
    public float BaseMaximum
    {
        get { return _baseMaximum; }
        set
        {
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