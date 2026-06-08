using UnityEngine;
using DG.Tweening;
using Player.ItemOverlap;

public class Pullable : InteractableBase
{
    public override InteractionKind Kind => InteractionKind.Pull;
    [SerializeField] protected InterfaceReference<IDependent<float>> dependent;
    [SerializeField] protected Transform handle;
    [SerializeField] protected float maxDistance = 1.5f;
    [SerializeField] protected float retractTime = 0.25f;
    [SerializeField] protected OverlapMoveCheck handleCheck;

    protected Vector3 _origin;
    protected Vector2 _pullDir;
    protected float _distance;
    protected LineRenderer _line;
    


    protected void Awake()
    {
        UpdateLayerName();
        _origin = handle.position;
        _line = GetComponent<LineRenderer>();
        if (handleCheck == null) handleCheck = GetComponentInChildren<OverlapMoveCheck>();
        DrawLine();
    }

    public override bool Interact(IInteractionContext context)
    {
        _pullDir = -context.LookDirection;   // the handle follows the player as they back away
        return true;
    }

    // advance the handle up to 'requested'; returns how far it actually moved so the player can match it
    public virtual float Pull(float requested)
    {

        float applied = Mathf.Min(requested, maxDistance - _distance);
        if (applied <= 0f) return 0f;                                    // at full → player can't go further

        Vector3 nextHandle = _origin + (Vector3)(_pullDir * (_distance + applied));
        if (IsObstructed(nextHandle)) return 0f;                         // blocked → frozen, player stops too

        SetDistance(_distance + applied);
        return applied;
    }
    
    public override void Release(IInteractionContext context)
    {
        Retract();
    }

    private bool IsObstructed(Vector3 pos)
    {
        if (handleCheck == null)
        {
            Debug.LogError($"handle check not serialized");
            return false;
        }
        
        handleCheck.transform.position = pos;
        return handleCheck.DoesOverlap(pos);
    }

    private void SetDistance(float distance)
    {
        _distance = Mathf.Clamp(distance, 0f, maxDistance);
        handle.position = _origin + (Vector3)(_pullDir * _distance);
        DrawLine();
        dependent.Value?.UpdateDependentValue(_distance / maxDistance);
    }

    protected void Retract()
    {
        DOTween.To(() => _distance, x => SetDistance(x), 0f, retractTime)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    private void DrawLine()
    {
        if (_line == null) return;
        _line.positionCount = 2;
        _line.SetPosition(0, _origin);
        _line.SetPosition(1, handle.position);
    }
    
}