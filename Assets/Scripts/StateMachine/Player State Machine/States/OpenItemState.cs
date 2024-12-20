using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenItemState : PlayerBaseState
{


    public override void EnterState(PlayerStateMachineManager stateManager)
    {
        stateManager.item.Interact(stateManager);
        stateManager.SwitchState(stateManager.defaultState);
    }

    public override void UpdateState(PlayerStateMachineManager stateManager)
    {
    }

    public override void FixedUpdateState(PlayerStateMachineManager stateManager)
    {
    }

    public override void OnCollisionEnter(PlayerStateMachineManager stateManager, Collision collision)
    {
    }

    public override void ExitState(PlayerStateMachineManager stateManager)
    {
        
    }

    public override void Action(PlayerStateMachineManager stateManager)
    {
        
    }
}
