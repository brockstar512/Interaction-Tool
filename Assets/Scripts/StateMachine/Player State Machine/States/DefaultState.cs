using Interface;
using UnityEngine;

public class DefaultState : PlayerBaseState
{
    AnimationMove MoveAnimation;
    
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
        if (stateManager.item == null)
        {
            Debug.Log("is default");
            return;
        }

        switch (stateManager.item.Kind)
        {
            case InteractionKind.Throw:
                stateManager.SwitchState(stateManager.throwItemState);
                break;
            case InteractionKind.Move:
                stateManager.SwitchState(stateManager.moveItemState);
                break;
            case InteractionKind.Slide:
                stateManager.SwitchState(stateManager.slideItemState);
                break;
            case InteractionKind.Equip:
                stateManager.SwitchState(stateManager.equipItemState);
                break;
            case InteractionKind.Open:
                stateManager.SwitchState(stateManager.OpenItemState);
                break;
            case InteractionKind.Pull:
                stateManager.SwitchState(stateManager.pullItemState);
                break;
            default:
                Debug.Log("is default");
                break;
        }
    }

}

