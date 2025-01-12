using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipItemState : PlayerBaseState
{
    AnimationEquipItem EquipItemAnimation;

    public EquipItemState()
    {
        EquipItemAnimation = new AnimationEquipItem();
    }


    public override void Action(PlayerStateMachineManager stateManager)
    {
        throw new System.NotImplementedException();
    }

    public async override void EnterState(PlayerStateMachineManager stateManager)
    {
        stateManager.item.Interact(stateManager);
        await EquipItemAnimation.Play(stateManager);
        stateManager.SwitchState(stateManager.defaultState);
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
        UpdateLookDirection(stateManager.movement);
    }



    
}
