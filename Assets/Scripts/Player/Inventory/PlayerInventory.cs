using System;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Player.Inventory
{
    using IT.Items;

    public class PlayerInventory : MonoBehaviour, IInventory
    {
        private int _currentIndex = 0;
        private List<IItem> inventory;
        private const int inventoryLimit = 2;
        public event Action<Sprite> ItemSwitch;
        private ItemDropper _itemDropper;

        public void Awake()
        {
           _currentIndex = 0;
           _itemDropper = GetComponent<ItemDropper>();
           inventory = new List<IItem>();
        }


        public IItem GetItem()
        {
            if (inventory.Count == 0) return null;
            _currentIndex = Mathf.Clamp(_currentIndex, 0, inventory.Count - 1);
            return inventory[_currentIndex];
        }

        // --- Story PB.3 persistence surface. Read-only enumeration in SLOT ORDER for
        // ItemStateRegistry.Capture; the backing list stays private (mirrors PB.2's
        // StatusController.Active). Nothing else in the game reads these. ---

        public IReadOnlyList<IItem> Items => inventory;

        // The held slot — captured into PlayerStateDTO.currentItemIndex. This is INVENTORY
        // state, deliberately independent of PlayerRoster (identity/lifecycle is PB.4).
        public int CurrentIndex => _currentIndex;

        // Restore the held slot after the items themselves are back. Clamped, because a
        // save may name a slot this inventory no longer has (fail-alive).
        public void RestoreCurrentIndex(int index)
        {
            _currentIndex = inventory.Count == 0 ? 0 : Mathf.Clamp(index, 0, inventory.Count - 1);
            ItemSwitch?.Invoke(GetCurrentSprite());
        }
   
        public void PickUpItem(IItemHolder holder)
        {
            //pickable calls this when the state manager interacts with it
            //item switch updates the sprite in the HUD
            ItemSwitch?.Invoke(holder.Sprite);
            //this gets a reference to the item
            var pickup = holder.item;
            //make this a parent of the item object now
            pickup.TakeChild(transform);
            if (inventory.Count >= inventoryLimit)
            {
                var displaced = inventory[_currentIndex] as ItemBase;
                inventory.RemoveAt(_currentIndex);
                holder.Swap(displaced);
                inventory.Insert(_currentIndex, pickup);
                return;
            }

            inventory.Add(pickup);
            _currentIndex = inventory.IndexOf(pickup);
            holder.PickedUp();
        }
    
        public void SwitchItem()
        {
            if (inventory.Count <= 0)
                return;
            if(_currentIndex + 1 >= inventory.Count)
            {
                _currentIndex = 0;
            }
            else
            {
                _currentIndex++;
            }

            ItemSwitch?.Invoke(inventory[_currentIndex].Sprite);

        }

        public Sprite GetCurrentSprite()
        {
            if (inventory.Count == 0) return null;
            return inventory[_currentIndex].Sprite;
        }
        public void DisposeOfCurrentItem()
        {
            inventory.RemoveAt(_currentIndex);
            if (_currentIndex >= inventory.Count && _currentIndex > 0)
                _currentIndex--;
            Sprite newSprite = inventory.Count == 0 ? null : inventory[_currentIndex].Sprite;
            ItemSwitch?.Invoke(newSprite);
        }
    
        public void PickUpItem(IItem item)
        {
            item.TakeChild(transform);
            ItemSwitch?.Invoke(item.Sprite);

            if (inventory.Count >= inventoryLimit)
            {
                ItemBase displaced = inventory[_currentIndex] as ItemBase;
                inventory[_currentIndex] = item;
                _itemDropper?.Drop(displaced, transform.position);
                return;
            }

            inventory.Add(item);
            _currentIndex = inventory.IndexOf(item);
        }
    
    }
}
