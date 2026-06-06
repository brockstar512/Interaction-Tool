using DG.Tweening;
using UnityEngine;
using Player.ItemOverlap;

public class Moveable : InteractableBase
{
    public override InteractionKind Kind => InteractionKind.Move;

    [SerializeField] private Utilities.KeyTypes key;
    private const float pushDistance = 1f;                   // one grid unit
    private const float pushSpeed = .75f;          // units/sec — lower = slower
    private const float wiggleStrength = 0.05f;  // keep it small
    private const float wiggleTime = 0.1f;

    private OverlapTargetCheck _targetCheck;
    private Collider2D _col;
    private LayerMask _obstructionMask;
    private Tween _pushTween;
    private bool _isMoving;
    private Vector3 _origin;
    private Vector3 _destination;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        UpdateLayerName();
        _targetCheck = GetComponentInChildren<OverlapTargetCheck>();

        _obstructionMask = 0;
        _obstructionMask |= 1 << Layers.SlidableObstruction;   // walls/boundaries (same place Slidable expects them)
        _obstructionMask |= 1 << Layers.Interactable;          // other blocks, doors
        _obstructionMask |= 1 << Layers.Locked;
        // _obstructionMask |= 1 << Layers.Enemy;              // add your actual enemy layer here
    }

    public override bool Interact(IInteractionContext context)
    {
        if (_isMoving) return false;

        Vector3 destination = transform.position + (Vector3)(context.LookDirection * pushDistance);
        if (IsBlocked(destination)) return false;     // target cell already occupied

        Push(destination);
        return true;
    }

    private void Push(Vector3 destination)
    {
        _isMoving = true;
        _origin = transform.position;
        _destination = destination;

        float duration = pushDistance / pushSpeed;

        _pushTween = DOTween.Sequence()
            .Append(transform.DOShakePosition(wiggleTime, wiggleStrength, 8, 90, false, true))      // wind-up wiggle
            .Append(transform.DOMove(destination, duration).SetEase(Ease.OutSine).OnUpdate(AbortIfPathBlocked))
            .SetLink(gameObject)
            .OnComplete(OnPushComplete);
    }

    private void AbortIfPathBlocked()
    {
        if (!_isMoving) return;
        if (!IsBlocked(_destination)) return;         // still clear → keep going

        _pushTween.Kill();
        transform.position = _origin;                 // snap back to the starting cell
        _isMoving = false;
    }

    private bool IsBlocked(Vector3 cell)
    {
        Vector2 size = _col.bounds.size * 0.8f;       // inset so edge-touching doesn't count
        Collider2D[] hits = Physics2D.OverlapBoxAll(cell, size, 0f, _obstructionMask);
        foreach (Collider2D hit in hits)
            if (hit != _col) return true;             // anything but ourselves blocks it
        return false;
    }

    public override void Release(IInteractionContext context) { }

    private void OnPushComplete()
    {
        CleanUp();
        Wiggle();
    }

    private void Wiggle()
    {
        transform.DOShakePosition(wiggleTime, wiggleStrength, vibrato: 8, randomness: 90, snapping: false, fadeOut: true)
            .SetLink(gameObject)
            .OnComplete(() => _isMoving = false);   // settled — free to push again
    }

    async void CleanUp()
    {
        try
        {
            bool isPlaced = await _targetCheck.IsOnKeyPort(key);
            if (isPlaced)
            {
                _targetCheck.CleanUp();
                Destroy(this);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Moveable.CleanUp failed: {ex}");
        }
    }
}