using Interface;
using UnityEngine;

public class DefaultState : PlayerBaseState
{
    AnimationMove MoveAnimation;
    
    //hurt
    //slideing key
    public DefaultState()
    {
        MoveAnimation = new AnimationMove();
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
            case Openable openable:
                stateManager.SwitchState(stateManager.OpenItemState);
                break;
            default:
                Debug.Log("is default");
                break;

        }

    }
}
