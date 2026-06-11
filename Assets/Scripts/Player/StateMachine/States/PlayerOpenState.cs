using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Player.StateMachine.States
{
    public class PlayerOpenState : PlayerStateBase
    {


        public override void EnterState(PlayerStateMachine stateManager)
        {
            Debug.Log("Player state");
            stateManager.item.Interact(stateManager);
            stateManager.SwitchState(stateManager.defaultState);
        }

        public override void UpdateState(PlayerStateMachine stateManager)
        {
        }

        public override void FixedUpdateState(PlayerStateMachine stateManager)
        {
        }

        public override void OnCollisionEnter(PlayerStateMachine stateManager, Collision collision)
        {
        }

        public override void ExitState(PlayerStateMachine stateManager)
        {
        
        }

        public override void Action(PlayerStateMachine stateManager)
        {
            stateManager.item.Release(stateManager);

        }
    }
}
