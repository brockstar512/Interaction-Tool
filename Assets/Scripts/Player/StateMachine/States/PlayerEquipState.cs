using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Player.StateMachine.States
{
    using IT.Animation.Player.States;

    public class PlayerEquipState : PlayerStateBase
    {
        EquipAnimState EquipItemAnimation;

        public PlayerEquipState()
        {
            EquipItemAnimation = new EquipAnimState();
        }


        public override void Action(PlayerStateMachine stateManager)
        {
            // No second-press action while equipping; ignore Interact pressed mid-equip-animation.
            // (PlayerStateMachine.Interact calls currentState.Action unconditionally, so a throw here
            //  would crash on input spam during equip — Story 1.4 AC-3 spam safety.)
        }

        public async override void EnterState(PlayerStateMachine stateManager)
        {
            if (!stateManager.item.Interact(stateManager))
            {
                stateManager.SwitchState(stateManager.defaultState);
                return;
            }
            int token = stateManager.TransitionCount;
            await EquipItemAnimation.Play(stateManager);
            if (stateManager.IsStale(token)) return;   //stale-guard (Story 1.4)
            stateManager.SwitchState(stateManager.defaultState);
        }

        public override void ExitState(PlayerStateMachine stateManager)
        {

        }

        public override void FixedUpdateState(PlayerStateMachine stateManager)
        {

        }

        public override void UpdateState(PlayerStateMachine stateManager)
        {
            UpdateLookDirection(stateManager.movement);
        }



    
    }
}
