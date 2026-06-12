using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace IT.Player.StateMachine.States
{
    using IT.Animation.Player.States;
    using IT.Interactables;
    using IT.Items;

    public class PlayerUseState : PlayerStateBase, IButtonUp
    {
        MoveAnimState MoveAnimation;
        private Action _buttonUp;
    
        public PlayerUseState()
        {
            MoveAnimation = new MoveAnimState();
        }
        public override void EnterState(PlayerStateMachine stateManager)
        {
            Action(stateManager);
        }

        public override void UpdateState(PlayerStateMachine stateManager)
        {
            base.UpdateLookDirection(stateManager.movement);
        }

        public override void ExitState(PlayerStateMachine stateManager)
        {

        }

        public override void FixedUpdateState(PlayerStateMachine stateManager)
        {
            //if you can move with the item, move and animate
            if (stateManager.itemManager.GetItem() != null && stateManager.itemManager.GetItem().CanWalk)
            {
                base.Move(stateManager);
                MoveAnimation.Play(stateManager);
            }
       
        }
    
        public override void Action(PlayerStateMachine stateManager)
        {
            //get the item
            IItem item = stateManager.itemManager.GetItem();
            //if there is an item
            if(item != null)
            {
                //subscribe to the button up if the item does something when you release the button
                if (item is IButtonUp needsButtonUpInvoker)
                {
                    _buttonUp = needsButtonUpInvoker.ButtonUp;
                }
                //use the item
                item.Use(stateManager);
            }
            else
            {
                //if there is no item... go back to the default state
                stateManager.SwitchState(stateManager.defaultState);
            }
        }
    
        public void ButtonUp()
        {
            //invoke the function that is subscribe to the button up
            _buttonUp?.Invoke();
            _buttonUp = null;
        }
    
    }
}
