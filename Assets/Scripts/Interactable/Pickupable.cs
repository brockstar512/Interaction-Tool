using UnityEngine;
using Items;

public class Pickupable : InteractableBase
{

    [SerializeField] protected Item itemPrefab;
    [HideInInspector] public Item item = null;
    SpriteRenderer sr;
    private void Awake()
    {
        item = Instantiate(itemPrefab,this.transform);
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = item.Sprite;
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
        this.sr.sprite = item.Sprite;
    }

}
