using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Interactable;

public class UseItemState : PlayerBaseState, IButtonUp
{
    //maybe this has the callvack
    private Action _buttonUp;
    public override void EnterState(PlayerStateMachineManager stateManager)
    {
        Action(stateManager);
    }

    public override void UpdateState(PlayerStateMachineManager stateManager)
    {
        base.UpdateLookDirection(stateManager.movement);
    }

    public override void OnCollisionEnter(PlayerStateMachineManager stateManager, Collision collision)
    {

    }

    public override void ExitState(PlayerStateMachineManager stateManager)
    {

    }

    public override void FixedUpdateState(PlayerStateMachineManager stateManager)
    {
       
    }
    
    public override void Action(PlayerStateMachineManager stateManager)
    {
        IItem item = stateManager.itemManager.GetItem();
        
        if(item != null)
        {
            if (item is IButtonUp needsButtonUpInvoker)
            {
                //if the item needs to be notified when the button is up...
                //this assigns the invoker here...
                //when the delegate is invoked.. whether it is null or not it will be fired.
                //the item needs to inhereit the interface to take responsibilty for how it 
                //responds to button up events
                _buttonUp = needsButtonUpInvoker.ButtonUp;
            }
            item.Use(stateManager);
        }
        else
        {
            stateManager.SwitchState(stateManager.defaultState);
        }
    }

    
    //this needs to stop the items where the buttons are held down
    public void ButtonUp()
    {
        Debug.Log($"Button up");
        _buttonUp?.Invoke();
        _buttonUp = null;
    }



    
}
