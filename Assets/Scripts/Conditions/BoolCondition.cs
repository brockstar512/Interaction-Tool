using System;
using UnityEngine;

public class BoolCondition : MonoBehaviour, ICondition
{
    [SerializeField] private bool startMet;
    private bool _met;

    public bool IsMet => _met;
    public event Action OnChanged;

    private void Awake() => _met = startMet;

    public void Set(bool met) { if (met == _met) return; _met = met; OnChanged?.Invoke(); }
    public void Toggle() => Set(!_met);
}