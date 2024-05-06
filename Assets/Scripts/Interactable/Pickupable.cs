using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : InteractableBase
{
    //if the inventory is full, either don't pick it up... or swap this with you current item
    //otherwise move your current item down the list and add the new one to the front
    //this handles inventenory logic
    [SerializeField] Item item;
    SpriteRenderer sr;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = item.Sprite;
    }

    public override void Interact(PlayerStateMachineManager player)
    {
        Debug.Log("Item");
        player.itemManager.PickUpItem(item);
    }


    public override void Release(PlayerStateMachineManager player)
    {
        throw new System.NotImplementedException();
    }

   
}
