using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipItemState : PlayerBaseState
{
    AnimationEquipItem EquipItemAnimation;
    AnimationPickUp PickUpAnimation;

    public EquipItemState()
    {
        EquipItemAnimation = new AnimationEquipItem();
        PickUpAnimation = new AnimationPickUp();
    }


    public override void Action(PlayerStateMachineManager stateManager)
    {

    }

    public async override void EnterState(PlayerStateMachineManager stateManager)
    {
        //this interactable item is suppose to determine the players inventory situation
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

    }

    
}
