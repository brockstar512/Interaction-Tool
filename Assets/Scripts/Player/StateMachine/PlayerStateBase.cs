using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Player.StateMachine
{
    using IT.Core.Utilities;

    public abstract class PlayerStateBase
    {
        public Vector2 LookDirection { get; set; }
        protected virtual float Speed { get { return 5; } }
        public abstract void EnterState(PlayerStateMachine stateManager);
        public abstract void UpdateState(PlayerStateMachine stateManager);
        public abstract void FixedUpdateState(PlayerStateMachine stateManager);
        public abstract void ExitState(PlayerStateMachine stateManager);
        public abstract void Action(PlayerStateMachine stateManager);

        // Interrupt cleanup (Story 4.4 Step 3.6): called by PlayerStateMachine.ForceExitToIdle
        // when a controller swap (e.g. On-Fire) yanks the player out of this state from OUTSIDE
        // the normal input flow. Default no-op; states holding external resources override it
        // (PlayerThrowState throws the held item so it doesn't stay parented to the player). NOT
        // called on a normal SwitchState, so existing per-state transition logic is untouched.
        public virtual void OnInterrupt(PlayerStateMachine stateManager) { }

        protected virtual void Move(PlayerStateMachine stateManager)
        {
            stateManager.rb.MovePosition(stateManager.rb.position + stateManager.movement * Speed * Time.deltaTime);
        }
        protected void UpdateLookDirection(Vector2 movement)
        {
            // Only EXACT cardinal input changes facing (diagonal/zero → null → unchanged).
            // No four-way if-chain at the call site — the cardinal mapping lives in Facing (WS5.1 / Story 1.5).
            Facing? facing = FacingExtensions.FromVector(movement);
            if (facing.HasValue)
                LookDirection = facing.Value.ToVector();
        }
    }
}
