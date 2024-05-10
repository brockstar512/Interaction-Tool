using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

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
       inventoryLimit = 1;
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
            var putdown = inventory[0] as Item;
            inventory.RemoveAt(0);
            holder.Swap(putdown);
            inventory.Add(pickup);
            return;
        }

        inventory.Add(pickup);
        holder.PickedUp();
    }

    public void SwitchItem()
    {
        Debug.Log("Switch Item");
        _currentIndex++;
    }

}
