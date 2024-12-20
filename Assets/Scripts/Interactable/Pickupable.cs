using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items;

public class Pickupable : InteractableBase
{

    [SerializeField] private Item item;
    public Item Item
    {
        get { return item; }
        set { item = value; }
    }
    SpriteRenderer sr;
    // public Sprite getSprite => sr.sprite;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = Item.Sprite;
        UpdateLayerName();
    }

    public override bool Interact(PlayerStateMachineManager player)
    {
        player.itemManager.PickUpItem(this);
        //if I need to query something in the inventory I can or a needed item in order to add
        return true;
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
