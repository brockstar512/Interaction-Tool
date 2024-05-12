using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultState : PlayerBaseState
{
    AnimationMoveState MoveAnimation;
    //states hold the animation classes
    //animation classes hold what animations they do

    public DefaultState()
    {
        MoveAnimation = new AnimationMoveState();
    }


    public override void EnterState(PlayerStateMachineManager stateManager)
    {

    }

    public override void UpdateState(PlayerStateMachineManager stateManager)
    {

    }

    public override void OnCollisionEnter(PlayerStateMachineManager stateManager, Collision collision)
    {

    }

    public override void FixedUpdateState(PlayerStateMachineManager stateManager)
    {
        base.Move(stateManager);
        MoveAnimation.Play(stateManager);
    }

    public override void ExitState(PlayerStateMachineManager stateManager)
    {
        
    }

    public override void Action(PlayerStateMachineManager stateManager)
    {
        switch (stateManager.item)
        {
            case Throwable throwable:
                stateManager.SwitchState(stateManager.throwItemState);
                break;
            case Moveable moveable:
                stateManager.SwitchState(stateManager.moveItemState);
                break;
            case Slidable moveable:
                stateManager.SwitchState(stateManager.slideItemState);
                break;
            case Pickupable pickupable:
                stateManager.SwitchState(stateManager.equipItemState);
                break;
            default:
                Debug.Log("is defualt");
                break;

        }

    }
}
