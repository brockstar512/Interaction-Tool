using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moveable : InteractableBase
{
    Vector3 GetWidth { get { return GetComponent<SpriteRenderer>().bounds.size; } }

   //have drag with overlapping. change layer if dragging to ingor that layer and player
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        UpdateLayerName();

    }

    public override bool Interact(PlayerStateMachineManager player)
    {
        this.transform.SetParent(player.transform);
        rb.isKinematic = false;
        return true;
    }


    public override void Release(PlayerStateMachineManager player)
    {
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
        this.transform.SetParent(null);
    }
}
