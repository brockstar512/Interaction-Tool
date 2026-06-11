
using System;
using UnityEngine;

namespace IT.Items
{
    public interface IItemManager
    {
        public event Action<Sprite> ItemSwitch;
        public IItem GetItem();

        public void PickUpItem(IItemPickUp holder);
        public void PickUpItem(IItem item);
    
        public void SwitchItem();

        public void DisposeOfCurrentItem();

        public Sprite GetCurrentSprite();
    }
}
