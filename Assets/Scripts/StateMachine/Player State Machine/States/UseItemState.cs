using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Interactable;

public class UseItemState : PlayerBaseState, IButtonUp
{
    Vector3 playerDirection = Vector3.zero;
    private bool isHoldingDown;
    //release will determine if this is pressed down or not
    public override void EnterState(PlayerStateMachineManager stateManager)
    {
        playerDirection = LookDirection;
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

    public void ButtonUp()
    {
        Debug.Log($"Button up");
    }
    
    
}
