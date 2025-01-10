
using System;
using UnityEngine;

public interface IItemManager
{
    public event Action<Sprite> ItemSwitch;
    public IItem GetItem();

    public void PickUpItem(Pickupable holder);

    public void SwitchItem();

    public void DisposeOfCurrentItem();

    public Sprite GetCurrentSprite();
}
