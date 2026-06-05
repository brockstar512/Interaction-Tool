using System;
using System.Collections;
using UnityEngine;

public class TimedCondition : MonoBehaviour, ICondition
{
    [SerializeField] private float delay = 3f;
    [SerializeField] private bool startOnEnable = true;
    [Tooltip("Flip back off after another 'delay' (a pulse).")]
    [SerializeField] private bool autoReset;

    private bool _met;
    public bool IsMet => _met;
    public event Action OnChanged;

    private void OnEnable() { if (startOnEnable) Begin(); }
    public void Begin() { StopAllCoroutines(); StartCoroutine(Run()); }

    private IEnumerator Run()
    {
        yield return new WaitForSeconds(delay);
        Set(true);
        if (!autoReset) yield break;
        yield return new WaitForSeconds(delay);
        Set(false);
    }

    private void Set(bool v) { if (v == _met) return; _met = v; OnChanged?.Invoke(); }
}