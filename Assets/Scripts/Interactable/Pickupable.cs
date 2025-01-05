using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items;
// [RequireComponent(typeof(SpriteRenderer))]
// [RequireComponent(typeof(Rigidbody2D))]
public class Pickupable : InteractableBase
{
    //enum to make a factory for this with switch

    [SerializeField] protected Item item;
    public Item Item;

    SpriteRenderer sr;
    private void Awake()
    {
        //ScriptableObject.CreateInstance<CandleConfig>();
        Debug.Log(item.GetType());
        Item = ScriptableObject.CreateInstance<Item>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = Item.Sprite;
        UpdateLayerName();
        
        if (Item is IInitializeScriptableObject<Item> thing)
        {
            thing.Init();
        }
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

   

    public virtual void PickedUp()
    {
        Destroy(this.gameObject);
    }


    public virtual void Swap(Item newItem)
    {

        item = newItem;
        RefreshHolderUI();
    }

    private void RefreshHolderUI()
    {
        this.sr.sprite = Item.Sprite;
    }

}
