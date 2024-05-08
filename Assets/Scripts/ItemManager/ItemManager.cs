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
        var pick = holder.Item as Item;
        Debug.Log("picking up its: " + pick.name);//this was item_1
        //var namer = holder.Item as Item;
        //Debug.Log($"looking at {namer}");
        if (inventory.Count >= inventoryLimit)
        {
            //inventory.Clear();
            //var namer = inventory[0] as Item;
            //am i just keep moving item to the first index
            //Debug.Log($"Picking item {holder.Item.name} I am holding {namer.name}");


            var leavingItem = inventory[0] as Item;


            holder.Swap(leavingItem);
            //inventory.Insert(0, holder.Item);

            //inventory.Clear();
            inventory.Add(holder.Item);

            var newitem = inventory[0] as Item;

            Debug.Log("I just put in " + newitem.name);//item 2

            var namer = inventory[0] as Item;
            Debug.Log("now its: "+namer.name);//item 2

            Debug.Log("my current item is: " + currentItem.name);

            return;
        }
        //Debug.Log("PICKUP");
        //inventory.Insert(0, holder.Item);
        inventory.Add(holder.Item);
        holder.PickedUp();

        var name = inventory[0] as Item;
        Debug.Log("now its: " + name.name);

    }

    public void SwitchItem()
    {
        Debug.Log("Switch Item");
        _currentIndex++;
    }


}
