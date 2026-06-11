using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Items
{
    using IT.Interactables;

    public class PickupHolder : Interactable, IItemHolder
    {
        public override InteractionType Kind => InteractionType.Equip;
        public virtual IItem item { get; protected set; }
        public virtual Sprite Sprite => sr.sprite;
        protected SpriteRenderer sr { get; private set; }
        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            item = GetComponentInChildren<IItem>();
            ApplyItemSprite();
            UpdateLayerName();
        }

        protected virtual void ApplyItemSprite()
        {
            if (item != null) sr.sprite = item.Sprite;
        }

        public void InitAsDropHolder(ItemBase droppedItem)
        {
            droppedItem.gameObject.SetActive(true);
            droppedItem.TakeChild(transform);
            item = droppedItem;
            if (sr != null) sr.sprite = droppedItem.Sprite;
            UpdateLayerName();
        }


        public override bool Interact(IInteractionContext context)
        {
            context.Items.PickUpItem(this);
            return true;
        }

        public override void Release(IInteractionContext context)
        {
            throw new System.NotImplementedException();
        }
    
        public virtual void PickedUp()
        {
            Destroy(this.gameObject);
        }
    
        public virtual void Swap(IItem newItem)
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
    
    }
}
