using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager 
{
    public ItemManager()
    {
       _currentIndex = 0;
       List<IItem> inventory = new List<IItem>();
}
    private int _currentIndex = 0;
    private List<IItem> inventory;

    public void GetItem()
    {
        //return item
        Debug.Log("Get Item");
    }
    public void PickUpItem(IItem item)
    {
        Debug.Log("Pick Up Item");
        //return item this is replacing if it is replacing
        //return the data of this so its replacement is appropriate
        inventory.Insert(0,item);

    }

    public void SwitchItem()
    {
        Debug.Log("Switch Item");
        _currentIndex++;
    }


}
