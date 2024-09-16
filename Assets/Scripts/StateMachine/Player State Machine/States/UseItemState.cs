using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseItemState : PlayerBaseState
{


    public override void EnterState(PlayerStateMachineManager stateManager)
    {
        //shouldn't this own the inventory?
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
            item.Use();
        }
        
        stateManager.SwitchState(stateManager.defaultState);
    }
}
