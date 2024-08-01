using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player.ItemOverlap;

public class Moveable : InteractableBase
{
    Vector3 GetWidth { get { return GetComponent<SpriteRenderer>().bounds.size; } }
    private OverlapMoveCheck moverCheck;
    public bool CannotMove()=> moverCheck.DoesOverlap(this.transform.position);

   //have drag with overlapping. change layer if dragging to ingor that layer and player
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        UpdateLayerName();
        moverCheck = GetComponentInChildren<OverlapMoveCheck>();

    }

    public override bool Interact(PlayerStateMachineManager player)
    {
        Debug.Log(player.currentState.LookDirection);

        moverCheck.SetDirectionOfOverlap(player.currentState.LookDirection);
        Utilities.PutObjectOnLayer(Utilities.InteractingLayer,this.gameObject);
        this.transform.SetParent(player.transform);
        rb.isKinematic = false;
        
        return true;
    }
    
    


    public override void Release(PlayerStateMachineManager player)
    {
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
        this.transform.SetParent(null);
        Utilities.PutObjectOnLayer(Utilities.InteractableLayer, this.gameObject);
    }


}
