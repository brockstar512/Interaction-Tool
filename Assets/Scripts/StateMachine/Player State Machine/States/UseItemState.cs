using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseItemState : PlayerBaseState
{
    //have a list of items
    //have index of current item
    //
    //this has the item...
    //this handles the animation
    //this does not handle any damage or weapon logic
    public override void EnterState(PlayerStateMachineManager stateManager)
    {
        Action(stateManager);
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
       
    }

    public override void Action(PlayerStateMachineManager stateManager)
    {
        Debug.Log("Using item");

        stateManager.SwitchState(stateManager.defaultState);
    }
}
