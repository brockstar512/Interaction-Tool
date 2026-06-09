// Assets/Scripts/Dependencies/MultiDependent.cs
// MonoBehaviour base for "I depend on a list of sources." Subscribes to
// every source, calls Reevaluate on any change and once on enable.
using System.Collections.Generic;
using UnityEngine;

public abstract class MultiDependent<T> : MonoBehaviour
{
    [SerializeField]
    private List<InterfaceReference<IDependencySource<T>>> sources = new();

    protected virtual void OnEnable()
    {
        foreach (var source in sources)
            if (source?.Value != null) source.Value.Changed += OnAnyChanged;
        Reevaluate();
    }

    protected virtual void OnDisable()
    {
        foreach (var source in sources)
            if (source?.Value != null) source.Value.Changed -= OnAnyChanged;
    }

    private void OnAnyChanged(T _) => Reevaluate();

    protected abstract void Reevaluate();

    // Convenience accessors for subclasses.
    protected IEnumerable<T> Values()
    {
        foreach (var s in sources)
            if (s?.Value != null) yield return s.Value.Value;
    }

    protected int SourceCount
    {
        get
        {
            int n = 0;
            foreach (var source in sources) if (source?.Value != null) n++;
            return n;
        }
    }
}