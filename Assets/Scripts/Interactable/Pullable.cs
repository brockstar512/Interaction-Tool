using UnityEngine;
using DG.Tweening;
using Player.ItemOverlap;

public class Pullable : InteractableBase
{
    public override InteractionKind Kind => InteractionKind.Pull;

    [SerializeField] private Transform handle;
    [SerializeField] private float maxDistance = 1.5f;
    [SerializeField] private float retractTime = 0.25f;
    [SerializeField] private bool locksAtFullPull = true;
    [SerializeField] private MonoBehaviour dependent;     // implements IPullDependent
    [SerializeField] private LineRenderer line;
    [SerializeField] private OverlapMoveCheck handleCheck;

    private Vector3 _origin;
    private Vector2 _pullDir;
    private IPullDependent _dependent;
    private float _distance;
    private bool _locked;
    
    [SerializeField] private InterfaceReference<IPullDependent> target;


    private void Awake()
    {
        UpdateLayerName();
        _origin = handle.position;
        _dependent = dependent as IPullDependent;
        if (handleCheck == null) handleCheck = GetComponentInChildren<OverlapMoveCheck>();
        DrawLine();
    }

    public override bool Interact(IInteractionContext context)
    {
        if (_locked) return false;
        _pullDir = -context.LookDirection;   // the handle follows the player as they back away
        return true;
    }

    // advance the handle up to 'requested'; returns how far it actually moved so the player can match it
    public float Pull(float requested)
    {
        if (_locked) return 0f;

        float applied = Mathf.Min(requested, maxDistance - _distance);
        if (applied <= 0f) return 0f;                                    // at full → player can't go further

        Vector3 nextHandle = _origin + (Vector3)(_pullDir * (_distance + applied));
        if (IsObstructed(nextHandle)) return 0f;                         // blocked → frozen, player stops too

        SetDistance(_distance + applied);
        return applied;
    }

    public override void Release(IInteractionContext context)
    {
        if (_locked) return;

        bool atFull = _distance >= maxDistance - 0.001f;
        if (locksAtFullPull && atFull) { _locked = true; return; }       // only locks if pulled all the way
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
        _distance = Mathf.Clamp(distance, 0f, maxDistance);
        handle.position = _origin + (Vector3)(_pullDir * _distance);
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