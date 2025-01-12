using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items;

public class Pickupable : InteractableBase, IItemPickUp
{

    public IItem item { get; protected set; }
    public Sprite Sprite => sr.sprite;
    private SpriteRenderer sr { get; set; }
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        item = GetComponentInChildren<IItem>();
        sr.sprite = item.Sprite;
        UpdateLayerName();
    }

    public override bool Interact(PlayerStateMachineManager player)
    {
        //equip item state calls this and this calls pickup item in item inventory
        player.itemManager.PickUpItem(this);
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
    
    public void Swap(IItem newItem)
    {
        if (item != null)
        {
            newItem.TakeChild(transform);
            item = newItem;
            RefreshHolderUI();
        }
    }

    private void RefreshHolderUI()
    {
        this.sr.sprite = item.Sprite;
    }
    
    IItem ItemFactory<T>(T item) where T : MonoBehaviour, IItem
    {
        GameObject newItemObject = new GameObject();
        T component = newItemObject.AddComponent<T>();
        return component;
    }

}
