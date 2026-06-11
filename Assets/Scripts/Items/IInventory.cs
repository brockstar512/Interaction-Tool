
using System;
using UnityEngine;

namespace IT.Items
{
    public interface IInventory
    {
        public event Action<Sprite> ItemSwitch;
        public IItem GetItem();

        public void PickUpItem(IItemHolder holder);
        public void PickUpItem(IItem item);
    
        public void SwitchItem();

        public void DisposeOfCurrentItem();

        public Sprite GetCurrentSprite();
    }
}
