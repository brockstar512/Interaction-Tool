using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItem
{
    public Sprite Sprite { get; }
    
    //when the item is being use only the item can tell the state manager when to return
    public void Use(PlayerStateMachineManager stateManager);
    

}
