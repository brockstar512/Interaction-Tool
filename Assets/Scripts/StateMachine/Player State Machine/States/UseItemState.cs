using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Interactable;

public class UseItemState : PlayerBaseState, IButtonUp
{

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
            item.Use(stateManager.transform.position, LookDirection,stateManager.SwitchState, stateManager.defaultState);
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
    }
    
    
}
