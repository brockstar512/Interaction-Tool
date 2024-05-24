using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideItemState : PlayerBaseState
{
    AnimationKick KickAnimation;

    public SlideItemState()
    {
        KickAnimation = new AnimationKick();
    }

    public override void EnterState(PlayerStateMachineManager stateManager)
    {
        Debug.Log("Sliding item");
        //animate
        stateManager.item.Interact(stateManager);
        //if item is slidable and cannot move stub toe?
        stateManager.SwitchState(stateManager.defaultState);
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


    }
}
