using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Player.StateMachine
{
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
            if (movement == Vector2.up)
            {
                LookDirection = movement;
            }
            if (movement == Vector2.down)
            {
                LookDirection = movement;
            }
            if (movement == Vector2.right)
            {
                LookDirection = movement;
            }
            if (movement == Vector2.left)
            {
                LookDirection = movement;
            }
           //Debug.Log(LookDirection);

        }
    }
}
