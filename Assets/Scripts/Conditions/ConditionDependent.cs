using System;
using System.Collections.Generic;
using UnityEngine;

public class ConditionDependent : MonoBehaviour, ICondition
{
    [Tooltip("The factors this depends on. Each must implement ICondition.")]
    [SerializeField] private List<MonoBehaviour> factors = new();
    [SerializeField] private ConditionMode mode = ConditionMode.All;
    [Tooltip("Once met, stays met and stops listening (one-shot doors etc.).")]
    [SerializeField] private bool latch;
    [Tooltip("Push the current state to responses once on start.")]
    [SerializeField] private bool fireOnStart = true;

    private readonly List<ICondition> _conditions = new();
    private IConditionResponse[] _responses;
    private bool _isMet;
    private bool _initialised;

    public bool IsMet => _isMet;
    public event Action OnChanged;

    private void Awake()
    {
        foreach (var f in factors)
            if (f is ICondition c) _conditions.Add(c);
        _responses = GetComponents<IConditionResponse>();
    }

    private void OnEnable()
    {
        if (latch && _isMet) return;
        foreach (var c in _conditions) c.OnChanged += Evaluate;
        Evaluate();
    }

    private void OnDisable() => Unsubscribe();

    private void Evaluate()
    {
        if (latch && _isMet) return;

        bool met = mode == ConditionMode.All ? AllMet() : AnyMet();
        if (_initialised && met == _isMet) return;

        bool first = !_initialised;
        _initialised = true;
        _isMet = met;

        if (!first || fireOnStart)
        {
            foreach (var r in _responses) r.OnConditionChanged(met);
            OnChanged?.Invoke();
        }

        if (latch && met) Unsubscribe();
    }

    private void Unsubscribe()
    {
        foreach (var c in _conditions) c.OnChanged -= Evaluate;
    }

    private bool AllMet()
    {
        if (_conditions.Count == 0) return false;
        foreach (var c in _conditions) if (!c.IsMet) return false;
        return true;
    }

    private bool AnyMet()
    {
        foreach (var c in _conditions) if (c.IsMet) return true;
        return false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        for (int i = 0; i < factors.Count; i++)
            if (factors[i] != null && factors[i] is not ICondition)
            {
                Debug.LogWarning($"{factors[i].name} does not implement ICondition.", this);
                factors[i] = null;
            }
    }
#endif
}