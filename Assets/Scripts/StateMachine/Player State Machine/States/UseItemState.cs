using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Interactable;

public class UseItemState : PlayerBaseState, IButtonUp
{
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
                _buttonUp = needsButtonUpInvoker.ButtonUp;
            }
            item.Use(stateManager, stateManager.SwitchState, stateManager.defaultState);
        }
        else
        {
            stateManager.SwitchState(stateManager.defaultState);
        }
        
        
    }
    //this needs to stop the items where the buttons are held down
    public void ButtonUp()
    {
        //should I change it to where only this is what changes the state back to default?... no different items do diiferent things, so this tells what the item should do and the item deterines if it is done
        Debug.Log($"Button up");
        _buttonUp?.Invoke();
        _buttonUp = null;
    }
    
    
}
