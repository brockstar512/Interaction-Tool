// Assets/Scripts/Dependencies/Dependent.cs
// MonoBehaviour base for "I depend on one source." Handles subscribe,
// initial sync, and unsubscribe. Subclass overrides OnSourceChanged.
using UnityEngine;

public abstract class Dependent<T> : MonoBehaviour
{
    [SerializeField] private InterfaceReference<IDependencySource<T>> source;

    protected IDependencySource<T> Source => source?.Value;

    protected virtual void OnEnable()
    {
        if (Source == null) return;
        Source.Changed += OnSourceChanged;
        OnSourceChanged(Source.Value);
    }

    protected virtual void OnDisable()
    {
        if (Source == null) return;
        Source.Changed -= OnSourceChanged;
    }

    protected abstract void OnSourceChanged(T value);
}