using System;
using System.Collections.Generic;
using UnityEngine;
using Items;

public class ItemManager 
{
    private int _currentIndex = 0;
    private List<IItem> inventory;
    private int inventoryLimit;
    public event Action<Sprite> ItemSwitch;


    //pass in hud and get the functions that update hud
    public ItemManager()
    {
       _currentIndex = 0;
       inventory = new List<IItem>();
       inventoryLimit = 2;
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

}
