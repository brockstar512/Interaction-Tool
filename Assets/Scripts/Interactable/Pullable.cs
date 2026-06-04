using UnityEngine;
using DG.Tweening;
using Player.ItemOverlap;

public class Pullable : InteractableBase
{
    public override InteractionKind Kind => InteractionKind.Pull;

    [SerializeField] private Transform handle;
    [SerializeField] private Vector2 pullDirection = Vector2.down;
    [SerializeField] private float maxDistance = 1.5f;
    [SerializeField] private float retractTime = 0.25f;
    [SerializeField] private bool locksAtFullPull = true;
    [SerializeField] private MonoBehaviour dependent;        // implements IPullDependent
    [SerializeField] private LineRenderer line;
    [SerializeField] private OverlapMoveCheck handleCheck;

    private Vector3 _origin;
    private Vector2 _grabPlayerPos;
    private IPullDependent _dependent;
    private float _distance;
    private bool _locked;

    private Vector2 PullDir => pullDirection.normalized;

    private void Awake()
    {
        UpdateLayerName();
        _origin = handle.position;
        _dependent = dependent as IPullDependent;
        if (handleCheck == null) handleCheck = GetComponentInChildren<OverlapMoveCheck>();
        DrawLine();
    }

    public override bool Interact(IInteractionContext context) => !_locked;
    public override void Release(IInteractionContext context) { }

    public void BeginPull(Vector2 playerPos) => _grabPlayerPos = playerPos;

    // returns the player's allowed position — leashed to how far the lever can actually travel
    public Vector2 Drag(Vector2 playerPos)
    {
        if (_locked) return playerPos;

        Vector2 dir = PullDir;
        float raw = Vector2.Dot(playerPos - _grabPlayerPos, dir);   // how far they've dragged along the axis
        float desired = Mathf.Clamp(raw, 0f, maxDistance);

        // don't drag the handle into an obstruction (only matters when extending)
        if (desired > _distance && IsObstructed(_origin + (Vector3)(dir * desired)))
            desired = _distance;

        SetDistance(desired);

        // leash: can't move past what the lever was allowed to do
        float excess = raw - desired;
        if (excess > 0f) playerPos -= dir * excess;
        return playerPos;
    }

    public void EndPull()
    {
        if (_locked) return;

        bool atFull = _distance >= maxDistance - 0.001f;
        if (locksAtFullPull && atFull) { _locked = true; return; }   // only locks if it was pulled all the way
        Retract();
    }

    private bool IsObstructed(Vector3 pos)
    {
        if (handleCheck == null) return false;
        handleCheck.transform.position = pos;
        return handleCheck.DoesOverlap(pos);
    }

    private void SetDistance(float distance)
    {
        float clamped = Mathf.Clamp(distance, 0f, maxDistance);
        if (Mathf.Approximately(clamped, _distance)) return;

        _distance = clamped;
        handle.position = _origin + (Vector3)(PullDir * _distance);
        DrawLine();
        _dependent?.OnPullChanged(_distance / maxDistance);   // item moves by the amount pulled
    }

    private void Retract()
    {
        DOTween.To(() => _distance, x => SetDistance(x), 0f, retractTime)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    private void DrawLine()
    {
        if (line == null) return;
        line.positionCount = 2;
        line.SetPosition(0, _origin);
        line.SetPosition(1, handle.position);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (dependent != null && dependent is not IPullDependent)
        {
            Debug.LogWarning($"{name}: '{dependent.GetType().Name}' doesn't implement IPullDependent.", this);
            dependent = null;
        }
    }
#endif
}