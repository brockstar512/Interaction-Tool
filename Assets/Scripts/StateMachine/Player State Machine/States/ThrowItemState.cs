using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowItemState : PlayerBaseState
{
    public override float Speed { get { return 4; } }


    public override void EnterState(PlayerStateMachineManager stateManager)
    {
        stateManager.item.Interact(stateManager);
    }
    public override void UpdateState(PlayerStateMachineManager stateManager)
    {

    }
    public override void OnCollisionEnter(PlayerStateMachineManager stateManager, Collision collision)
    {

    }

    public override void ExitState(PlayerStateMachineManager stateManager)
    {
        
    }

    public override void FixedUpdateState(PlayerStateMachineManager stateManager)
    {
        base.Move(stateManager);
    }

    public override void Action(PlayerStateMachineManager stateManager)
    {
        stateManager.item.Release(stateManager);
        stateManager.SwitchState(stateManager.defaultState);
    }
}
