using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player.ItemOverlap;

public class DefaultState : PlayerBaseState
{
    AnimationMove MoveAnimation;
    
    // private OverlapObjectCheck overlapObjectCheck;
    
    public DefaultState()
    {
    }


    public override void EnterState(PlayerStateMachineManager stateManager)
    {

    }

    public override void UpdateState(PlayerStateMachineManager stateManager)
    {
        base.UpdateLookDirection(stateManager.movement);
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
        //Debug.Log($"is {stateManager.item}");

        switch (stateManager.item)
        {
            case Throwable throwable:
                stateManager.SwitchState(stateManager.ThrowItemState);
                break;
            case Moveable moveable:
                stateManager.SwitchState(stateManager.MoveItemState);
                break;
            case Slidable moveable:
                stateManager.SwitchState(stateManager.SlideItemState);
                break;
            case Pickupable pickupable:
                stateManager.SwitchState(stateManager.EquipItemState);
                break;
            default:
                Debug.Log("is defualt");
                break;

        }

    }
}
