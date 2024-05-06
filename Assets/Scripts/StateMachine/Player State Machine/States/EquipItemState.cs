using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipItemState : PlayerBaseState
{
    public override void Action(PlayerStateMachineManager stateManager)
    {
        throw new System.NotImplementedException();
    }

    public override void EnterState(PlayerStateMachineManager stateManager)
    {
        //this interactable item is suppose to determine the players inventory situation
        stateManager.item.Interact(stateManager);
    }

    public override void ExitState(PlayerStateMachineManager stateManager)
    {

    }

    public override void FixedUpdateState(PlayerStateMachineManager stateManager)
    {

    }

    public override void OnCollisionEnter(PlayerStateMachineManager stateManager, Collision collision)
    {

    }

    public override void UpdateState(PlayerStateMachineManager stateManager)
    {

    }

    
}
