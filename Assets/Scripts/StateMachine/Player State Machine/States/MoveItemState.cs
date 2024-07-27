using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveItemState : PlayerBaseState
{
    protected override float Speed { get { return 2; } }
    private Vector2 LimitedMovementBounds = Vector2.zero;
    AnimationPushAndPull animationPushAndPull;
    private IDoesOverlapLayer DoesOverlapItemLayer;

    public MoveItemState()
    {
        animationPushAndPull = new AnimationPushAndPull();
    }

    public override void EnterState(PlayerStateMachineManager stateManager)
    {
        LimitedMovementBounds = LookDirection;
        animationPushAndPull.EnterPushAnimation(stateManager);
        stateManager.item.Interact(stateManager);
        //Debug.Log(DoesOverlapItemLayer == null);
       

    }

    public override void UpdateState(PlayerStateMachineManager stateManager)
    {

    }

    public override void OnCollisionEnter(PlayerStateMachineManager stateManager, Collision collision)
    {

    }

    public override void ExitState(PlayerStateMachineManager stateManager)
    {

    }

    public override void FixedUpdateState(PlayerStateMachineManager stateManager)
    {
        Move(stateManager);
    }

    protected override void Move(PlayerStateMachineManager stateManager)
    {
        Vector2 _movement = stateManager.movement;
        
        if (LimitedMovementBounds == Vector2.down || LimitedMovementBounds == Vector2.up)
        {
            _movement.x *= 0;
        }
        else if (LimitedMovementBounds == Vector2.right || LimitedMovementBounds == Vector2.left)
        {
            _movement.y *= 0;
        }
        if (stateManager.item is Moveable moveable && moveable.CannotMove(_movement))
        {
            return;
        }
        stateManager.item.rb.MovePosition(stateManager.item.rb.position + _movement * Speed * Time.deltaTime);
        stateManager.rb.MovePosition(stateManager.rb.position + _movement * Speed * Time.deltaTime);
        animationPushAndPull.Play(stateManager);
    }

    public override void Action(PlayerStateMachineManager stateManager)
    {
        animationPushAndPull.LeavePushAnimation();
        stateManager.item.Release(stateManager);
        stateManager.SwitchState(stateManager.defaultState);
    }
}
