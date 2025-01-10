using System;
using System.Collections.Generic;
using UnityEngine;
using Items;

public class ItemManager : MonoBehaviour, IItemManager
{
    private int _currentIndex = 0;
    private List<IItem> inventory;
    private const int inventoryLimit = 2;
    public event Action<Sprite> ItemSwitch;

    public void Awake()
    {
       _currentIndex = 0;
       inventory = new List<IItem>();
    }


    public IItem GetItem()
    {
        if(inventory.Count<= 0)
        {
            return null;
        }
        return inventory[_currentIndex];
    }

    public void PickUpItem(Pickupable holder)
    {
        ItemSwitch?.Invoke(holder.Item.Sprite);
        var pickup = holder.Item;

        if (inventory.Count >= inventoryLimit)
        {
            var putdown = inventory[_currentIndex] as Item;
            inventory.RemoveAt(_currentIndex);
            holder.Swap(putdown);
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
        return inventory[_currentIndex].Sprite;
    }
    public void DisposeOfCurrentItem()
    {
        inventory.RemoveAt(_currentIndex);
        //if we are at the end and the item index is not 0 we are going to decrement the index
        //otherwise the item that was ahead will fall down to the current index
        //if it's not 0 that will help in the event that you can only have one item
        _currentIndex = _currentIndex == inventoryLimit &&_currentIndex!=0  ? _currentIndex-- : _currentIndex;
        //update UI to match the inventory
        Sprite newSprite = (_currentIndex > inventory.Count)||(inventory.Count == 0) ? null : inventory[_currentIndex].Sprite;
        ItemSwitch?.Invoke(newSprite);


    }
}
