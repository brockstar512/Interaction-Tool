using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : InteractableBase
{
    //if the inventory is full, either don't pick it up... or swap this with you current item
    //otherwise move your current item down the list and add the new one to the front
    //this handles inventenory logic
    //[SerializeField]
    public Item Item;
    //public Item Item
    //{
    //    get { return item; }
    //    set { item = value; }
    //}
    SpriteRenderer sr;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = Item.Sprite;
    }

    public override void Interact(PlayerStateMachineManager player)
    {
        //Debug.Log("Item");
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

    private void Update()
    {
        //Debug.Log($"HEre is item {Item.name}");
        //what is placed down is correct... i don't think that what i wam holding is correct so I am placing down the wrong one

    }

    public void Swap(Item newItem)
    {
        //Debug.Log(newItem);
        //Debug.Log($"switching {newItem} for {Item} next which is what it should be now");

        //Debug.Log("-----------");

        Item = newItem;
        RefreshHolderUI();
    }

    private void RefreshHolderUI()
    {
        this.sr.sprite = Item.Sprite;
    }

}
