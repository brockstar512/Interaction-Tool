using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : InteractableBase
{
    //if the inventory is full, either don't pick it up... 
    //otherwise move your current item down the list and add the new one to the front
    //this handles inventenory logic
    //
    [SerializeField] private Item item;
    public Item Item
    {
        get { return item; }
        set { item = value; }
    }
    SpriteRenderer sr;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = Item.Sprite;
    }

    public override void Interact(PlayerStateMachineManager player)
    {
        player.itemManager.PickUpItem(this);
    }

    public override void Release(PlayerStateMachineManager player)
    {
        throw new System.NotImplementedException();
    }

   

    public void PickedUp()
    {
        Destroy(this.gameObject);
    }


    public void Swap(Item newItem)
    {

        item = newItem;
        RefreshHolderUI();
    }

    private void RefreshHolderUI()
    {
        this.sr.sprite = Item.Sprite;
    }

}