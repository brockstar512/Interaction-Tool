using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseItemState : PlayerBaseState
{
    Vector3 playerDirection = Vector3.zero;
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
//to do should actions be private... should all states functions be private except enter and maybe exit
    public override void Action(PlayerStateMachineManager stateManager)
    {
        IItem item = stateManager.itemManager.GetItem();
        if(item != null)
        {
            item.Use(stateManager.transform.position, LookDirection);
        }
        
        stateManager.SwitchState(stateManager.defaultState);
    }
}
