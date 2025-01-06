using System.Linq;
using UnityEngine;
using Items;
using Items.Scripts;

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
        //we don't need an instance of the gameobject just the instance of the class
        //so we can destory it
        DestroyAllChildren();

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
    
    void DestroyAllChildren()
    {
        foreach (Transform child in transform.Cast<Transform>().ToList())
        {
            Destroy(child.gameObject);
        }
    }

}
