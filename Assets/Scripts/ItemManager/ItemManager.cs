using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.WSA;

//this should be in charge of invoking the item change event
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
        return inventory[0];
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
            inventory.Add(pickup);
            return;
        }

        inventory.Add(pickup);
        holder.PickedUp();
    }

    public void SwitchItem()
    {
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