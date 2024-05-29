using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowItemState : PlayerBaseState
{
    public override float Speed { get { return 4; } }

    AnimationPickUp PickUpAnimation;
    AnimationThrow ThrowAnimation;
    AnimationCarry CarryAnimation;

    public ThrowItemState()
    {
        PickUpAnimation = new AnimationPickUp();
        ThrowAnimation = new AnimationThrow();
        CarryAnimation = new AnimationCarry();
    }

    public override async void EnterState(PlayerStateMachineManager stateManager)
    {
        Debug.Log("enter start");

        await PickUpAnimation.Play(stateManager);
        stateManager.item.Interact(stateManager);
        Debug.Log("enter finished");

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
        //if animation is x return... or is not carry
        Debug.Log("Fixed Update");
        base.Move(stateManager);
    }

    public override async void Action(PlayerStateMachineManager stateManager)
    {
        await ThrowAnimation.Play(stateManager);
        stateManager.item.Release(stateManager);
        stateManager.SwitchState(stateManager.defaultState);
    }
}
