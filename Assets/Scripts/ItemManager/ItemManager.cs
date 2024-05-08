using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public class ItemManager 
{
    private int _currentIndex = 0;//    private List<IItem> inventory;
    public Item currentItem;
    public List<IItem> inventory;
    private int inventoryLimit;
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
            //Debug.Log("No Item to get");
            return null;
        }
        //return item
        //Debug.Log("Get Item");
        return inventory[0];
    }
    public void PickUpItem(Pickupable holder)
    {
        //todo look at message next to logs and dentermin if the varibales are coorct
        currentItem = holder.Item;
        var pickup = holder.Item as Item;
        Debug.Log("picking up: " + pickup);//this was item_1
       //p
        if (inventory.Count >= inventoryLimit)
        {
            //var putdown = inventory.RemoveAt(0);
            var putdown = inventory[0] as Item;
            inventory.RemoveAt(0);
            Debug.Log("i am removing: " + putdown + " and the count is now "+ inventory.Count);
            holder.Swap(putdown);
            inventory.Add(pickup);

            var newitem = inventory[0] as Item;

            Debug.Log("I just put in " + newitem);//item 2

            var namer = inventory[0] as Item;
            Debug.Log("now its: "+namer);//item 2

            //Debug.Log("my current item is: " + currentItem.name);

            return;
        }

        inventory.Add(pickup);
        holder.PickedUp();

        //var name = inventory[0] as Item;
        //Debug.Log("now its: " + name.name);

    }

    public void SwitchItem()
    {
        Debug.Log("Switch Item");
        _currentIndex++;
    }


}
