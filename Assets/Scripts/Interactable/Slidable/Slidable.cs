using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Player.ItemOverlap;
using UnityEngine;
// slidable
// Time
// Symbol
[RequireComponent(typeof(SpriteRenderer))]
public class Slidable : InteractableBase, IDependencySource<bool>
{
    protected virtual Utilities.KeyTypes key => Utilities.KeyTypes.SlidingBlock;

    [SerializeField] private OverlapMoveDamageCheck moverCheckPrefab;
    [SerializeField] private OverlapTargetCheck     targetCheckPrefab;
    public LayerMask obstructionLayer;

    private const int   AnimationDelayMs   = 250;
    private const int   ContactPointCount  = 3;
    private const float SlideSpeed         = 15f;
    private const float WallEpsilon        = 1f;

    private Collider2D              _col;
    private OverlapMoveDamageCheck  _moverCheck;
    private OverlapTargetCheck      _targetCheck;
    private Tweener                 _slideAnimation;

    public override InteractionKind Kind => InteractionKind.Slide;

    // IDependencySource<bool> via composition.
    private readonly Observable<bool> _locked = new();
    public bool Value => _locked.Value;
    public event Action<bool> Changed
    {
        add    => _locked.Changed += value;
        remove => _locked.Changed -= value;
    }

    private void Awake()
    {
        obstructionLayer |= (1 << Layers.SlidableObstruction)
                          | (1 << Layers.Interactable)
                          | (1 << Layers.Locked);

        _col = GetComponent<Collider2D>();
        UpdateLayerName();
    }

    public override bool Interact(IInteractionContext context)
        => !_locked.Value && CanMove(context.LookDirection);

    public override void Release(IInteractionContext context) { }

    // ---------- movement ----------

    private bool CanMove(Vector2 direction)
    {
        ClosestContactPointHelper hit = GetClosestColliderHit(direction);
        if (hit.Col == null || IsAgainstWall(direction, hit)) return false;

        SlideItem(direction, hit);
        return true;
    }

    private ClosestContactPointHelper GetClosestColliderHit(Vector2 direction)
    {
        bool horizontal = direction.x != 0f;
        var points = new List<ClosestContactPointHelper>(ContactPointCount);

        for (int i = 0; i < ContactPointCount; i++)
        {
            var p = new ClosestContactPointHelper(direction, obstructionLayer);
            p.SetOrigin(_col, i, horizontal);
            p.SetColliderHit();
            points.Add(p);
        }
        return points.OrderBy(p => p.Distance).First();
    }

    private bool IsAgainstWall(Vector2 direction, ClosestContactPointHelper hit)
    {
        bool horizontal = direction.x != 0f;
        float selfExtent  = horizontal ? _col.bounds.extents.x : _col.bounds.extents.y;
        float otherExtent = horizontal ? hit.Col.bounds.extents.x : hit.Col.bounds.extents.y;
        return hit.Distance - (selfExtent + otherExtent) < WallEpsilon;
    }

    private async void SlideItem(Vector2 direction, ClosestContactPointHelper hit)
    {
        try
        {
            Vector3 destination = ComputeDestination(direction, hit);
            float duration = hit.Distance / SlideSpeed;

            await Task.Delay(AnimationDelayMs);

            _moverCheck = Instantiate(moverCheckPrefab,
                moverCheckPrefab.transform.position, Quaternion.identity, transform);
            _moverCheck.SetDirectionOfOverlap(-direction);
            _moverCheck.SetEmergencyStop(EmergencyStopTween);

            _targetCheck = Instantiate(targetCheckPrefab,
                transform.position + targetCheckPrefab.transform.position,
                Quaternion.identity, transform);

            _slideAnimation = transform.DOMove(destination, duration);
            _slideAnimation.onComplete = CleanUp;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Slidable.SlideItem failed: {ex}");
        }
    }

    private Vector3 ComputeDestination(Vector2 direction, ClosestContactPointHelper hit)
    {
        bool horizontal = direction.x != 0f;
        Vector3 pos          = transform.position;
        Vector3 selfExtents  = _col.bounds.extents;
        Vector3 otherExtents = hit.Col.bounds.extents;

        if (horizontal)
        {
            float buffer = (selfExtents.x + otherExtents.x) * -direction.x;
            return new Vector3(pos.x + hit.Distance * direction.x + buffer, pos.y, pos.z);
        }
        else
        {
            float buffer = (selfExtents.y + otherExtents.y) * -direction.y;
            if (direction == Vector2.down) buffer -= selfExtents.x / 2f;   // preserved quirk from original
            return new Vector3(pos.x, pos.y + hit.Distance * direction.y + buffer, pos.z);
        }
    }

    private void EmergencyStopTween()
    {
        _slideAnimation.Kill();
        CleanUp();
    }

    private async void CleanUp()
    {
        bool isPlaced = false;
        try { isPlaced = await _targetCheck.IsOnKeyPort(key); }
        catch (Exception ex) { Debug.LogError($"Slidable.CleanUp failed: {ex}"); }

        _moverCheck.CleanUp();
        _targetCheck.CleanUp();

        if (isPlaced) _locked.Value = true;   // publishes to subscribers
    }

    // ---------- contact helper ----------

    private struct ClosestContactPointHelper
    {
        public Collider2D Col;
        public float      Distance;

        private readonly Vector2   _direction;
        private readonly LayerMask _obstructionLayer;
        private Vector2            _originPoint;

        public ClosestContactPointHelper(Vector2 direction, LayerMask obstructionLayer)
        {
            _direction        = direction;
            _obstructionLayer = obstructionLayer;
            _originPoint      = Vector2.zero;
            Col               = null;
            Distance          = Mathf.Infinity;
        }

        // Spreads contact points perpendicular to motion: spread along Y when moving
        // horizontally, along X when moving vertically. Index 1 stays centered.
        public void SetOrigin(Collider2D self, int index, bool horizontal)
        {
            _originPoint = self.bounds.center;
            Vector2 offset = horizontal
                ? new Vector2(0f, self.bounds.extents.y)
                : new Vector2(self.bounds.extents.x, 0f);

            if      (index == 0) _originPoint += offset;
            else if (index == 2) _originPoint -= offset;
        }

        public void SetColliderHit()
        {
            Physics2D.queriesStartInColliders = false;
            RaycastHit2D hit = Physics2D.Raycast(_originPoint, _direction, int.MaxValue, _obstructionLayer);
            Debug.DrawRay(_originPoint, _direction, Color.blue);

            if (hit.collider == null) return;

            Col = hit.collider;
            bool horizontalDir = _direction.x != 0f;
            Distance = horizontalDir
                ? Mathf.Abs(_originPoint.x - hit.collider.transform.position.x)
                : Mathf.Abs(_originPoint.y - hit.collider.transform.position.y);
        }
    }
}