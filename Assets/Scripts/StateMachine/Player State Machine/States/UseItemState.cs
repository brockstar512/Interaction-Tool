using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Interactable;
using Items.Scriptable_object_scripts_for_items;

public class UseItemState : PlayerBaseState, IButtonUp
{
    AnimationMove MoveAnimation;
    private Action _buttonUp;
    
    public UseItemState()
    {
        MoveAnimation = new AnimationMove();
    }
    public override void EnterState(PlayerStateMachineManager stateManager)
    {
        Action(stateManager);
    }

    public override void UpdateState(PlayerStateMachineManager stateManager)
    {
        base.UpdateLookDirection(stateManager.movement);
    }

    public override void OnCollisionEnter(PlayerStateMachineManager stateManager, Collision collision)
    {

    }

    public override void ExitState(PlayerStateMachineManager stateManager)
    {

    }

    public override void FixedUpdateState(PlayerStateMachineManager stateManager)
    {
        //if you can move with the item, move and animate
        if (stateManager.itemManager.GetItem().CanWalk)
        {
            base.Move(stateManager);
            MoveAnimation.Play(stateManager);
        }
       
    }
    
    public override void Action(PlayerStateMachineManager stateManager)
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
