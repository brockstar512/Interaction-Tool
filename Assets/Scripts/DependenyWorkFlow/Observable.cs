// Plain C# observable value. Use directly, or compose into classes
// that can't extend DependencySource<T> (e.g. they already have a base).
using System;
using System.Collections.Generic;

public class Observable<T> : IDependencySource<T>
{
    private T _value;

    public Observable(T initial = default) => _value = initial;

    public T Value
    {
        get => _value;
        set
        {
            if (EqualityComparer<T>.Default.Equals(_value, value)) return;
            _value = value;
            Changed?.Invoke(_value);
        }
    }

    public event Action<T> Changed;
}