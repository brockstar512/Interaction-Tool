using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//the hud will manage the screens
namespace IT.Player.StateMachine.States
{
    // Minimal safe death state (Story 1.4 / refactor WS3.5): stops the player and throws nowhere.
    // Full death presentation (animation, HUD, respawn) is later feature work; 0-HP entry is Story 4.2.
    public class PlayerDeathState : PlayerStateBase
    {
        public override void EnterState(PlayerStateMachine stateManager)
        {
            // Stop residual motion; input is already gated (UseItem/Interact only fire from idle).
            if (stateManager.rb != null) stateManager.rb.linearVelocity = Vector2.zero;
            Debug.Log("Player died");
        }

        public override void UpdateState(PlayerStateMachine stateManager) { }

        public override void FixedUpdateState(PlayerStateMachine stateManager) { }   // no Move → player stays put

        public override void ExitState(PlayerStateMachine stateManager) { }

        public override void Action(PlayerStateMachine stateManager) { }
    }
}
