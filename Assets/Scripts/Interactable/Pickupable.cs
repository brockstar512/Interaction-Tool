using UnityEngine;
using Items;

public class Pickupable : InteractableBase
{

    [SerializeField] protected Item item;
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
        UpdateLayerName();
    }

    public override bool Interact(PlayerStateMachineManager player)
    {
        player.itemManager.PickUpItem(this);
        return true;
    }

    public override void Release(PlayerStateMachineManager player)
    {
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
