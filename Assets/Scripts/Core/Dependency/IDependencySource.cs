using System;

namespace IT.Core.Dependency
{
    public interface IDependencySource<T>
    {
        T Value { get; }
        event Action<T> Changed;
    }
}
