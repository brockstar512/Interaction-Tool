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
            throw new System.NotImplementedException();
        }

        public async override void EnterState(PlayerStateMachine stateManager)
        {
            if (!stateManager.item.Interact(stateManager))
            {
                stateManager.SwitchState(stateManager.defaultState);
                return;
            }
            await EquipItemAnimation.Play(stateManager);
            stateManager.SwitchState(stateManager.defaultState);
        }

        public override void ExitState(PlayerStateMachine stateManager)
        {

        }

        public override void FixedUpdateState(PlayerStateMachine stateManager)
        {

        }

        public override void OnCollisionEnter(PlayerStateMachine stateManager, Collision collision)
        {

        }

        public override void UpdateState(PlayerStateMachine stateManager)
        {
            UpdateLookDirection(stateManager.movement);
        }



    
    }
}
