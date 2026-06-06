using System;

namespace Dependencies
{
    // Watches one DependencyEvent. When the value satisfies the condition, runs the action.
    public class DependentEvent<T>
    {
        private readonly DependencyEvent<T> _dependency;
        private readonly Func<T, bool> _condition;
        private readonly Action<T> _action;

        public DependentEvent(DependencyEvent<T> dependency, Func<T, bool> condition, Action<T> action)
        {
            _dependency = dependency;
            _condition = condition;
            _action = action;
            _dependency.Changed += Evaluate;
        }

        private void Evaluate(T value)
        {
            if (_condition(value)) _action(value);
        }

        public void Stop() => _dependency.Changed -= Evaluate;
    }
}