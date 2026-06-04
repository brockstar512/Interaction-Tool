using UnityEngine;
using DG.Tweening;
using Player.ItemOverlap;

public class Pullable : InteractableBase
{
    public override InteractionKind Kind => InteractionKind.Pull;

    [SerializeField] private Transform handle;
    [SerializeField] private Vector2 pullDirection = Vector2.down;
    [SerializeField] private float maxDistance = 1.5f;
    [SerializeField] private float pullSpeed = 2f;
    [SerializeField] private float retractTime = 0.25f;
    [SerializeField] private bool locksAtFullPull = true;
    [SerializeField] private GameObject dependentObject;
    [SerializeField] private LineRenderer line;
    [SerializeField] private OverlapMoveCheck handleCheck;   // sensor at the handle; auto-ignores the Player layer

    private Vector3 _origin;
    private IPullDependent _dependent;
    private float _distance;
    private bool _locked;

    private void Awake()
    {
        UpdateLayerName();
        _origin = handle.position;
        if (dependentObject != null) _dependent = dependentObject.GetComponent<IPullDependent>();
        if (handleCheck == null) handleCheck = GetComponentInChildren<OverlapMoveCheck>();
        DrawLine();
    }

    public override bool Interact(IInteractionContext context) => !_locked;
    public override void Release(IInteractionContext context) { }

    public void PullStep(float deltaTime)
    {
        if (_locked) return;

        float next = Mathf.Clamp(_distance + pullSpeed * deltaTime, 0f, maxDistance);
        Vector3 nextPos = _origin + (Vector3)(pullDirection.normalized * next);

        if (IsObstructed(nextPos)) return;   // something other than the player is in the way → don't move anything
        SetDistance(next);
    }

    public void EndPull()
    {
        if (_locked) return;

        bool atFull = _distance >= maxDistance - 0.001f;
        if (locksAtFullPull && atFull) { _locked = true; return; }
        Retract();
    }

    private bool IsObstructed(Vector3 pos)
    {
        if (handleCheck == null) return false;
        handleCheck.transform.position = pos;   // aim the sensor where the handle is about to go
        return handleCheck.DoesOverlap(pos);    // strips the Player layer, so the puller never counts
    }

    private void SetDistance(float distance)
    {
        float clamped = Mathf.Clamp(distance, 0f, maxDistance);
        if (Mathf.Approximately(clamped, _distance)) return;

        _distance = clamped;
        handle.position = _origin + (Vector3)(pullDirection.normalized * _distance);
        DrawLine();
        _dependent?.OnPullChanged(_distance / maxDistance);
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
}