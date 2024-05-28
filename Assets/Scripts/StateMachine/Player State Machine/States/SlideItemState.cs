using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideItemState : PlayerBaseState
{
    AnimationKick KickAnimation;
    AnimationHurtToe HurtToeAnimation;


    public SlideItemState()
    {
        KickAnimation = new AnimationKick();
        HurtToeAnimation = new AnimationHurtToe();
    }

    public override void EnterState(PlayerStateMachineManager stateManager)
    {
        Debug.Log("Sliding item");
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

    public override async void Action(PlayerStateMachineManager stateManager)
    {
        if (stateManager.item.Interact(stateManager))
        {
            await KickAnimation.Play(stateManager);
        }
        else
        {
            await KickAnimation.Play(stateManager);
            await HurtToeAnimation.Play(stateManager);
        }
        //await KickAnimation.Play(stateManager);
        stateManager.SwitchState(stateManager.defaultState);
    }
}
