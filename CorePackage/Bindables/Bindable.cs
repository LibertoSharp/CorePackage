using System;
using System.Diagnostics;

public class Bindable<T>
{
    private T _value;
    public event Action<T, T> OnValueChanged;
    private bool _locked = false;

    public T Value
    {
        get => _value;
        set
        {
            if (Equals(_value, value) || _locked) return;
            T oldValue = _value;
            _value = value;
            
            OnValueChanged?.Invoke(oldValue, value);
        }
    }

    public Bindable(T defaultValue = default)
    {
        _value = defaultValue;
    }

    public void Lock(T value)
    {
        Value = value;
        _locked = true;
    }
}