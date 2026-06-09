using System;

public interface IDependencySource<T>
{
    T Value { get; }
    event Action<T> Changed;
}