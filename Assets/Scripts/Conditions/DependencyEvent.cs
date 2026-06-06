using System;
using System.Collections.Generic;

namespace Dependencies
{
    // The tracked thing. Holds a value, raises Changed when it actually changes.
    // Knows nothing about who depends on it.
    public class DependencyEvent<T>
    {
        private T _value;

        public DependencyEvent(T value = default) => _value = value;

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
}