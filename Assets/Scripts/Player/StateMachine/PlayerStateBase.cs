using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Player.StateMachine
{
    using IT.Core.Utilities;

    public abstract class PlayerStateBase
    {
        // PB.4.5 R4.5 (ruling b): Down = the codebase's canonical neutral (FromVectorSnapped's
        // tie default). A zero LookDirection matched NOTHING in MoveAnimState.Play (no else —
        // animator never told to play, the spawn-wonky symptom) and fed a zero probe direction
        // to GetOverlapObject (PSM:139). Initializing here makes both unreachable by construction.
        public Vector2 LookDirection { get; set; } = Vector2.down;
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
            // PB.4.5 R4.5: dominant-axis mapping (was FromVector, exact-cardinal-only — sticks
            // never matched, facing never followed analog input). Zero and near-diagonal still
            // hold facing; keyboard/d-pad bit-identical. Mapping lives in Facing (WS5.1 pattern).
            Facing? facing = FacingExtensions.FromVectorDominant(movement);
            if (facing.HasValue)
                LookDirection = facing.Value.ToVector();
        }
    }
}
