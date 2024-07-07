using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowItemState : PlayerBaseState
{
    protected override float Speed { get { return 4; } }

    AnimationPickUp PickUpAnimation;
    AnimationThrow ThrowAnimation;
    AnimationCarry CarryAnimation;
    AnimationState CurrentAnimation;

    public ThrowItemState()
    {
        PickUpAnimation = new AnimationPickUp();
        ThrowAnimation = new AnimationThrow();
        CarryAnimation = new AnimationCarry();
        CurrentAnimation = null;
    }

    public override async void EnterState(PlayerStateMachineManager stateManager)
    {
        //update look direction
        Debug.Log($"look direction  {LookDirection}");

        CurrentAnimation = PickUpAnimation;
        await PickUpAnimation.Play(stateManager);
        stateManager.item.Interact(stateManager);
        CurrentAnimation = CarryAnimation;
        Debug.Log("enter finished");

    }
    public override void UpdateState(PlayerStateMachineManager stateManager)
    {
        base.UpdateLookDirection(stateManager.Movement);
    }
    public override void OnCollisionEnter(PlayerStateMachineManager stateManager, Collision collision)
    {

    }

    public override void ExitState(PlayerStateMachineManager stateManager)
    {
        
    }

    public override void FixedUpdateState(PlayerStateMachineManager stateManager)
    {
        //Debug.Log("Fixed Update carry");

        //if animation is x return... or is not carry
        if (CurrentAnimation is not AnimationCarry)
            return;
        //Debug.Log("Fixed Update carry");
        base.Move(stateManager);
        CarryAnimation.Play(stateManager);
        Debug.Log("play carry");

    }

    public override async void Action(PlayerStateMachineManager stateManager)
    {
        CurrentAnimation = ThrowAnimation;
        stateManager.item.Release(stateManager);
        await ThrowAnimation.Play(stateManager);
        CurrentAnimation = null;
        stateManager.SwitchState(stateManager.defaultState);
        
    }
}
