using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OccupancyCondition : MonoBehaviour, ICondition
{
    [Tooltip("Which layers count (e.g. Slidable/Moveable for a pad, Enemy for a region).")]
    [SerializeField] private LayerMask accepts;
    [Tooltip("Met when EMPTY (region cleared) instead of OCCUPIED (block on pad).")]
    [SerializeField] private bool metWhenEmpty;
    [Tooltip("Empty-mode only: don't count as met until something has been present at least once.")]
    [SerializeField] private bool requirePriorOccupancy = true;

    private readonly HashSet<Collider2D> _inside = new();
    private bool _everOccupied;
    private bool _was;

    public bool IsMet => metWhenEmpty
        ? (!requirePriorOccupancy || _everOccupied) && _inside.Count == 0
        : _inside.Count > 0;

    public event Action OnChanged;

    private void Reset() => GetComponent<Collider2D>().isTrigger = true;
    private void Start() => _was = IsMet;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((accepts.value & (1 << other.gameObject.layer)) == 0) return;
        _inside.Add(other);
        _everOccupied = true;
        Notify();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_inside.Remove(other)) Notify();
    }

    private void Notify()
    {
        bool met = IsMet;
        if (met == _was) return;
        _was = met;
        OnChanged?.Invoke();
    }
}