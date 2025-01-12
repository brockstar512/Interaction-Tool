using UnityEngine;

public interface IItemPickUp
{
     public Sprite Sprite { get; }
     public void Swap(IItem newItem);
     public void PickedUp();
     public IItem item { get; }


}
