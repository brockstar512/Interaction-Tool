using UnityEngine;

public interface IItem
{
    //helps swap and set the sprites for the hud managed by the item manager and the pickup holder sprite
    public Sprite Sprite { get; }
    //destermines if player can walk while using item
    public bool CanWalk{ get; }
    //when the item is being use only the item can tell the state manager when to return
    public void Use(PlayerStateMachineManager stateManager);
    
    //helps pass the transform of the item to set the parent
    public void TakeChild(Transform parentTransform);
}
