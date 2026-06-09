// Assets/Scripts/Dependencies/DependencySource.cs
// MonoBehaviour base for sources that don't already inherit from something else.
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class DependencySource<T> : MonoBehaviour, IDependencySource<T>
{
    private T _value;

    public T Value
    {
        get => _value;
        protected set
        {
            if (EqualityComparer<T>.Default.Equals(_value, value)) return;
            _value = value;
            Changed?.Invoke(_value);
        }
    }

    public event Action<T> Changed;
}