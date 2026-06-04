using DG.Tweening;
using UnityEngine;
using Player.ItemOverlap;

public class Moveable : InteractableBase
{
    public override InteractionKind Kind => InteractionKind.Move;

    [SerializeField] private Utilities.KeyTypes key;
    [SerializeField] private float pushDistance = 1f;   // one grid unit
    [SerializeField] private float speed = 8f;          // units per second

    private OverlapMoveCheck moverCheck;
    private OverlapTargetCheck _targetCheck;
    private Tweener _pushTween;
    private bool _isMoving;
    private Vector3 _origin;

    public bool CannotMove() => moverCheck.DoesOverlap(transform.position);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        UpdateLayerName();
        moverCheck = GetComponentInChildren<OverlapMoveCheck>();
        _targetCheck = GetComponentInChildren<OverlapTargetCheck>();
    }

    public override bool Interact(IInteractionContext context)
    {
        if (_isMoving) return false;                       // mid-push, ignore

        moverCheck.SetDirectionOfOverlap(context.LookDirection);
        if (CannotMove()) return false;                    // obstruction ahead → don't move

        Push(context.LookDirection);
        return true;
    }

    private void Push(Vector2 direction)
    {
        _isMoving = true;
        _origin = transform.position;
        Vector3 destination = _origin + (Vector3)(direction * pushDistance);

        _pushTween = transform.DOMove(destination, pushDistance / speed)
            .SetLink(gameObject)              // tween dies with the object
            .OnUpdate(AbortIfPathBlocked)     // bail if something steps into the path
            .OnComplete(OnPushComplete);
    }

    private void AbortIfPathBlocked()
    {
        if (!_isMoving) return;
        if (!CannotMove()) return;            // path still clear → keep going

        _pushTween.Kill();
        transform.position = _origin;         // snap back to the starting cell
        _isMoving = false;                    // free to try again
    }

    private void OnPushComplete()
    {
        _isMoving = false;
        CleanUp();
    }

    public override void Release(IInteractionContext context) { }   // one-shot push, nothing to release

    async void CleanUp()
    {
        try
        {
            bool isPlaced = await _targetCheck.IsOnKeyPort(key);
            if (isPlaced)
            {
                _targetCheck.CleanUp();
                moverCheck.CleanUp();
                Destroy(this);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Moveable.CleanUp failed: {ex}");
        }
    }
}