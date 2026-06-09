using System;
using System.Collections.Generic;

public interface IDependencySource<T>
{
    T Value { get; }
    event Action<T> Apply;
    
}
