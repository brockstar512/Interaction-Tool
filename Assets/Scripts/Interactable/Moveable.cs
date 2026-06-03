using UnityEngine;
using Player.ItemOverlap;

public class Moveable : InteractableBase
{
    public override InteractionKind Kind => InteractionKind.Move;

    [SerializeField] private Utilities.KeyTypes key;
    private OverlapMoveCheck moverCheck;
    public bool CannotMove()=> moverCheck.DoesOverlap(this.transform.position);
    private OverlapTargetCheck _targetCheck;

   //have drag with overlapping. change layer if dragging to ingor that layer and player
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        UpdateLayerName();
        moverCheck = GetComponentInChildren<OverlapMoveCheck>();
        _targetCheck = GetComponentInChildren<OverlapTargetCheck>();


    }


    public override bool Interact(IInteractionContext context)
    {
        return false;
        //todo rework moving like pokemon using strength later
        moverCheck.SetDirectionOfOverlap(context.LookDirection);
        Utilities.PutObjectOnLayer(Utilities.InteractingLayer, this.gameObject);
        this.transform.SetParent(context.Transform);
        rb.isKinematic = false;
        return true;
    }
    
    public override void Release(IInteractionContext context)
    {
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
        this.transform.SetParent(null);
        Utilities.PutObjectOnLayer(Utilities.InteractableLayer, this.gameObject);
        CleanUp();
    }
    
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
