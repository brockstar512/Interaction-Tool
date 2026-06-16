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
