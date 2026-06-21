using UnityEngine;

namespace IT.Player.StateMachine.States
{
    using IT.Animation.Player.States;
    using IT.Interactables;

    public class PlayerIdleState : PlayerStateBase
    {
        MoveAnimState MoveAnimation;
    
        public PlayerIdleState()
        {
            MoveAnimation = new MoveAnimState();
        }


        public override void EnterState(PlayerStateMachine stateManager)
        {

        }

        public override void UpdateState(PlayerStateMachine stateManager)
        {
            base.UpdateLookDirection(stateManager.movement);
        }
    
        public override void FixedUpdateState(PlayerStateMachine stateManager)
        {
            base.Move(stateManager);
            MoveAnimation.Play(stateManager);
        }

        public override void ExitState(PlayerStateMachine stateManager)
        {
        
        }

        public override void Action(PlayerStateMachine stateManager)
        {
            if (stateManager.item == null)
            {
                Debug.Log("is default");
                return;
            }

            switch (stateManager.item.Kind)
            {
                case InteractionType.Throw:
                    stateManager.SwitchState(stateManager.throwItemState);
                    break;
                case InteractionType.Move:
                    stateManager.SwitchState(stateManager.moveItemState);
                    break;
                case InteractionType.Slide:
                    stateManager.SwitchState(stateManager.slideItemState);
                    break;
                case InteractionType.Equip:
                    stateManager.SwitchState(stateManager.equipItemState);
                    break;
                case InteractionType.Open:
                    stateManager.SwitchState(stateManager.PlayerOpenState);
                    break;
                case InteractionType.Pull:
                    stateManager.SwitchState(stateManager.pullItemState);
                    break;
                case InteractionType.Possess:
                    // Story 3.4: hand off to the interactable directly — no PlayerPossessState.
                    // VehicleInteractable.Interact grabs the PlayerWrapper via context.Transform
                    // and calls PossessVehicle, which defers the controller swap to the next
                    // PlayerWrapper.Update (avoids re-entrancy NRE since we are inside Tick here).
                    stateManager.item.Interact(stateManager);
                    break;
                default:
                    Debug.Log("is default");
                    break;
            }
        }

    }
}
